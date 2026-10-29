using UnityEngine;

public class Shaker : MonoBehaviour
{
    [SerializeField] private float intensity = 0.2f;
    [SerializeField] private float shakeDurationMax = 0.1f;
    
    private float shakeTimer = 0f;
    
    private void Update()
    {
        UpdateShake();
    }

    private void UpdateShake()
    {
        if (Time.timeScale == 0f || shakeTimer <= 0f) return;
        
        shakeTimer = Mathf.Clamp(shakeTimer - Time.deltaTime, 0f, shakeDurationMax);
        
        float intensityMultiplier = shakeTimer / shakeDurationMax;
        
        Vector2 randomOffset = Random.insideUnitSphere * (intensity * intensityMultiplier);
        
        transform.localPosition = randomOffset;
    }

    public void Shake()
    {
        shakeTimer = shakeDurationMax;
    }

    public void StopShake()
    {
        shakeTimer = 0f;
        transform.localPosition = Vector3.zero;
    }
}
