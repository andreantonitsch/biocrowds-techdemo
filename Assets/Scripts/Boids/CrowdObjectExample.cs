using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Unity.Mathematics.math;
using Unity.Mathematics;

namespace BioCrowdsTechDemo
{
    [System.Serializable]
    public struct GroupData
    {
        public int quantity;
        public int type;
        public int region;
    }

    [System.Serializable]
    public struct Region
    {
        public float2 origin;
        public Rect area;
    }
    public class CrowdObjectExample : MonoBehaviour
    {

        public CrowdManager manager;
        public CrowdObjectRenderer renderer;
        
        // Quantity, type, region
        public GroupData[] group_data;
        public Region[] regions;

        public Agent mouse_goal;

        // Start is called before the first frame update
        void Start()
        {
            manager.Init();
            renderer.Init();
            mouse_goal.Init();
            for (int i = 0; i < group_data.Length; i++)
            {
                manager.AddGoal(regions[i].origin);
                AddAgentsToRegion(group_data[i]);
            }

        }

        // Update is called once per frame
        void Update()
        {

            // Set goal 0 to mouse pointer
            var p = mouse_goal.rb.position;
            manager.SetGoal(float2(p.x, p.z), 0);
            manager.SimulationStep(Time.deltaTime);


            renderer.UpdateStep();
        }

        //void FixedUpdate()
        //{
        //    manager.SimulationStep(Time.fixedDeltaTime);
        //}

        private void AddAgentsToRegion(GroupData data)
        {
            
            for (int i = 0; i < data.quantity; i++)
            {
                float2 p = float2(0.0f, 0.0f);
                Rect region = regions[data.region].area;
                p.x = UnityEngine.Random.Range(region.xMin, region.xMax);
                p.y = UnityEngine.Random.Range(region.yMin, region.yMax);
                var pos = p + regions[data.region].origin;
                
                manager.AddAgent(pos, data.type);
            }
        }

    }


}

