using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using System.Linq;

[RequireComponent(typeof(PhotonView))]
public class GhostAI : MonoBehaviour
{
    public enum GhostState { Idle, Roaming, Chasing, Investigating }
    [HideInInspector] public PhotonView photonView;

    [Header("Core References")]
    private NavMeshAgent agent;
    public List<Renderer> ghostRenderers = new();
    private Collider ghostCollider;
    public float viewDistance = 10f;
    public float viewAngle = 360f;

    [Header("Behavior Settings")]
    public float patrolSpeed = 0.75f;
    public float chaseSpeed = 1.5f;
    public float investigateDuration = 6f;

    [Header("AI Area Parents (exactly 3 levels)")]
    public Transform undergroundArea;
    public Transform groundFloorArea;
    public Transform firstFloorArea;

    [Header("Roaming Parameters")]
    [Range(0f, 1f)] public float undergroundChance = 0.2f;
    [Range(0f, 1f)] public float groundChance = 0.5f;
    [Range(0f, 1f)] public float firstFloorChance = 0.3f;
    public int maxPatrolsBeforeSwitch = 3;

    [Header("Zone & Cache Detection")]
    public List<Transform> hidingSpots;
    private List<Transform> undergroundPoints = new();
    private List<Transform> groundFloorPoints = new();
    private List<Transform> firstFloorPoints = new();

    private List<Transform> currentPatrolList = new();
    private int currentFloor = 0;
    private int patrolsSinceSwitch = 0;


    [Header("Stuck Detection")]
    private Vector3 stuckCheckPosition;
    private float stuckCheckTimer = 0f;
    private float hardResetTimer = 0f;
    public float stuckCheckInterval = 5f;
    public float minDistanceToConsiderMoving = 0.2f;
    public float maxStuckDuration = 30f;
    private Vector3 initialPosition;

    private GameObject currentTarget;
    private Vector3 lastKnownPosition;
    private bool aiActive = false;
    private Coroutine ghostEffectRoutine;
    private bool ghostEffectActive = false;
    private List<GameObject> allPlayers = new List<GameObject>();
    private GhostState currentState = GhostState.Idle;
    private int patrolIndex = 0;

	public bool IsAiActive() => aiActive;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        photonView = GetComponent<PhotonView>();
        initialPosition = transform.position;
        stuckCheckPosition = transform.position;
        ghostCollider = GetComponent<Collider>();

        foreach (var renderer in ghostRenderers)
            if (renderer != null) renderer.enabled = false;

        if (ghostCollider != null)
            ghostCollider.enabled = false;
        
        FindAllPlayers();
        InitPatrolPoints();
        Debug.Log($"[GhostAI] INIT floor point counts — UG: {undergroundPoints.Count}, GF: {groundFloorPoints.Count}, FF: {firstFloorPoints.Count}");
        StartCoroutine(DelayedActivation());
    }

    void Update()
    {
        if (!aiActive) return;

        switch (currentState)
        {
            case GhostState.Chasing:
                UpdateChase();
                break;
            case GhostState.Investigating:
                UpdateInvestigate();
                SearchForTarget();
                break;
            case GhostState.Roaming:
                UpdateRoaming();
                SearchForTarget();
                break;
        }

        SetGhostEffect(currentState == GhostState.Chasing);
        CheckForStuck();

        if (!agent.pathPending && agent.remainingDistance <= 0.5f && currentState == GhostState.Roaming)
        {
            Debug.Log("[GhostAI] Reached patrol point, choosing next.");
            GoToNextPatrolPoint();
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogError("[GhostAI] Agent is off the NavMesh! Repositioning or respawn required.");
            return;
        }

        if (!agent.hasPath && !agent.pathPending)
        {
            Debug.LogWarning("[GhostAI] No current path set. Forcing patrol point.");
            GoToNextPatrolPoint();
        }

        CheckForDoorsAround();
    }

    void CheckForStuck()
    {
        stuckCheckTimer += Time.deltaTime;

        if (stuckCheckTimer >= stuckCheckInterval)
        {
            float moved = Vector3.Distance(transform.position, stuckCheckPosition);

            if (moved < minDistanceToConsiderMoving)
            {
                hardResetTimer += stuckCheckTimer; // ← on ajoute le temps uniquement si elle a été bloquée
                Debug.LogWarning("[GhostAI] Suspected stuck. Re-routing...");
                GoToNextPatrolPoint();
            }
            else
            {
                hardResetTimer = 0f; // ← si elle a bougé, on reset le timer
            }

            stuckCheckPosition = transform.position;
            stuckCheckTimer = 0f;
        }

        if (hardResetTimer >= maxStuckDuration)
        {
            Debug.LogError("[GhostAI] HARD RESET triggered (trapped too long). Resetting...");
            hardResetTimer = 0f;
            transform.position = initialPosition;
            agent.Warp(initialPosition);
            DeactivateAI();
            ActivateAI();
        }
    }

    void SearchForTarget()
    {
        foreach (var player in allPlayers)
        {
            if (player != null && CanSeePlayer(player))
            {
                currentTarget = player;
                currentState = GhostState.Chasing;
                Debug.Log("[GhostAI] Target acquired: " + player.name);
                return;
            }
        }
    }

    void FindAllPlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        allPlayers.Clear();
        allPlayers.AddRange(players);
        Debug.Log("[GhostAI] Players found: " + allPlayers.Count);
    }
    
    void InitPatrolPoints()
    {
        undergroundPoints.Clear();
        groundFloorPoints.Clear();
        firstFloorPoints.Clear();

        foreach (Transform child in undergroundArea) undergroundPoints.Add(child);
        foreach (Transform child in groundFloorArea) groundFloorPoints.Add(child);
        foreach (Transform child in firstFloorArea) firstFloorPoints.Add(child);

        SelectNextFloor();
    }
    
    [ContextMenu("Refresh Player List")]
    public void RefreshPlayerList()
    {
        FindAllPlayers();
    }

    void UpdateChase()
    {
        if (currentTarget == null)
        {
            currentState = GhostState.Investigating;
            StartCoroutine(InvestigateRoutine());
            return;
        }

        if (CanSeePlayer(currentTarget))
        {
            lastKnownPosition = currentTarget.transform.position;
            agent.speed = chaseSpeed;
            agent.SetDestination(lastKnownPosition);

			if (Vector3.Distance(transform.position, currentTarget.transform.position) < 1.2f)
			{
    			if (photonView.IsMine)
    			{
        			GhostCatchManager.Instance?.OnPlayerCaught(currentTarget);
    			}
			}

            if (photonView.IsMine)
                photonView.RPC("RPC_FlashlightsFlicker", RpcTarget.All);

            Debug.Log("[GhostAI] Chasing: " + currentTarget.name);
        }
        else
        {
            currentTarget = null;
            currentState = GhostState.Investigating;
            StartCoroutine(InvestigateRoutine());
        }
    }

    void UpdateInvestigate()
    {
        if (!agent.pathPending && agent.remainingDistance <= 0.5f)
        {
            Debug.Log("[GhostAI] Done with investigate move, starting random investigation.");
        }
    }

    void UpdateRoaming()
    {
        if (!agent.pathPending && agent.remainingDistance <= 0.5f)
        {
            GoToNextPatrolPoint();
        }
    }
    
    void GoToNextPatrolPoint()
    {
        if (currentPatrolList == null || currentPatrolList.Count == 0)
        {
            Debug.LogError("[GhostAI] Current floor patrol list is empty.");
            return;
        }

        patrolsSinceSwitch++;

        if (patrolsSinceSwitch >= maxPatrolsBeforeSwitch)
        {
            SelectNextFloor();
            patrolsSinceSwitch = 0;
        }

        Transform target = currentPatrolList[Random.Range(0, currentPatrolList.Count)];
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(target.position, path);

        if (path.status == NavMeshPathStatus.PathComplete)
        {
            agent.speed = patrolSpeed;
            agent.SetDestination(target.position);
            Debug.Log("[GhostAI] Patrolling to: " + target.name);
        }
        else
        {
            Debug.LogWarning("[GhostAI] Invalid path to: " + target.name + " – retrying.");
            GoToNextPatrolPoint(); // retry with another
        }
    }
    
    void SelectNextFloor()
    {
        float roll = Random.value;
        if (roll < undergroundChance)
        {
            currentPatrolList = undergroundPoints;
            currentFloor = 0;
        }
        else if (roll < undergroundChance + groundChance)
        {
            currentPatrolList = groundFloorPoints;
            currentFloor = 1;
        }
        else
        {
            currentPatrolList = firstFloorPoints;
            currentFloor = 2;
        }

        Debug.Log($"[GhostAI] Switched to floor {currentFloor} with {currentPatrolList.Count} points.");
    }

    void CheckForDoorsAround()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 1.5f);
        foreach (var hit in hits)
        {
            var door = hit.GetComponentInParent<InteractionScripts.Door>();
            if (door != null && !door.IsOpen())
            {
                // Ignorer les portes de type Crowbar
                if (door.doorType == InteractionScripts.Door.DoorType.Crowbar)
                    continue;

                bool isUnlocked =
                    door.doorType == InteractionScripts.Door.DoorType.Normal ||
                    (door.doorType == InteractionScripts.Door.DoorType.LockKey && !door.HasActiveLock<InteractionScripts.LockKey>()) ||
                    (door.doorType == InteractionScripts.Door.DoorType.PadLock && !door.HasActiveLock<InteractionScripts.PadLock>());

                if (isUnlocked)
                {
                    Debug.Log("[GhostAI] Opening door: " + door.name);
                    door.ToggleDoor();
                }
            }
        }
    }

    bool CanSeePlayer(GameObject player)
    {
        if (player == null) return false;

        Vector3 dir = (player.transform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dir);

        if (angle < viewAngle * 0.5f || viewAngle >= 360f)
        {
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, out RaycastHit hit, viewDistance))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    return !IsPlayerHidden(player);
                }
            }
        }

        return false;
    }

    bool IsPlayerHidden(GameObject player)
    {
        foreach (Transform spot in hidingSpots)
        {
            if (spot.GetComponent<Collider>().bounds.Contains(player.transform.position))
            {
                return true;
            }
        }
        return false;
    }
    
    public void ActivateAI()
    {
        if (!PhotonNetwork.IsMasterClient || aiActive) return;

        aiActive = true;
        currentState = GhostState.Roaming;
        GoToNextPatrolPoint();

        foreach (var renderer in ghostRenderers)
            if (renderer != null) renderer.enabled = true;

        if (ghostCollider != null)
            ghostCollider.enabled = true;
    }

    
    [ContextMenu("Force Activation (!NOT SAFE!)")]
    public void ForceActivation()
    {
        aiActive = true;
        currentState = GhostState.Roaming;
        GoToNextPatrolPoint();
    }
    
    IEnumerator DelayedActivation()
    {
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
        yield return new WaitForSeconds(0.1f); // petite sécurité

        AskForActivation(); // demande au master
    }

    [PunRPC]
    void RPC_RequestAIActivation()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ActivateAI();
        }
    }

    public void AskForActivation()
    {
        photonView.RPC("RPC_RequestAIActivation", RpcTarget.MasterClient);
    }

    public void DeactivateAI()
    {
        if (!PhotonNetwork.IsMasterClient || aiActive is false) return;
        
        aiActive = false;
        currentTarget = null;
        agent.ResetPath();
        
        foreach (var renderer in ghostRenderers)
            if (renderer != null) renderer.enabled = false;

        if (ghostCollider != null)
            ghostCollider.enabled = false;
    }
    
    [PunRPC]
    void RPC_RequestAIDeactivation()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            DeactivateAI();
        }
    }
    
    public void AskForDeactivation()
    {
        photonView.RPC("RPC_RequestAIDeactivation", RpcTarget.MasterClient);
    }

    public void EnableGhostEffect()
    {
        if (ghostEffectRoutine == null)
            ghostEffectRoutine = StartCoroutine(GhostEffect());
    }
    
    public void DisableGhostEffect()
    {
        if (ghostEffectRoutine != null)
        {
            StopCoroutine(ghostEffectRoutine);
            foreach (var renderer in ghostRenderers)
            {
                if (renderer != null)
                    renderer.enabled = true;
            }
            ghostEffectRoutine = null;
        }
    }
    
    IEnumerator GhostEffect()
    {
        while (true)
        {
            foreach (var renderer in ghostRenderers)
            {
                if (renderer != null)
                    renderer.enabled = !renderer.enabled;
            }
            yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
        }
    }
    
    void SetGhostEffect(bool enabled)
    {
        if (enabled && !ghostEffectActive)
        {
            EnableGhostEffect();
            ghostEffectActive = true;
        }
        else if (!enabled && ghostEffectActive)
        {
            DisableGhostEffect();
            ghostEffectActive = false;
        }
    }

    IEnumerator InvestigateRoutine()
    {
        agent.speed = patrolSpeed;
        float timer = 0f;

        while (timer < investigateDuration)
        {
            Vector3 randomOffset = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
            Vector3 point = lastKnownPosition + randomOffset;

            if (NavMesh.SamplePosition(point, out NavMeshHit hit, 1f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                Debug.Log("[GhostAI] Investigating point: " + hit.position);

                float waitTime = 0f;
                while (agent.pathPending || agent.remainingDistance > 0.5f)
                {
                    waitTime += Time.deltaTime;
                    if (waitTime > 3f)
                    {
                        Debug.LogWarning("[GhostAI] Investigation wait timeout.");
                        break;
                    }
                    yield return null;
                }
            }
            timer += 2f;
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("[GhostAI] Investigation done, resuming patrol.");
        currentState = GhostState.Roaming;
        GoToNextPatrolPoint();
    }
    
    [PunRPC]
    void RPC_FlashlightsFlicker()
    {
        var allLights = FindObjectsOfType<MonoBehaviour>().OfType<IFlashlightFlicker>();
        foreach (var light in allLights)
        {
            light.TriggerFlicker();
        }
    }

    void OnDrawGizmosSelected()
    {
        // Vision range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        // Destination actuelle
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, agent.destination);
            Gizmos.DrawSphere(agent.destination, 0.2f);
        }

        // Points de patrouille : Vert = current floor / Gris = autres
        void DrawPoints(List<Transform> points, Color color)
        {
            Gizmos.color = color;
            foreach (var pt in points)
            {
                if (pt != null)
                    Gizmos.DrawWireSphere(pt.position, 0.3f);
            }
        }

        DrawPoints(undergroundPoints, currentFloor == 0 ? Color.green : Color.gray);
        DrawPoints(groundFloorPoints, currentFloor == 1 ? Color.green : Color.gray);
        DrawPoints(firstFloorPoints, currentFloor == 2 ? Color.green : Color.gray);
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
