using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BioCrowdsTechDemo
{

    [System.Serializable]
    public struct BioParameters
    {
        [Range(0, 128)] public int markers;
        [Range(0f, 10f)] public float max_agent_speed;

        [Range(0f, 3f)] public float agent_radius;
        [Range(1f, 10f)] public float agent_LOS;
    }
}