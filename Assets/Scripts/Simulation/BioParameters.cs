using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BioCrowdsTechDemo
{


    /// <summary>
    /// BioCrowds parameters
    /// 
    ///
    /// </summary>
    [System.Serializable]
    public struct BioParameters
    {
        [Range(0, 512)] public int markers;
        [Range(0f, 10f)] public float max_agent_speed;

        [Range(0f, 3f)] public float agent_radius;
        [Range(1f, 10f)] public float agent_LOS;
        [Range(0f, 0.5f)] public float movement_epsilon;

    }
}