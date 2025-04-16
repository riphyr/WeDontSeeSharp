using UnityEngine;
namespace Studio
{
    public class ChandelierFlicker : MonoBehaviour
    {
        public Light[] bulbLights; // Assigne ici les 3 Lights des ampoules
        public float minIntensity = 0f;
        public float maxIntensity = 1.5f;
        public float flickerSpeed = 0.05f;
        private float timer = 0f;
        void Update()
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                foreach (Light bulb in bulbLights)
                {
                    if (bulb != null)
                    {
                        bulb.intensity = Random.Range(minIntensity, maxIntensity);
                    }
                }
                timer = Random.Range(flickerSpeed, flickerSpeed * 3f);
            }
        }
    }
}