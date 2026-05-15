using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class AssetOpacity : MonoBehaviour
{
    [Range(0f, 1f)]
    [Tooltip("Opacity level: 0 = fully transparent, 1 = fully opaque")]
    public float opacity = 0.5f;

    private Material material;
    private Color originalColor;

    private void Start()
    {
        // Get the renderer and material
        Renderer renderer = GetComponent<Renderer>();
        material = renderer.material;

        // Store the original color
        originalColor = material.color;

        // Ensure the material is set to transparent mode if it's a standard shader
        if (material.shader.name.Contains("Standard"))
        {
            material.SetFloat("_Mode", 3); // Transparent mode
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
        }
        // For other shaders, assume they support transparency or note that manual setup is needed
    }

    private void Update()
    {
        // Update the alpha value based on the opacity slider
        Color newColor = originalColor;
        newColor.a = opacity;
        material.color = newColor;
    }
}