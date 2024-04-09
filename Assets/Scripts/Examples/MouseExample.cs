using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Unity.Mathematics.math;
using Unity.Mathematics;

namespace BioCrowdsTechDemo
{

    public class MouseExample : MonoBehaviour
    {

        public CrowdManager manager;
        public GameObject rendererObject;
        private ICrowdRenderer renderer;
        
        // Quantity, type, region
        public GroupData[] group_data;
        public Region[] regions;

        public Agent mouse_goal;

        public int AgentCapacity;

        // Start is called before the first frame update
        void Start()
        {
            manager.agent_capacity = AgentCapacity;
            //manager.SetDimensions(new float2(100, 100));
            manager.Init();

            renderer = rendererObject.GetComponent<ICrowdRenderer>();

            renderer.Init();
            mouse_goal.Init();

            AddGoals();
            FillAgents();

        }

        // Update is called once per frame
        void Update()
        {

            // Set goal 0 to mouse pointer
            var p = mouse_goal.rb.position;
            manager.SetGoal(float2(p.x, p.z), 0);

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
            for (int i = 0; i < group_data.Length; i++)
            {
                AddAgentsToRegion(group_data[i], manager.agent_count_per_goal[group_data[i].type]);
            }
        }

        private void AddAgentsToRegion(GroupData data, int current = 0)
        {
            
            for (int i = 0; i < data.quantity - current; i++)
            {
                float2 p = float2(0.0f, 0.0f);
                Rect region = regions[data.region].area;
                p.x = UnityEngine.Random.Range(region.xMin, region.xMax);
                p.y = UnityEngine.Random.Range(region.yMin, region.yMax);
                var pos =  p + new float2(regions[data.region].area.position);
                
                manager.AddAgent(pos, data.type);
            }
        }

    }


}

