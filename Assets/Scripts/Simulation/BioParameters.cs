using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BioCrowdsTechDemo
{


    /// <summary>
    /// BioCrowds Simulation Parameters
    /// </summary>
    [System.Serializable]
    public struct BioParameters
    {
        /// <summary>
        /// How many markers each cell will sample per simulation step.
        /// </summary>
        [Range(0, 512)] public int markers;
        
        /// <summary>
        /// Maximum agent speed in m/s.
        /// </summary>
        [Range(0f, 10f)] public float max_agent_speed;

        /// <summary>
        /// Agent capture collision radius.
        /// An agent with a collision radius > 0.0f will not move if its radius would end outside its captured space.
        /// WARNING :: NOT CURRENTLY IMPLEMENTED.
        /// </summary>
        [Range(0f, 3f)] public float agent_radius;

        /// <summary>
        /// Agent Line of Sight.
        /// An Agent only captures a sampled space marker if the marker is within this LOS distance.
        /// </summary>
        [Range(1f, 10f)] public float agent_LOS;

        /// <summary>
        /// The minimum movement simulated movement vector length that allows agents to move in a simulation step.
        /// This controls how jittery agent movement frame-over-frame is.
        /// </summary>
        [Range(0f, 0.5f)] public float movement_epsilon;

    }
}