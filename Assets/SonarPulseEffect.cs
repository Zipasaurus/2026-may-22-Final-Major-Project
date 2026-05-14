using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class EcholocationController : MonoBehaviour
{
    [Header("Player & Positioning")]
    public Transform player;
    [Tooltip("How far down to raycast for ground")]
    public float raycastDistance = 6f;
    [Tooltip("Height above ground where the pulse appears")]
    public float groundOffset = 0.3f;

    [Header("Ring Settings")]
    [Range(0.05f, 3f)]
    public float startRadius = 0.15f;        // Very small
    public float maxRadius = 90f;
    public float speed = 42f;

    [Header("Visuals")]
    public Color ringColor = new Color(0f, 1.6f, 1.6f, 1f);
    public float mainThickness = 2.2f;
    public float ghostThickness = 10f;
    public float mainIntensity = 19f;
    public float ghostIntensity = 4.8f;

    [Header("Input")]
    public KeyCode triggerKey = KeyCode.E;

    private Camera cam;
    private Material ringMat;
    private List<Ring> activeRings = new List<Ring>();

    private struct Ring
    {
        public Vector3 center;
        public float radius;
    }

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.depthTextureMode = DepthTextureMode.Depth;

        Shader shader = Shader.Find("Hidden/EcholocationRing");
        if (shader == null)
        {
            Debug.LogError("Shader 'Hidden/EcholocationRing' not found!");
            return;
        }

        ringMat = new Material(shader);
    }

    void Update()
    {
        if (Input.GetKeyDown(triggerKey))
            TriggerPulse();

        // Update rings
        for (int i = activeRings.Count - 1; i >= 0; i--)
        {
            Ring r = activeRings[i];
            r.radius += speed * Time.deltaTime;
            activeRings[i] = r;

            if (r.radius >= maxRadius)
                activeRings.RemoveAt(i);
        }

        if (activeRings.Count > 0)
            UpdateShader();
    }

    public void TriggerPulse()
    {
        if (player == null)
        {
            Debug.LogWarning("Player transform not assigned!");
            return;
        }

        Vector3 spawnPos = GetGroundPosition();

        Ring newRing = new Ring
        {
            center = spawnPos,
            radius = startRadius
        };

        activeRings.Add(newRing);
    }

    private Vector3 GetGroundPosition()
    {
        // Start raycast from player's position + a bit up
        Vector3 origin = player.position + Vector3.up * 1f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, raycastDistance))
        {
            return hit.point + Vector3.up * groundOffset;
        }

        // Fallback - directly under player
        return player.position - Vector3.up * 0.8f;
    }

    void UpdateShader()
    {
        if (ringMat == null) return;

        int count = Mathf.Min(activeRings.Count, 8);
        Vector4[] centers = new Vector4[8];
        float[] radii = new float[8];

        for (int i = 0; i < count; i++)
        {
            centers[i] = activeRings[i].center;
            radii[i] = activeRings[i].radius;
        }

        ringMat.SetVectorArray("_RingCenters", centers);
        ringMat.SetFloatArray("_RingRadii", radii);
        ringMat.SetInt("_RingCount", count);
        ringMat.SetColor("_Color", ringColor);
        ringMat.SetFloat("_MainThickness", mainThickness);
        ringMat.SetFloat("_GhostThickness", ghostThickness);
        ringMat.SetFloat("_MainIntensity", mainIntensity);
        ringMat.SetFloat("_GhostIntensity", ghostIntensity);
        ringMat.SetFloat("_MaxRadius", maxRadius);
        ringMat.SetFloat("_FadePower", 2.4f);
        ringMat.SetFloat("_TimeFade", 2.2f);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (activeRings.Count > 0 && ringMat != null)
            Graphics.Blit(src, dest, ringMat);
        else
            Graphics.Blit(src, dest);
    }

    void OnDestroy()
    {
        if (ringMat != null) Destroy(ringMat);
    }
}