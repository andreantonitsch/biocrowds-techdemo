using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Unity.Mathematics.math;

namespace BioCrowdsTechDemo
{
    public class CrowdObjectRenderer : MonoBehaviour, ICrowdRenderer
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

        // Calling this Render is cute :)
        public void Render()
        {
            // agents that are wrong enabled.
            var agent_delta = max(0, manager.active_agents - currentAgents);
            currentAgents = manager.active_agents;

            //agent position and goal arrays
            var position = manager.position;
            var goal = manager.goals;

            for (int i = 0; i < manager.active_agents; i++)
            {
                var p = agents[i].rb.position;
                agents[i].rb.position = new Vector3(position[i].x, p.y, position[i].y);
                if(!agents[i].isActiveAndEnabled)
                    agents[i].gameObject.SetActive(true);
                SetAgentColor(agents[i], goal[i]);
            }


            // Goes over agents which were added in the simulation this frame.
            for (int i = 0; i < agent_delta; i++)
            {
                agents[currentAgents  - i  - 1 ].gameObject.SetActive(false);
            }


        }

        void SetAgentColor(Agent agent, int type)
        {
            if (type == 0 & agent.color != AgentColor.WHITE) {
                agent.renderer.material = materials[0];
                agent.color = AgentColor.WHITE;
            }
            else if(type == 1 & agent.color != AgentColor.BLUE) {
                agent.renderer.material = materials[1]; 
                agent.color = AgentColor.BLUE;
            }
            else if(type == 2 & agent.color != AgentColor.RED) {
                agent.renderer.material = materials[2];
                agent.color = AgentColor.RED;

            }
        }
    }
}

