using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;



namespace BioCrowdsTechDemo
{

    public enum AgentColor
    {
        WHITE,
        BLUE,
        RED
    }

    [RequireComponent(typeof(Rigidbody))]
    public class Agent : MonoBehaviour
    {
        [SerializeField]
        public Rigidbody rb { get; private set; }
        public AgentColor color = AgentColor.WHITE;

        // Start is called before the first frame update
        public void Init()
        {
            rb = GetComponent<Rigidbody>();

        }

    }



}