using UnityEngine;

public class AddLight : MonoBehaviour
{
    public Light flickerLight;
    public float minIntensity = 0.8f;
    public float maxIntensity = 1.5f;
    public float flickerSpeed = 0.1f;

    void Start()
    {
        if (flickerLight == null)
            flickerLight = GetComponent<Light>();

        InvokeRepeating("Flicker", 0f, flickerSpeed);
    }

    void Flicker()
    {
        float intensity = Random.Range(minIntensity, maxIntensity);
        flickerLight.intensity = intensity;
    }
}