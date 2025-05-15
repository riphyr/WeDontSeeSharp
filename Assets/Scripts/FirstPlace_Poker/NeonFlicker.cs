using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeonFlicker : MonoBehaviour
{
    public Material neonMaterial;
    public float flickerSpeed = 0.1f;

    void Start()
    {
        StartCoroutine(Flicker());
    }

    IEnumerator Flicker()
    {
        while (true)
        {
            float intensity = Random.Range(0.5f, 2f);
            neonMaterial.SetColor("_EmissionColor", Color.red * intensity);
            yield return new WaitForSeconds(Random.Range(0.05f, flickerSpeed));
        }
    }
}

