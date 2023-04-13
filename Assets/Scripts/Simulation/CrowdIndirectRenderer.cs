using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Unity.Mathematics.math;
using  Unity.Mathematics;

namespace BioCrowdsTechDemo
{
    public class CrowdIndirectRenderer : MonoBehaviour, ICrowdRenderer
    {

        public CrowdManager manager;
        public Camera drawCamera;
        public Material instance_material;
        public Mesh instance_mesh;
        public GameObject prefab;
        

        private ComputeBuffer positionBuffer;
        private ComputeBuffer typesBuffer;

        private ComputeBuffer argsBuffer;
        private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };


        private Bounds bounds;
        public Bounds Bounds { get => bounds; set => bounds = value; }

        public void InitializeBuffers() 
        {
            // GPU arguments for indirect rendering
            argsBuffer = new ComputeBuffer(5, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

            // A float2 per agent
            positionBuffer = new ComputeBuffer(manager.agent_capacity, sizeof(float) * 2);

            // An int per agent
            typesBuffer = new ComputeBuffer(manager.agent_capacity, sizeof(int));

        }

        public void ReleaseBuffers()
        {
            argsBuffer.Release();
            positionBuffer.Release();
            typesBuffer.Release();
        }

        public void InitializeBounds()
        {
            bounds = manager.grid_dimensions.AsBounds();
        }

        void OnApplicationQuit()
        {
            ReleaseBuffers();
        }

        public void UpdateBuffers() 
        {
            //if(instanceCapacity < manager.active_agents)
            //{
            //    // Update buffer size
            //}

            // Otherwise just update positions
            // and types

            positionBuffer.SetData(manager.position);
            typesBuffer.SetData(manager.goals);

            instance_material.SetBuffer("positionBuffer", positionBuffer);
            instance_material.SetBuffer("typeBuffer", typesBuffer);

            args[0] = (uint)instance_mesh.GetIndexCount(0);
            args[1] = (uint)manager.active_agents;
            args[2] = (uint)instance_mesh.GetIndexStart(0); 
            args[3] = (uint)instance_mesh.GetBaseVertex(0);

            argsBuffer.SetData(args);

        }


        public void Init()
        {
            InitializeBounds();
            InitializeBuffers();
        }

        public void Render()
        {
            UpdateBuffers();

            // Render
            Graphics.DrawMeshInstancedIndirect(instance_mesh, 0, instance_material, bounds, argsBuffer,
                                               camera: drawCamera,
                                               castShadows:UnityEngine.Rendering.ShadowCastingMode.On);
        }


    }
}

