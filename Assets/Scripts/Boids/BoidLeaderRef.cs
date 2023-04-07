using UnityEngine;
using static Unity.Mathematics.math;


namespace BioCrowdsTechDemo
{
    using Unity.Mathematics;
    public class BoidLeaderRef : Boid
    {

        [field: SerializeField]
        public float2 velocity { get; private set; }
        private Vector3 _lastPos;

        // Start is called before the first frame update
        void Start()
        {
            base.Start();
            _lastPos = transform.position;
        }

        new void FixedUpdate()
        {

        }

        // Update is called once per frame
        new void Update()
        {
            var delta = transform.position - _lastPos;
            velocity = float2(delta.x, delta.z);

            _lastPos = transform.position;

        }
    }


}