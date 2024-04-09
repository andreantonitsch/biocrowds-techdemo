using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Unity.Mathematics.math;
using Unity.Mathematics;

namespace BioCrowdsTechDemo
{

    public class GenericExample : MonoBehaviour
    {

        public CrowdManager manager;
        public GameObject rendererObject;
        private ICrowdRenderer renderer;
        
        // Quantity, type, region
        public GroupData[] group_data;
        public Region[] regions;

        public int AgentCapacity;

        private float currentDelay = 0;
        public float agentAddDelay = 10;
        public int maxAgentsPerSpawn = 10;

        //private Dictionary<int, int> agentTypeToRegion = new Dictionary<int, int>();

        Color[] colors = { Color.blue, Color.red, Color.green, Color.cyan, Color.yellow, Color.magenta };

        // Start is called before the first frame update
        void Start()
        {
            manager.agent_capacity = AgentCapacity;
            //manager.SetDimensions(new float2(100, 100));
            manager.Init();

            renderer = rendererObject.GetComponent<ICrowdRenderer>();

            renderer.Init();

            AddGoals();
            FillAgents();

        }

        // Update is called once per frame
        void Update()
        {

            FillAgents();
            manager.SimulationStep(Time.deltaTime);
            renderer.Render();
        }


        private void AddGoals()
        {
            for (int i = 0; i < group_data.Length; i++)
            {
                manager.AddGoal(regions[i].area.position);
            }
        }

        private void FillAgents()
        {
            currentDelay -= Time.deltaTime;
            if (currentDelay > 0) return;
            currentDelay = agentAddDelay;

            for (int i = 0; i < group_data.Length; i++)
            {
                AddAgentsToRegion(group_data[i], manager.agent_count_per_goal[group_data[i].type]);
            }
        }

        private void AddAgentsToRegion(GroupData data, int current = 0)
        {
            
            for (int i = 0; i < math.min(data.quantity - current, maxAgentsPerSpawn); i++)
            {
                float2 p = float2(0.0f, 0.0f);
                Rect region = regions[data.region].area;
                p.x = UnityEngine.Random.Range(region.xMin, region.xMax);
                p.y = UnityEngine.Random.Range(region.yMin, region.yMax);
                var pos = p;
                
                manager.AddAgent(pos, data.type);
            }
        }


        private void OnDrawGizmosSelected()
        {
            
            for(int i = 0; i < regions.Length; i++)
            {
                Region r = regions[i];
                Gizmos.color = colors[i % colors.Length];
                Gizmos.DrawWireCube(new Vector3(r.area.position.x + r.area.width / 2, 0.0f, r.area.position.y + r.area.height / 2),
                                    new Vector3(r.area.width, 3.0f, r.area.height));
            }

            for (int i = 0; i < group_data.Length; i++)
            {
                GroupData g = group_data[i];
                Gizmos.color = colors[i % colors.Length];
                Region r = regions[g.region];
                Gizmos.DrawWireSphere(new Vector3(r.area.position.x, r.area.position.y),
                                      1);
            }
        }

    }


}

