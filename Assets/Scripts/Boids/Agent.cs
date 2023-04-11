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
        public Renderer renderer;

        // Start is called before the first frame update
        public void Init()
        {
            renderer = GetComponentInChildren<Renderer>();
            rb = GetComponent<Rigidbody>();

        }

    }



}