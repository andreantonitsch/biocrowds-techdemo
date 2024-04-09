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
        //public float2 origin;
        public Rect area;
    }
}