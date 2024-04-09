using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using static Unity.Mathematics.math;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;


namespace BioCrowdsTechDemo
{
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Jobs.LowLevel.Unsafe;

    public class CrowdManager : MonoBehaviour
    {
        /// <summary>
        /// Jank Stuff
        /// </summary>
        public NativeHashSet<int> destroyed_agents;

        [Header("Simulation Data Structures")]
        public NativeArray<float2> position;
        public NativeArray<uint> goals;

        [HideInInspector]
        public List<int> agent_count_per_goal;

        private NativeArray<float2> step;
        private NativeMultiHashMap<int, float2> samples;
        private NativeArray<float2> goal_positions;


        // grid of cell id -> neighboring boid ids.
        private NativeMultiHashMap<int, int> grid;
        private NativeHashSet<int> active_grid;

        [Header("Simulation Area")]
        //[HideInInspector]
        [SerializeField]
        private GridDimensions gridDimensions;
        private readonly int2[] neigh_offset = { int2(-1, -1), int2(-1, 0), int2(-1, 1),
            int2(0, -1), int2(0, 0), int2(0, 1),
            int2(1, -1), int2(1, 0), int2(1, 1)};
        private NativeArray<int2> offset_array;



        // Unity Jobs random Array
        private NativeArray<Unity.Mathematics.Random> randoms;

        // TODO We could add an unique ID mapping.
        // With ID + version to keep track of added agents and what not.

        /// <summary>
        /// Job Parameters
        /// </summary>
        [Header("Simulation Meta Parameters")]
        public int parallelBatchCount = 16;
        public int spaceDiscretizationBins = 100;
        public int stepIterations = 1;


        /// <summary>
        /// Quantity parameters
        /// </summary>
        [Header("Simulation Quantities Parameters")]
        public int agent_capacity = 400;
        public int goal_capacity = 5;
        public int active_goals = 0;
        public int active_agents = 0;

        [Header("BioCrowds Parameters")]
        [SerializeField]
        private BioParameters parameters;


        /// <summary>
        /// Initializes the Simulation.
        /// Parameters must be set.
        /// Initializes Data Structures.
        /// </summary>
        public void Init()
        {
            // Set up the grid math according to agent line of sight.
            //gridDimensions = new GridDimensions(new float2(100, 100));
            //gridDimensions.set_cellsize(parameters.agent_LOS * 2);
            gridDimensions.set_bins_cellsize(parameters.agent_LOS * 2, spaceDiscretizationBins);
            //var total_cells = (gridDimensions.cells.x) * (gridDimensions.cells.y);

            // Initialize grid data structures.
            // Neighborhood cell offset array.
            offset_array = new NativeArray<int2>(neigh_offset, Allocator.Persistent);
            grid = new NativeMultiHashMap<int, int>(agent_capacity * 9, Allocator.Persistent);

            // I think the hashset removes duplicates after the job. when used as a Parallel Writer.
            // So I think we have to add the maximum ossible value with duplicates
            active_grid = new NativeHashSet<int>(agent_capacity * 9, Allocator.Persistent);
            //active_grid = new NativeHashSet<int>(total_cells, Allocator.Persistent);

            // Initialize agent simulation data structures.
            step = new NativeArray<float2>(agent_capacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            position = new NativeArray<float2>(agent_capacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            goals = new NativeArray<uint>(agent_capacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            samples = new NativeMultiHashMap<int, float2>(agent_capacity * parameters.markers * 2, Allocator.Persistent);
            // initialize goal position arrays
            goal_positions = new NativeArray<float2>(goal_capacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            // Initialize one random per job thread.
            randoms = new NativeArray<Unity.Mathematics.Random>(JobsUtility.MaxJobThreadCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < JobsUtility.MaxJobThreadCount; i++)
            {
                var r = (uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                randoms[i] = new Unity.Mathematics.Random(r);
            }

            destroyed_agents = new NativeHashSet<int>(agent_capacity, Allocator.Persistent);

            agent_count_per_goal = new List<int>() ;
        }

        private void Dispose()
        {
            step.Dispose();
            position.Dispose();

            randoms.Dispose();

            samples.Dispose();

            goal_positions.Dispose();
            goals.Dispose();

            grid.Dispose();
            active_grid.Dispose();
            offset_array.Dispose();

            destroyed_agents.Dispose();
        }

        // Clean up structures.
        private void OnApplicationQuit()
        {
            Dispose();
        }

        /// <summary>
        /// Performs a Simulation step advancing deltaTime seconds.
        /// </summary>
        /// <param name="deltaTime">A delta of time to Simulate.</param>
        public void SimulationStep(float deltaTime)
        {
            for (int i = 0; i < stepIterations; i++)
            {
                deltaTime /= stepIterations;

                var neighboursJob = new FillNeighboursJob
                {
                    grid = grid.AsParallelWriter(),
                    active_grid = active_grid.AsParallelWriter(),

                    position = position,
                    offset = offset_array,
                    grid_dimensions = gridDimensions
                };

                var weightJob = new WeightAggregation
                {
                    goals = goals,
                    goals_position = goal_positions,
                    parameters = parameters,
                    positions = position,
                    samples = samples,
                    step = step
                };

                var positionJob = new UpdatePositionJob
                {
                    deltaTime = deltaTime,
                    position = position,
                    step = step
                };

                // Sort of slow
                samples.Clear();
                grid.Clear();
                active_grid.Clear();
                var neighborFillHandle = neighboursJob.Schedule(active_agents, parallelBatchCount);
                neighborFillHandle.Complete();

                var cell_keys = active_grid.ToNativeArray(Allocator.TempJob);
                var samplingJob = new SpaceSamplingJob
                {
                    active_cells = cell_keys,
                    grid = grid,
                    grid_dimensions = gridDimensions,
                    parameters = parameters,
                    randoms = randoms,
                    positions = position,
                    samples = samples.AsParallelWriter()
                };

                var samplingHandle = samplingJob.Schedule(cell_keys.Length, parallelBatchCount, neighborFillHandle);
                var weightHandle = weightJob.Schedule(active_agents, parallelBatchCount, samplingHandle);
                var positionHandle = positionJob.Schedule(active_agents, parallelBatchCount, weightHandle);
                positionHandle.Complete();

                cell_keys.Dispose();

                var markagentsJob = new MarkAgentsForDestruction
                {
                    destroyed_agents = destroyed_agents.AsParallelWriter(),
                    goals = goals,
                    position = position,
                    goal_positions = goal_positions
                };

                var markAgentsHandle = markagentsJob.Schedule(active_agents, parallelBatchCount);
                markAgentsHandle.Complete();
                if (destroyed_agents.Count() > 0)
                {
                    //Debug.Log(destroyed_agents.Count());
                    List<int> agentsToRemove = new List<int>();
                    var it = destroyed_agents.GetEnumerator();
                    while (it.MoveNext())
                    {
                        agentsToRemove.Add(it.Current);
                    }

                    RemoveAgents(agentsToRemove);

                }
                destroyed_agents.Clear();
            }
        }


        [BurstCompile]
        private struct FillNeighboursJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<float2> position;
            [ReadOnly]
            public NativeArray<int2> offset;

            [WriteOnly]
            public NativeMultiHashMap<int, int>.ParallelWriter grid;
            [WriteOnly]
            public NativeHashSet<int>.ParallelWriter active_grid;

            public GridDimensions grid_dimensions;

            public void Execute(int index)
            {

                var cell = grid_dimensions.position_to_cell(position[index]);

                for (int j = 0; j < 9; j++)
                {
                    var id = grid_dimensions.cell_to_id(cell + offset[j]);
                    grid.Add(id, index);
                    active_grid.Add(id);
                }

            }

        }

        /// <summary>
        /// Space sampling per cell.
        /// </summary>
        [BurstCompile]
        private struct SpaceSamplingJob : IJobParallelFor
        {
            // Random in Unity Jobs data.
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<Unity.Mathematics.Random> randoms;
            [NativeSetThreadIndex] private int _threadId;


            // Per agent space samples.
            [WriteOnly]
            public NativeMultiHashMap<int, float2>.ParallelWriter samples;

            [ReadOnly]
            public NativeArray<float2> positions;

            [ReadOnly]
            public NativeArray<int> active_cells;
            [ReadOnly]
            public NativeMultiHashMap<int, int> grid;

            public GridDimensions grid_dimensions;
            public BioParameters parameters;

            public void Execute(int index)
            {
                var random = randoms[_threadId];
                var cell = active_cells[index];

                var cell_corner = grid_dimensions.cell_corner(grid_dimensions.id_to_cell(cell));

                for (int i = 0; i < parameters.markers; i++)
                {

                    var marker = cell_corner + random.NextFloat2() * grid_dimensions.cell_size;
                    int closest = -1;
                    float distance = float.MaxValue;

                    if (grid.TryGetFirstValue(cell, out int agent, out var it))
                        do
                        {
                            var agent_dist = length(marker - positions[agent]);
                            if (agent_dist < distance && agent_dist < parameters.agent_LOS)
                            {
                                distance = agent_dist;
                                closest = agent;
                            }
                        } while (grid.TryGetNextValue(out agent, ref it));

                    samples.Add(closest, marker);
                }

                // Replace the random struct.
                randoms[_threadId] = random;
            }
        }


        /// <summary>
        /// Per agent sample aggregation
        /// </summary>
        [BurstCompile]
        private struct WeightAggregation : IJobParallelFor
        {
            [WriteOnly]
            public NativeArray<float2> step;

            // Per agent space samples.
            [ReadOnly]
            public NativeMultiHashMap<int, float2> samples;
            [ReadOnly]
            public NativeArray<uint> goals;
            [ReadOnly]
            public NativeArray<float2> goals_position;
            [ReadOnly]
            public NativeArray<float2> positions;

            public BioParameters parameters;

            public void Execute(int index)
            {

                var pos = positions[index];
                var goal_id = goals[index];
                var goal = goals_position[(int)goal_id];

                // Total marker weight
                float wt = 0.0f;

                // Total directional weight
                float2 fd_total = float2(0.0f, 0.0f);

                // BioCrows F sum loop.
                if (samples.TryGetFirstValue(index, out float2 sample, out var it))
                    do
                    {
                        float f = AgentFunctions.F(sample, pos, goal);
                        wt += f;

                        fd_total += f * (sample - pos);

                    } while (samples.TryGetNextValue(out sample, ref it));

                // This is a constant value, in the model sum.
                // We can do the multiplication after the loop for
                // some speed gains.
                float c = parameters.max_agent_speed / wt;
                float2 direction = c * fd_total;

                // Cap the agent speed according to max speed.
                float m = length(direction);
                if (m > parameters.max_agent_speed)
                    m = parameters.max_agent_speed;

                if (m > parameters.movement_epsilon)
                    step[index] = normalize(direction) * m;
                else
                    step[index] = float2(0.0f, 0.0f);
            }
        }


        [BurstCompile]
        private struct UpdatePositionJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<float2> step;

            public NativeArray<float2> position;

            public float deltaTime;

            public void Execute(int index)
            {
                var v = step[index];
                var p = position[index];

                p += v * deltaTime;

                position[index] = p;
            }
        }


        [BurstCompile]
        private struct MarkAgentsForDestruction : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<float2> position;
            [ReadOnly]
            public NativeArray<uint> goals;
            [ReadOnly]
            public NativeArray<float2> goal_positions;

            [WriteOnly]
            public NativeHashSet<int>.ParallelWriter destroyed_agents;

            public void Execute(int index)
            {
                var goal_id = goals[index];
                var goal = goal_positions[(int)goal_id];
                var pos = position[index];

                //if (goal_id == 0)
                //    return;

                if (length(pos - goal) < 10.0f)
                    destroyed_agents.Add(index);
            }
        }


        /// <summary>
        /// Adds an Agent of Type Type at position agent_position.
        /// An Agent 'Group' tracks and set goals for batches of agents.
        /// </summary>
        /// <param name="agent_position">The initial Agent position in world units.</param>
        /// <param name="groupID">
        /// A 'coloring' this agent.
        /// Agents of the same 'Group' track the same goals and have the same Group ID.
        /// </param>
        public void AddAgent(float2 agent_position, int groupID)
        {
            if (active_agents >= agent_capacity)
                return;

            agent_count_per_goal[groupID]++;

            position[active_agents] = agent_position;
            var t = new Vector3(agent_position.x, 1.0f, agent_position.y);

            goals[active_agents] = (uint)groupID;

            active_agents++;
        }

        /// <summary>
        /// Removes Agent by step ID.
        /// If you need to remove multiple Agents in the same Simulation Step use RemoveAgents instead.
        /// </summary>
        /// <param name="id">The ID of an Agent in the current Simulation Step.</param>
        public void RemoveAgent(int id)
        {
            active_agents--;
            agent_count_per_goal[(int)goals[id]]--;

            position[id] = position[active_agents];
            goals[id] = goals[active_agents];


        }

        /// <summary>
        /// Removes all agents in agent_indexes from the simulation.
        /// This method uses per Simulation Step indexing and does not guarantee an index acquired in a previous frame
        /// will remove the agent that owned that index before.
        /// </summary>
        /// <param name="agent_indexes">A list of Agent indexes. Indexes must be current Simulation Step indexes.</param>
        public void RemoveAgents(List<int> agent_indexes)
        {
            agent_indexes.Sort();

            for (int i = agent_indexes.Count-1; i >=0; i--)
            {
                RemoveAgent(agent_indexes[i]);
            }
        }

        /// <summary>
        /// Sets the Goal for an Agent Group.
        /// </summary>
        /// <param name="position">The Goal new position.</param>
        /// <param name="groupID">The Group ID of the Group.</param>
        public void SetGoal(float2 position, int groupID)
        {
            goal_positions[groupID] = position;
        }

        /// <summary>
        /// Adds a new Goal (and new Agent Group) to the simulation.
        /// </summary>
        /// <param name="position">The new Goal position.</param>
        /// <returns></returns>
        public int AddGoal(float2 position)
        {
            if (goal_capacity == active_goals)
                return -1;

            goal_positions[active_goals] = position;

            agent_count_per_goal.Add(0);

            active_goals++;
            return active_goals - 1;
        }

        //public void SetDimensions(float2 widthHeight)
        //{
        //    gridDimensions = new GridDimensions(widthHeight);
        //}


    }
}