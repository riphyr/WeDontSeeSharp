using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class TeleportBehindEnemy : MonoBehaviourPun
{
    public string enemyTag = "Enemy";
    public float behindDistance = 4f;

    private CharacterController cc;

    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetKeyDown(KeyCode.L))
        {
            GameObject target = FindNearestEnemy();
            if (target != null)
            {
                TeleportBehind(target);
            }
        }
    }

    GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        GameObject nearest = null;
        float shortestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < shortestDistance)
            {
                shortestDistance = dist;
                nearest = enemy;
            }
        }

        return nearest;
    }

    void TeleportBehind(GameObject enemy)
    {
        Vector3 enemyForward = enemy.transform.forward;
        Vector3 behindPosition = enemy.transform.position - enemyForward.normalized * behindDistance;

        Debug.Log("💨 Téléportation en cours...");
        Debug.Log("Position actuelle : " + transform.position);

        if (cc != null)
        {
            cc.enabled = false;
        }

        transform.position = behindPosition;
        transform.LookAt(enemy.transform.position);

        if (cc != null)
        {
            cc.enabled = true;
        }

        Debug.Log("📍 Téléporté derrière l'ennemi à : " + transform.position);
    }
} 