using UnityEngine;

[RequireComponent(typeof(SonarFx))]
public class SonarPulseTrigger : MonoBehaviour
{
    SonarFx sonar;

    [Header("Pulse Settings")]
    [SerializeField] float pulseAmplitude = 8f;
    [SerializeField] float waveSpeed = 25f;
    [SerializeField] float ringSize = 2f;
    [SerializeField] float ringDistance = 15f;
    [SerializeField] float ghostDuration = 1.5f;
    [SerializeField] Color pulseColor = new Color(1f, 0.4f, 1.5f, 1f);
    [SerializeField] Color ghostColor = new Color(1f, 0.4f, 1.5f, 0.25f);
    [SerializeField] Transform playerTransform;
    [SerializeField] Vector3 originOffset = new Vector3(0f, -1f, 0f);

    void Awake()
    {
        sonar = GetComponent<SonarFx>();
        sonar.enabled = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            TriggerNewPulse();
        }
    }

    public void TriggerNewPulse()
    {
        sonar.mode = SonarFx.SonarMode.Spherical;
        var origin = playerTransform != null ? playerTransform.position : transform.position;
        var pulseOrigin = origin + originOffset;
        sonar.origin = pulseOrigin;
        sonar.waveAmplitude = pulseAmplitude;
        sonar.waveSpeed = waveSpeed;
        sonar.waveInterval = ringDistance / Mathf.Max(0.0001f, waveSpeed);
        sonar.waveColor = pulseColor;
        sonar.pulseWidth = ringSize;
        sonar.ghostDuration = ghostDuration;
        sonar.ghostColor = ghostColor;
        sonar.TriggerPulse(pulseOrigin);
    }
}