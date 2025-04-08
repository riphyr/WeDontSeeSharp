using UnityEngine;
using Photon.Pun;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class PlayerUsing : MonoBehaviourPun
{
    public GameObject matchPrefab;
    private AudioSource audioSource;
    public AudioClip matchSound;

    private PlayerInventory inventory;
    private Transform playerBody;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        inventory = GetComponent<PlayerInventory>();
        playerBody = transform;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            photonView.RPC("UseMatch", RpcTarget.All);
        }
    }

    [PunRPC]
    private void UseMatch()
    {
        if (!photonView.IsMine) 
            return;

        if (!inventory.HasItem("Match"))
        {
            return;
        }

        inventory.RemoveItem("Match", 1);
        audioSource.PlayOneShot(matchSound);

        StartCoroutine(SpawnMatchAfterSound());
    }
    
    private Vector3 GetMatchSpawnPosition()
    {
        Vector3 spawnStart = playerBody.position + playerBody.forward * 0.5f + Vector3.down * 0.5f;
        RaycastHit hit;

        if (Physics.Raycast(spawnStart, Vector3.down, out hit, 2f))
        {
            return hit.point + Vector3.up * 0.1f;
        }

        return playerBody.position + playerBody.forward * 0.5f + Vector3.up * 0.1f;
    }

    private IEnumerator SpawnMatchAfterSound()
    {
        yield return new WaitForSeconds(matchSound.length);

        Vector3 spawnPos = GetMatchSpawnPosition();
        Quaternion matchRotation = Quaternion.Euler(-90f, playerBody.eulerAngles.y, 0f);

        GameObject matchInstance = PhotonNetwork.Instantiate(matchPrefab.name, spawnPos, matchRotation);

        yield return new WaitForSeconds(0.1f);

        InteractionScripts.Match matchScript = matchInstance.GetComponent<InteractionScripts.Match>();

        matchScript.AssignOwner(photonView.Owner, playerBody);
        matchScript.IgniteMatch();
    }
}