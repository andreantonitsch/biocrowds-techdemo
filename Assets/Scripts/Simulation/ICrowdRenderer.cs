using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BioCrowdsTechDemo
{
    public interface ICrowdRenderer
    {

        public void Init();
        // Should this be called Render?
        public void Render();
    }
}
