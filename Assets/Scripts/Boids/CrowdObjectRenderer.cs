using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BioCrowdsTechDemo
{
    public class CrowdObjectRenderer : MonoBehaviour
    {

        // Pool of agents to use
        public Agent[] agents;
        public Material[] materials;

        public CrowdManager manager;

        public int currentAgents = 0;
        public int maxAgents = 0;

        public void Init()
        {
            maxAgents = agents.Length;
            for (int i = 0; i < maxAgents; i++)
            {
                agents[i].Init();
            }
        }

        // Update is called once per frame
        public void UpdateStep()
        {
            var position = manager.position;
            var goal = manager.goals;
            for (int i = 0; i < manager.active_agents; i++)
            {
                var p = agents[i].rb.position;
                agents[i].rb.position = new Vector3(position[i].x, p.y, position[i].y);
                SetAgentColor(agents[i], goal[i]);
            }
        }

        void SetAgentColor(Agent agent, int type)
        {
            if (type == 0 & agent.color != AgentColor.WHITE) {
                var renderer = agent.GetComponentInChildren<Renderer>().material = materials[0];
                agent.color = AgentColor.WHITE;
            }
            else if(type == 1 & agent.color != AgentColor.BLUE) {
                var renderer = agent.GetComponentInChildren<Renderer>().material = materials[1];
                agent.color = AgentColor.BLUE;
            }
            else if(type == 2 & agent.color != AgentColor.RED) {
                var renderer = agent.GetComponentInChildren<Renderer>().material = materials[2];
                agent.color = AgentColor.RED;

            }
        }
    }
}

