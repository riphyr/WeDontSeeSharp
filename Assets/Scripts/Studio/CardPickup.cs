using UnityEngine;
using System.Collections.Generic;

public class CardPickup : MonoBehaviour
{
    public static List<CardData> collectedCards = new List<CardData>(); // Liste des cartes ramassées
    private bool isInRange = false;
    public string cardName; // Nom unique de la carte
    public Material cardMaterial; // Texture de la carte

    void Update()
    {
        if (isInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!collectedCards.Exists(card => card.name == cardName))
            {
                collectedCards.Add(new CardData(cardName, cardMaterial));
                Destroy(gameObject); // Détruire la carte une fois ramassée
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = false;
        }
    }
}

[System.Serializable]
public class CardData
{
    public string name;
    public Material material;

    public CardData(string name, Material material)
    {
        this.name = name;
        this.material = material;
    }
}