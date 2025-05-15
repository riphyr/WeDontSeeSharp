using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class OcclusionCullingDisablerZone : MonoBehaviour
{
    [Header("Colliders de zone à utiliser comme triggers")]
    public List<Collider> zoneTriggers = new List<Collider>();

    [Header("Nom du Layer correspondant au joueur")]
    public string playerLayerName = "Player";

    private int playerLayer;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer(playerLayerName);

        foreach (Collider col in zoneTriggers)
        {
            if (col != null) col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!EstTriggerValide(other)) return;

        Camera cam = other.GetComponentInChildren<Camera>();
        if (cam != null)
        {
            cam.useOcclusionCulling = false;
            Debug.Log("[🟢 OcclusionCullingDisablerZone] Occlusion désactivée !");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!EstTriggerValide(other)) return;

        Camera cam = other.GetComponentInChildren<Camera>();
        if (cam != null)
        {
            cam.useOcclusionCulling = true;
            Debug.Log("[🔴 OcclusionCullingDisablerZone] Occlusion réactivée !");
        }
    }

    private bool EstTriggerValide(Collider other)
    {
        GameObject go = other.GetComponentInParent<PhotonView>()?.gameObject;

        if (go == null)
        {
            Debug.Log("[OcclusionCullingDisablerZone] PhotonView manquant");
            return false;
        }

        if (go.layer != playerLayer)
        {
            Debug.Log($"[OcclusionCullingDisablerZone] Mauvais layer : {go.layer}");
            return false;
        }

        PhotonView view = go.GetComponent<PhotonView>();
        return view != null && view.IsMine;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 1f, 0.8f, 0.3f);
        foreach (Collider col in zoneTriggers)
        {
            if (col is BoxCollider box)
            {
                Gizmos.matrix = col.transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawSphere(col.transform.position + sphere.center, sphere.radius);
            }
        }
    }
}
