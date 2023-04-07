using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyPostProcessing : MonoBehaviour
{

    public Material PostProcessingMaterial;


    private void Start()
    {
        var cam = GetComponent<Camera>();
        cam.depthTextureMode = cam.depthTextureMode | DepthTextureMode.DepthNormals;
    }

    protected virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, PostProcessingMaterial);
    }
}
