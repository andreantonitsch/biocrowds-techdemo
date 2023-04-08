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
        public Rigidbody rb { get; private set; }
        public AgentColor color = AgentColor.WHITE;

        // Start is called before the first frame update
        protected void Start()
        {
            rb = GetComponent<Rigidbody>();

        }

    }



}