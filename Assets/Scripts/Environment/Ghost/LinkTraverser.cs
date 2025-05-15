using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class LinkTraverser : MonoBehaviour
{
    private NavMeshAgent agent;
    private bool isTraversing;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (!agent.isOnOffMeshLink || isTraversing) return;

        StartCoroutine(TraverseLink(agent.currentOffMeshLinkData));
    }

    IEnumerator TraverseLink(OffMeshLinkData linkData)
    {
        isTraversing = true;

        Vector3 startPos = agent.transform.position;
        Vector3 endPos = linkData.endPos + Vector3.up * agent.baseOffset;

        float distance = Vector3.Distance(startPos, endPos);
        float duration = distance / agent.speed; // ← On utilise la vitesse du NavMeshAgent !

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            agent.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        agent.CompleteOffMeshLink();
        isTraversing = false;
    }
}