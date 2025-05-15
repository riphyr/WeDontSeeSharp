using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class GameProgressManager : MonoBehaviourPunCallbacks
{
    public static GameProgressManager Instance;

    public Dictionary<string, bool> levelCompleted = new Dictionary<string, bool>();
    private HashSet<string> playersInHub = new HashSet<string>();
    public Dictionary<string, HashSet<string>> perPlayerLevelCompletion = new Dictionary<string, HashSet<string>>();

    public int totalPlayers;
    public string currentLevelID = ""; // Nouveau champ pour stocker le niveau actuel
    public string soloLevelID = "porte2";
    public bool allCardsCollectedInSoloLevel = false;
    public GameObject finalDoorPrefab;
    public Transform finalDoorSpawnPoint;
    private string currentSoloPlayer = null;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        totalPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Mettre à jour le niveau actuel à chaque chargement de scène
        currentLevelID = scene.name;

        // À adapter si tu veux redéclencher des effets visuels dans la scène
        foreach (var kvp in levelCompleted)
        {
            if (kvp.Value)
            {
                photonView.RPC("DestroyDoor", RpcTarget.All, kvp.Key);
            }
        }
    }
    
    public bool CanEnterSoloLevel(string userId)
    {
        return currentSoloPlayer == null || currentSoloPlayer == userId;
    }

    public void EnterSoloLevel(string userId)
    {
        if (currentSoloPlayer == null)
        {
            currentSoloPlayer = userId;
            Debug.Log($"[SoloLevel] {userId} est entré dans le niveau solo.");
        }
    }


    public void RegisterReturnToHub(string userId, string lastLevelID)
    {
        Debug.Log("Registered Return to Hub");
        if (!playersInHub.Contains(userId))
        {
            playersInHub.Add(userId);
            Debug.Log("PlayerID" + userId);

            if (lastLevelID != soloLevelID && !levelCompleted.ContainsKey(lastLevelID))
            {
                levelCompleted[lastLevelID] = true;
                Debug.Log("DESTROYYYYYYYY" + lastLevelID);
                Debug.Log(levelCompleted.Count);
                photonView.RPC("DestroyDoor", RpcTarget.All, lastLevelID);
            }

            CheckFinalDoorConditions();
        }
        // Si ce joueur était dans le solo, libère l'accès
        if (currentSoloPlayer == userId)
        {
            currentSoloPlayer = null;
            Debug.Log($"[SoloLevel] {userId} est revenu, accès libéré.");
        }
    }

    public void RegisterSoloLevelCompletion(string playerId, string doorID)
    {
        if (!perPlayerLevelCompletion.ContainsKey(doorID))
        {
            perPlayerLevelCompletion[doorID] = new HashSet<string>();
        }

        if (!perPlayerLevelCompletion[doorID].Contains(playerId))
        {
            perPlayerLevelCompletion[doorID].Add(playerId);
            Debug.Log($"[Progression] Joueur {playerId} a terminé la porte solo {doorID}");
        }
        CheckSoloLevelCompletion();
    }
    
    public bool HasPlayerCompletedSoloLevel(string playerId, string doorID)
    {
        return perPlayerLevelCompletion.ContainsKey(doorID) && perPlayerLevelCompletion[doorID].Contains(playerId);
    }



    public void SetAllCardsCollected()
    {
        allCardsCollectedInSoloLevel = true;
        CheckSoloLevelCompletion();
    }

    private void CheckSoloLevelCompletion()
    {
        if (!levelCompleted.ContainsKey(soloLevelID))
        {
            if (perPlayerLevelCompletion.Count == totalPlayers || allCardsCollectedInSoloLevel)
            {
                levelCompleted[soloLevelID] = true;
                photonView.RPC("DestroyDoor", RpcTarget.All, soloLevelID);
                CheckFinalDoorConditions();
            }
        }
    }

    private void CheckFinalDoorConditions()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        bool allDone = levelCompleted.ContainsKey("porte1") &&
                       levelCompleted.ContainsKey("porte2") &&
                       levelCompleted.ContainsKey("porte3") &&
                       levelCompleted.ContainsKey("porte4");

        if (allDone)
        {
            photonView.RPC("SpawnFinalDoor", RpcTarget.All);
        }
    }

    [PunRPC]
    void SpawnFinalDoor()
    {
        if (finalDoorPrefab != null && finalDoorSpawnPoint != null)
        {
            Instantiate(finalDoorPrefab, finalDoorSpawnPoint.position, finalDoorSpawnPoint.rotation);
        }
    }

    [PunRPC]
    void DestroyDoor(string doorID)
    {
        GameObject door = GameObject.Find(doorID);
        if (door != null)
        {
            Debug.Log("PORTEEEE AUREVOIRRRRRR");
            Destroy(door);
        }
    }
}


/*je t'expique les niveaux : il y en a 4 et 3/4 les joueurs vont tous revenir en meme temps donc y'a pas besoin de gerer
 le fait de attendre que tous les joeurs reviennent pour considere que le niveau soit fait et que la porte soit détruite. 
 En revanche, une des portes contiendra un niveau ou les joueurs devront y acceder un par un, donc quand un joueur est present 
 dans le niveau plus personne peut y acceder jusqu'au retour de l'autre joueur. Il y a deux cas dans ce niveau qui permette 
 de derminer si le niveau est finie : si les joeurs ont recuperé toutes les cartes presntentent ou alors que tous les joueurs ont 
 deja ete dans ce niveau une fois. Maintenant, les portes suivent une certaine logiqe : chaque porte dependent de qi le 
 niveau d'avant a ete fait (sinon la porte est verrouiller en attendant que le niveau d'avant soit realiser), 
 il faut dans ce qu'a stocké qu'elle niveau a ete fait juste avant ( quelle porte a ete franchie juste avant) sachant que 
 lorsque le nievau est realise au retour des joueurs la porte n'existera pus sur la scene. Une fois que tous les niveaux 
 sont faits et que tous les joueurs sont revenus dans le hub, une ultime porte apparait dans le hub. Il faudra bien separer 
 les differents script et les optimiser pour etre le plus propre et clair possible :)*/