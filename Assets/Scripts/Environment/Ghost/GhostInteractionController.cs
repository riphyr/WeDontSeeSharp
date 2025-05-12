using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class GhostInteractionController : MonoBehaviourPun
{
    [Header("Switches à contrôler (lumières fixes)")]
    public List<InteractionScripts.Switch> allSwitches;

    [Header("Portes à manipuler")]
    public List<InteractionScripts.Door> allDoors;
    public float doorChaosDuration = 6f;
    public float doorToggleCooldown = 0.5f;

    [Header("Référence vers l'IA")]
    public GhostAI ghostAI;
    private float baseViewDistance;
    private float basePatrolSpeed;
    private float baseChaseSpeed;
    
    private PhotonView view;
    
    void Start()
    {
        view = GetComponent<PhotonView>();
        
        if (ghostAI)
        {
            baseViewDistance = ghostAI.viewDistance;
            basePatrolSpeed = ghostAI.patrolSpeed;
            baseChaseSpeed = ghostAI.chaseSpeed;
        }
    }

    [ContextMenu("Flicker Fixed Lights (Switches)")]
    public void FlickerSwitchLights()
    {
        StartCoroutine(FlickerSwitchRoutine());
    }

    private IEnumerator FlickerSwitchRoutine()
    {
        float time = 0f;
        float flickRate = 0.25f;

        while (time < 3f)
        {
            foreach (var sw in allSwitches)
            {
                sw.ToggleSwitch();
            }

            yield return new WaitForSeconds(flickRate);
            time += flickRate;
        }
    }

    [ContextMenu("Flicker Flashlights")]
    public void FlickerFlashlights()
    {
        view.RPC("RPC_FlickerFlashlights", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_FlickerFlashlights()
    {
        var allFlashlights = FindObjectsOfType<MonoBehaviour>().OfType<IFlashlightFlicker>();
        foreach (var fl in allFlashlights)
        {
            fl.TriggerFlicker();
        }
    }

    [ContextMenu("Trigger Door Chaos")]
    public void ToggleDoorsRandomly()
    {
        StartCoroutine(DoorChaosRoutine());
    }

    private IEnumerator DoorChaosRoutine()
    {
        float elapsed = 0f;

        while (elapsed < doorChaosDuration)
        {
            var door = allDoors[Random.Range(0, allDoors.Count)];
            if (door != null)
                door.ToggleDoor();

            yield return new WaitForSeconds(Random.Range(0.1f, doorToggleCooldown));
            elapsed += doorToggleCooldown;
        }
    }

    [ContextMenu("Increase Aggressivity")]
    public void IncreaseAggressivity()
    {
        if (!ghostAI) return;

        ghostAI.viewDistance = Mathf.Clamp(ghostAI.viewDistance * 1.2f, baseViewDistance * 0.5f, baseViewDistance * 2f);
        ghostAI.patrolSpeed = Mathf.Clamp(ghostAI.patrolSpeed * 1.2f, basePatrolSpeed * 0.65f, basePatrolSpeed * 1.5f);
        ghostAI.chaseSpeed = Mathf.Clamp(ghostAI.chaseSpeed * 1.2f, baseChaseSpeed * 0.65f, baseChaseSpeed * 1.5f);

        Debug.Log("[Ghost] Agressivité augmentée !");
    }

    [ContextMenu("Decrease Aggressivity")]
    public void DecreaseAggressivity()
    {
        if (!ghostAI) return;

        ghostAI.viewDistance = Mathf.Clamp(ghostAI.viewDistance * 0.75f, baseViewDistance * 0.5f, baseViewDistance * 2f);
        ghostAI.patrolSpeed = Mathf.Clamp(ghostAI.patrolSpeed * 0.9f, basePatrolSpeed * 0.65f, basePatrolSpeed * 1.5f);
        ghostAI.chaseSpeed = Mathf.Clamp(ghostAI.chaseSpeed * 0.9f, baseChaseSpeed * 0.65f, baseChaseSpeed * 1.5f);

        Debug.Log("[Ghost] Agressivité réduite.");
    }
}
