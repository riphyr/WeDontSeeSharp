using UnityEngine;

namespace Labyrinthe
{
    public class MouvementPlayer : MonoBehaviour
    {
        public float speed = 5f;

        void Update()
        {
            //Déplacement du joueur
            float horizontal = Input.GetAxis("Horizontal");
            //A/D ou fleches gauche droite
            float vertical = Input.GetAxis("Vertical");
            //W/S ou fleche haut/bas
            
            
            Vector3 movement = new Vector3(horizontal, 0, vertical);
            transform.Translate(movement);
        }
    }
}