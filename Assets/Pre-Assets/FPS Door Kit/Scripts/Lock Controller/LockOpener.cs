
/* ~ FPS Door Kit V1.0 ~ */

using UnityEngine;

namespace EnivStudios
{
    [System.Serializable]
    public class DoorKeys
    {
        public bool blueKey;
        public bool redKey;
        public bool greenKey;
        public bool blackKey;
    }
    [System.Serializable]
    public class Keycards
    {
        public bool blueKeycard;
        public bool redKeycard;
        public bool greenKeycard;
        public bool blackKeycard;
    }
    public class LockOpener : MonoBehaviour
    {
       
        [Header("Interact UI")]
        public string interactUI;
        [Space]

        public DoorKeys doorKeys;

        public Keycards keycards;
        
    }
}
