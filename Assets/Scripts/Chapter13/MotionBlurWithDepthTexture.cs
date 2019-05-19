using UnityEngine;
using System.Collections;

public class MotionBlurWithDepthTexture : PostEffectsBase
{

    public Shader motionBlurShader;
    private Material motionBlurMaterial = null;

    public Material material
    {
        get
        {
            motionBlurMaterial = CheckShaderAndCreateMaterial(motionBlurShader, motionBlurMaterial);
            return motionBlurMaterial;
        }
    }

    private Camera myCamera;
    public Camera m_camera
    {
        get
        {
            if (myCamera == null)
            {
                myCamera = GetComponent<Camera>();
            }
            return myCamera;
        }
    }

    [Range(0.0f, 1.0f)]
    public float blurSize = 0.5f;

    private Matrix4x4 previousViewProjectionMatrix;

    void OnEnable()
    {
        m_camera.depthTextureMode |= DepthTextureMode.Depth;

        previousViewProjectionMatrix = m_camera.projectionMatrix * m_camera.worldToCameraMatrix;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (material != null)
        {
            material.SetFloat("_BlurSize", blurSize);

            material.SetMatrix("_PreviousViewProjectionMatrix", previousViewProjectionMatrix);
            Matrix4x4 currentViewProjectionMatrix = m_camera.projectionMatrix * m_camera.worldToCameraMatrix;
            Matrix4x4 currentViewProjectionInverseMatrix = currentViewProjectionMatrix.inverse;
            material.SetMatrix("_CurrentViewProjectionInverseMatrix", currentViewProjectionInverseMatrix);
            previousViewProjectionMatrix = currentViewProjectionMatrix;

            Graphics.Blit(src, dest, material);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}
