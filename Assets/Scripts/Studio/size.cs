using TMPro;
using UnityEngine;

public class size : MonoBehaviour
{
    public TextMeshProUGUI texte; // Pour UI
    // public TextMeshPro texte;  // Pour un objet 3D dans la scène

    public float nouvelleTaille = 40f;

    void Start()
    {
        if (texte != null)
        {
            texte.fontSize = nouvelleTaille;
        }
    }
}