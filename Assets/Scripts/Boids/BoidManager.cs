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

    public class BoidManager : MonoBehaviour
    {
        public Transform[] goal_objects;

        [SerializeField]
        public BioParameters parameters;

        private NativeArray<float2> step;
        private NativeArray<float2> position;

        private NativeArray<Unity.Mathematics.Random> randoms;

        private NativeMultiHashMap<int, float2> samples;

        private NativeArray<float2> goal_positions;
        private NativeArray<int> goals;


        // grid of cell id -> neighboring boid ids.
        private NativeMultiHashMap<int, int> grid;
        private NativeHashSet<int> active_grid;

        public GridDimensions grid_dimensions;
        public int2[] neigh_offset = {
            int2(-1, -1),
            int2(-1, 0),
            int2(-1, 1),

            int2(0, -1),
            int2(0, 0),
            int2(0, 1),

            int2(1, -1),
            int2(1, 0),
            int2(1, 1)};
        public NativeArray<int2> offset_array;

        public int parallelBatchCount = 16;

        public Boid[] boids;
        public int active_boids;

        public Boid leader;

        // Initialize data structures.
        void Start()
        {

            grid_dimensions.set_cellsize(parameters.agent_LOS);
            var total_cells = grid_dimensions.cells.x * grid_dimensions.cells.y;

            step = new NativeArray<float2>(active_boids, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            position = new NativeArray<float2>(active_boids, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            goals = new NativeArray<int>(active_boids, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            goal_positions = new NativeArray<float2>(3, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);


            randoms = new NativeArray<Unity.Mathematics.Random>(JobsUtility.MaxJobThreadCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            for (int i = 0; i < JobsUtility.MaxJobThreadCount; i++)
            {
                var r = (uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                randoms[i] = new Unity.Mathematics.Random(r);
            }

            offset_array = new NativeArray<int2>(neigh_offset, Allocator.Persistent);
            grid = new NativeMultiHashMap<int, int>(active_boids * 18, Allocator.Persistent);
            active_grid = new NativeHashSet<int>(total_cells, Allocator.Persistent);


            samples = new NativeMultiHashMap<int, float2>(active_boids * parameters.markers * 9, Allocator.Persistent);

            active_boids = boids.Length;

            // Initialize data structures.
            for (int i = 0; i < active_boids; i++)
            {
                if (i < 192)
                    goals[i] = 2;
                if (i < 128)
                    goals[i] = 1;
                if (i < 64)
                    goals[i] = 0;

                var p = boids[i].transform.position;
                position[i] = float2(p.x, p.z);
            }
        }

        // Clean up structures.
        private void OnApplicationQuit()
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
        }

        // Update is called once per frame
        void Update()
        {
            for (int i = 0; i < active_boids; i++)
            {
                var p = boids[i].rb.position;
                boids[i].rb.position = new Vector3(position[i].x, p.y, position[i].y);
            }

            var leader_p = leader.rb.position;
            goal_positions[0] = float2(leader_p.x, leader_p.z);
            for (int i = 0; i < goal_objects.Length; i++)
            {
                var p = float2(0f, 0f);
                var go = goal_objects[i].position;
                p.x = go.x;
                p.y = go.z;
                goal_positions[i + 1] = p;

            }

        }

        // Update is called once per frame
        void FixedUpdate()
        {
            ScheduleMovement();
        }


        void ScheduleMovement()
        {

            var neighboursJob = new FillNeighboursJob
            {
                grid = grid.AsParallelWriter(),
                active_grid = active_grid.AsParallelWriter(),

                position = position,
                offset = offset_array,
                grid_dimensions = grid_dimensions
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
                deltaTime = Time.deltaTime,
                parameters = parameters,
                position = position,
                step = step
            };

            samples.Clear();
            grid.Clear();
            active_grid.Clear();
            var neighborFillHandle = neighboursJob.Schedule(active_boids, parallelBatchCount);
            neighborFillHandle.Complete();

            var cell_keys = active_grid.ToNativeArray(Allocator.TempJob);
            var samplingJob = new SpaceSamplingJob
            {
                active_cells = cell_keys,
                grid = grid,
                grid_dimensions = grid_dimensions,
                parameters = parameters,
                randoms = randoms,
                positions = position,
                samples = samples.AsParallelWriter()
            };

            var samplingHandle = samplingJob.Schedule(cell_keys.Length, parallelBatchCount, neighborFillHandle);
            var weightHandle = weightJob.Schedule(active_boids, parallelBatchCount, samplingHandle);
            var positionHandle = positionJob.Schedule(active_boids, parallelBatchCount, weightHandle);
            positionHandle.Complete();

            cell_keys.Dispose();
        }

        [BurstCompile]
        public struct FillNeighboursJob : IJobParallelFor
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
        public struct SpaceSamplingJob : IJobParallelFor
        {
            // Random in jobs stuff
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

                //TODO can i invert this loop?
                for (int i = 0; i < parameters.markers; i++)
                {

                    var marker = cell_corner + random.NextFloat2() * grid_dimensions.cell_size ;
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

                randoms[_threadId] = random;
            }
        }


        /// <summary>
        /// Per agent sample aggregation
        /// </summary>
        [BurstCompile]
        public struct WeightAggregation : IJobParallelFor
        {
            [WriteOnly]
            public NativeArray<float2> step;

            // Per agent space samples.
            [ReadOnly]
            public NativeMultiHashMap<int, float2> samples;
            [ReadOnly]
            public NativeArray<int> goals;
            [ReadOnly]
            public NativeArray<float2> goals_position;
            [ReadOnly]
            public NativeArray<float2> positions;

            public BioParameters parameters;

            public void Execute(int index)
            {

                var pos = positions[index];
                var goal_id = goals[index];
                var goal = goals_position[goal_id];

                float w = 0.0f;

                float2 fd_total = float2(0.0f, 0.0f);

                if (samples.TryGetFirstValue(index, out float2 sample, out var it))
                    do
                    {
                        float f = AgentFunctions.F(sample, pos, goal);
                        w += f;

                        fd_total += f * (sample - pos);

                    } while (samples.TryGetNextValue(out sample, ref it));

                float c = parameters.max_agent_speed / w;

                float2 direction = c * fd_total;

                float m = length(direction);

                if (m > parameters.max_agent_speed)
                    m = parameters.max_agent_speed;

                if (m > 0.000001)
                    step[index] = normalize(direction) * m;
                else
                    step[index] = 0.0f;
            }
        }


        [BurstCompile]
        public struct UpdatePositionJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<float2> step;

            public NativeArray<float2> position;

            public float deltaTime;
            public BioParameters parameters;

            public void Execute(int index)
            {
                var v = step[index];
                var p = position[index];

                p += v * deltaTime;

                position[index] = p;

            }
        }

            

    }
}