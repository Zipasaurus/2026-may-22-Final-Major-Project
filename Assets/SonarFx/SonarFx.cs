//
// Sonar FX - Optimized for always-on + clean single pulses
// Original by Keijiro Takahashi
//

using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SonarFx : MonoBehaviour
{
    public enum SonarMode { Directional, Spherical }
    [SerializeField] SonarMode _mode = SonarMode.Directional;
    public SonarMode mode { get { return _mode; } set { _mode = value; } }

    [SerializeField] Vector3 _direction = Vector3.forward;
    public Vector3 direction { get { return _direction; } set { _direction = value; } }

    [SerializeField] Vector3 _origin = Vector3.zero;
    public Vector3 origin { get { return _origin; } set { _origin = value; } }

    [SerializeField] Color _baseColor = new Color(0.2f, 0.2f, 0.2f, 0);
    public Color baseColor { get { return _baseColor; } set { _baseColor = value; } }

    [SerializeField] Color _waveColor = new Color(1.0f, 0.2f, 0.2f, 0);
    public Color waveColor { get { return _waveColor; } set { _waveColor = value; } }

    [SerializeField] float _waveAmplitude = 0f;
    public float waveAmplitude { get { return _waveAmplitude; } set { _waveAmplitude = value; } }

    [SerializeField] float _waveExponent = 22.0f;
    public float waveExponent { get { return _waveExponent; } set { _waveExponent = value; } }

    [SerializeField] float _waveInterval = 20.0f;
    public float waveInterval { get { return _waveInterval; } set { _waveInterval = value; } }

    [SerializeField] float _waveSpeed = 10.0f;
    public float waveSpeed { get { return _waveSpeed; } set { _waveSpeed = value; } }

    [SerializeField] float _pulseWidth = 2.0f;
    public float pulseWidth { get { return _pulseWidth; } set { _pulseWidth = value; } }

    [SerializeField] float _ghostDuration = 1.0f;
    public float ghostDuration { get { return _ghostDuration; } set { _ghostDuration = value; } }

    [SerializeField] Color _addColor = Color.black;
    public Color addColor { get { return _addColor; } set { _addColor = value; } }

    [SerializeField] Color _ghostColor = new Color(0.2f, 0.4f, 1.0f, 0.25f);
    public Color ghostColor { get { return _ghostColor; } set { _ghostColor = value; } }

    [SerializeField] Shader shader;

    const int maxPulseCount = 8;
    float[] pulseTimes = new float[maxPulseCount];
    Vector4[] pulseOrigins = new Vector4[maxPulseCount];
    int pulseCount;

    // IDs
    int baseColorID, waveColorID, waveParamsID, waveVectorID, addColorID, ghostColorID, pulseWidthID, ghostDurationID, pulseCountID, pulseTimesID, pulseOriginsID;

    void Awake()
    {
        baseColorID      = Shader.PropertyToID("_SonarBaseColor");
        waveColorID      = Shader.PropertyToID("_SonarWaveColor");
        waveParamsID     = Shader.PropertyToID("_SonarWaveParams");
        waveVectorID     = Shader.PropertyToID("_SonarWaveVector");
        addColorID       = Shader.PropertyToID("_SonarAddColor");
        ghostColorID     = Shader.PropertyToID("_SonarGhostColor");
        pulseWidthID     = Shader.PropertyToID("_SonarPulseWidth");
        ghostDurationID  = Shader.PropertyToID("_SonarGhostDuration");
        pulseCountID     = Shader.PropertyToID("_SonarPulseCount");
        pulseTimesID     = Shader.PropertyToID("_SonarPulseTimes");
        pulseOriginsID   = Shader.PropertyToID("_SonarPulseOrigins");

        pulseCount = 0;
    }

    void OnEnable()
    {
        GetComponent<Camera>().SetReplacementShader(shader, null);
    }

    void OnDisable()
    {
        GetComponent<Camera>().ResetReplacementShader();
    }

    void Update()
    {
        // Always update base settings
        Shader.SetGlobalColor(baseColorID, _baseColor);
        Shader.SetGlobalColor(waveColorID, _waveColor);
        Shader.SetGlobalColor(addColorID, _addColor);
        Shader.SetGlobalColor(ghostColorID, _ghostColor);

        var param = new Vector4(_waveAmplitude, _waveExponent, _waveInterval, _waveSpeed);
        Shader.SetGlobalVector(waveParamsID, param);
        Shader.SetGlobalFloat(pulseWidthID, _pulseWidth);
        Shader.SetGlobalFloat(ghostDurationID, _ghostDuration);
        Shader.SetGlobalInt(pulseCountID, pulseCount);
        Shader.SetGlobalFloatArray(pulseTimesID, pulseTimes);
        Shader.SetGlobalVectorArray(pulseOriginsID, pulseOrigins);

        if (_mode == SonarMode.Directional)
        {
            Shader.DisableKeyword("SONAR_SPHERICAL");
            Shader.SetGlobalVector(waveVectorID, _direction.normalized);
        }
        else
        {
            Shader.EnableKeyword("SONAR_SPHERICAL");
            Shader.SetGlobalVector(waveVectorID, _origin);
        }
    }

    public void TriggerPulse(Vector3 origin)
    {
        if (pulseCount < maxPulseCount)
        {
            pulseTimes[pulseCount] = Time.time;
            pulseOrigins[pulseCount] = new Vector4(origin.x, origin.y, origin.z, 0);
            pulseCount++;
        }
        else
        {
            for (int i = 1; i < maxPulseCount; i++)
            {
                pulseTimes[i - 1] = pulseTimes[i];
                pulseOrigins[i - 1] = pulseOrigins[i];
            }

            pulseTimes[maxPulseCount - 1] = Time.time;
            pulseOrigins[maxPulseCount - 1] = new Vector4(origin.x, origin.y, origin.z, 0);
        }
    }
}