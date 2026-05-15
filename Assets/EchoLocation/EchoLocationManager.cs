using UnityEngine;

[RequireComponent(typeof(Camera))]
public class EcholocationOverlay : MonoBehaviour
{
    [Tooltip("The material using the Hidden/EcholocationOverlay shader")]
    public Material overlayMaterial;
    
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        // Tell the camera to generate a depth texture
        cam.depthTextureMode |= DepthTextureMode.Depth;
    }

    // This built-in Unity function intercepts the camera's final image
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (overlayMaterial != null)
        {
            // Calculate the matrix needed to turn screen pixels back into 3D world coordinates
            Matrix4x4 viewMat = cam.worldToCameraMatrix;
            Matrix4x4 projMat = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);
            Matrix4x4 viewProjMat = projMat * viewMat;
            
            // Send it to the shader
            overlayMaterial.SetMatrix("_InverseViewProj", viewProjMat.inverse);

            // Draw the effect over the screen
            Graphics.Blit(source, destination, overlayMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}