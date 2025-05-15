using UnityEngine;

public class LoreProgressManager : MonoBehaviour
{
    [Header("Lore Progress States")]
    [SerializeField] private bool clothesUnlocked = false;
    [SerializeField] private bool clothesSearched = false;
    [SerializeField] private bool diaryUnlocked = false;
    [SerializeField] private bool diaryTaken = false;
    [SerializeField] private bool trunk1Unlocked = false;
    [SerializeField] private bool trunk1Moved = false;
    [SerializeField] private bool trunk1Opened = false;
    [SerializeField] private bool trunk2Unlocked = false;
    [SerializeField] private bool trunk2Opened = false;
    [SerializeField] private bool photoTakeable = false;
    [SerializeField] private bool photoTaken = false;
    [SerializeField] private bool letterTaken = false;
    [SerializeField] private bool cassetteTaken = false;
    [SerializeField] private bool radioPlayed = false;

    public void SetClothesUnlocked(bool value) => clothesUnlocked = value;
    public void SetClothesSearched()
    {
        clothesSearched = true;
        diaryUnlocked = true;
        Debug.Log("[Lore] Clothes searched → Diary unlocked.");
    }
    public void SetDiaryTaken()
    {
        diaryTaken = true;
        Debug.Log("[Lore] Diary taken.");
    }
    public void SetTrunk1Unlocked(bool value) => trunk1Unlocked = value;
    public void SetTrunk1Moved()
    {
        trunk1Moved = true;
        Debug.Log("[Lore] Trunk 1 moved.");
    }
    public void SetTrunk1Opened()
    {
        trunk1Opened = true;
        Debug.Log("[Lore] Trunk 1 opened.");
    }
    public void SetTrunk2Unlocked(bool value) => trunk2Unlocked = value;
    public void SetTrunk2Opened()
    {
        trunk2Opened = true;
        Debug.Log("[Lore] Trunk 2 opened.");
    }
    public void SetPhotoTakeable(bool value) => photoTakeable = value;
    public void SetPhotoTaken()
    {
        photoTaken = true;
        Debug.Log("[Lore] Photo taken.");
    }
    public void SetLetterTaken()
    {
        letterTaken = true;
        Debug.Log("[Lore] Letter taken.");
    }
    public void SetCassetteTaken()
    {
        cassetteTaken = true;
        Debug.Log("[Lore] Cassette taken.");
    }
    public void SetRadioPlayed()
    {
        radioPlayed = true;
        Debug.Log("[Lore] Radio played.");
    }

    public bool IsClothesUnlocked() => clothesUnlocked;
    public bool IsClothesSearched() => clothesSearched;
    public bool IsDiaryUnlocked() => diaryUnlocked;
    public bool IsDiaryTaken() => diaryTaken;
    public bool IsTrunk1Unlocked() => trunk1Unlocked;
    public bool IsTrunk1Moved() => trunk1Moved;
    public bool IsTrunk1Opened() => trunk1Opened;
    public bool IsTrunk2Unlocked() => trunk2Unlocked;
    public bool IsTrunk2Opened() => trunk2Opened;
    public bool IsPhotoTakeable() => photoTakeable;
    public bool IsPhotoTaken() => photoTaken;
    public bool IsLetterTaken() => letterTaken;
    public bool IsCassetteTaken() => cassetteTaken;
    public bool IsRadioPlayed() => radioPlayed;
}
