using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    // Start is called before the first frame update
    private GameObject character1;
    private GameObject character2;
    private GameObject character3;
    private GameObject character4;

    private NetworkingManager.NetworkingManager _networkingManager;
    private LevelManager.LevelManager _levelManager;
    private GameObject bulletPrefab;

    private void Awake()
    {
       
    }

    void Start()
    {
        _networkingManager = FindObjectOfType<NetworkingManager.NetworkingManager>();
        _levelManager = FindObjectOfType<LevelManager.LevelManager>();
        if (isServer) {
            character1 = _networkingManager.spawnPrefabs[0];
            character2 = _networkingManager.spawnPrefabs[1];
            character3 = _networkingManager.spawnPrefabs[2];
            character4 = _networkingManager.spawnPrefabs[3];

        }
        
        if (isClient && isLocalPlayer)
        {
            gameObject.tag = "LocalPlayer";
            if(_levelManager.getCurrentLevel().spawnPlayers) CmdSpawnPlayer(GameObject.FindGameObjectWithTag("DDOL").GetComponent<DDOL>().playerName);
        }

        for (int i = 0; i < _networkingManager.spawnPrefabs.Count; i++)
        {
            if (_networkingManager.spawnPrefabs[i].tag.Equals("Bullet"))
            {
                bulletPrefab = _networkingManager.spawnPrefabs[i];
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
     
    }

    [Command]
    public void CmdSelectedCharacter(string characterName)
    {
        UICharacterSelectionManager uICharacterSelectionManager = GameObject.Find("UICharacterSelectionManager").GetComponent<UICharacterSelectionManager>();
        FindObjectOfType<NetworkingManager.NetworkingManager>().addSelectedPlayer(characterName);
        if (characterName == "Markus Nobel") uICharacterSelectionManager.Character1Taken = true;
        else if (characterName == "Ken Nolo") uICharacterSelectionManager.Character2Taken = true;
        else if (characterName == "Kam Brylla") uICharacterSelectionManager.Character3Taken = true;
        else if (characterName == "Raphael Nosun") uICharacterSelectionManager.Character4Taken = true;
        uICharacterSelectionManager.numCharactersChoosen++;
    }
    
    [Command]
    public void CmdSpawnPlayer(String playerName)
    {
        GameObject go = null;
        if (playerName == "Markus Nobel")
        {
            
            go = Instantiate(character1, _levelManager.getCurrentLevel().getPlayerPosition(playerName), Quaternion.identity);
        }
        else if (playerName == "Ken Nolo")
        {
            go = Instantiate(character2, _levelManager.getCurrentLevel().getPlayerPosition(playerName), Quaternion.identity);

        }
        else if (playerName == "Kam Brylla")
        {
            go = Instantiate(character3, _levelManager.getCurrentLevel().getPlayerPosition(playerName), Quaternion.identity);
        }
        else if (playerName == "Raphael Nosun")
        {
            go = Instantiate(character4, _levelManager.getCurrentLevel().getPlayerPosition(playerName), Quaternion.identity);
        }

        if (go != null)
        {
            go.GetComponent<PlayerMoveset>().playerName = playerName;
            NetworkServer.SpawnWithClientAuthority(go, connectionToClient);   
        }
        
    }

    [Command]
    public void CmdAssignAuthority(GameObject gameObject)
    {
        NetworkServer.objects[gameObject.GetComponent<NetworkIdentity>().netId].AssignClientAuthority(connectionToClient);
    } 
    
    [Command]
    public void CmdRemoveAuthority(GameObject gameObject)
    {
        NetworkServer.objects[gameObject.GetComponent<NetworkIdentity>().netId].RemoveClientAuthority(connectionToClient);
    }

    [Command]
    public void CmdSpawnBullets(Vector3 position, Quaternion rotation, float spread, float bulletForce, string bulletName, string shooterName)
    {
        var randomNumberX = UnityEngine.Random.Range(-spread, spread);
        var randomNumberZ = UnityEngine.Random.Range(-spread, spread);
        GameObject spawnedGameObject = Instantiate(bulletPrefab, position, rotation);
        spawnedGameObject.transform.Rotate(randomNumberX, 0f, randomNumberZ);
        spawnedGameObject.GetComponent<Rigidbody>().AddForce(spawnedGameObject.transform.forward * bulletForce, ForceMode.Impulse);
        spawnedGameObject.GetComponent<Bullet>().bulletName = bulletName;
        spawnedGameObject.GetComponent<Bullet>().shooterName = shooterName;
        NetworkServer.Spawn(spawnedGameObject);
        Destroy(spawnedGameObject, 3f);
    }

    [Command]
    public void CmdsetLight1(bool value)
    {
        GameObject.FindGameObjectWithTag("DarkPuzzleMatchManager").GetComponent<DarkPuzzleMatchManager>().light1On =
            value;
    }

    [Command]
    public void CmdsetLight2(bool value)
    {
        GameObject.FindGameObjectWithTag("DarkPuzzleMatchManager").GetComponent<DarkPuzzleMatchManager>().light2On =
            value;
    }
    
    [Command]
    public void CmdsetLight3(bool value)
    {
        GameObject.FindGameObjectWithTag("DarkPuzzleMatchManager").GetComponent<DarkPuzzleMatchManager>().light3On =
            value;
    }

    [Command]
    public void CmdsetLight4(bool value)
    {
        GameObject.FindGameObjectWithTag("DarkPuzzleMatchManager").GetComponent<DarkPuzzleMatchManager>().light4On =
            value;
    }

    [Command]
    public void CmdSetMarkusNobleStats(int points, int deaths, int friendlyKill, int powerUp, int collectible, int antivirusKilled, int notOrdered, int greetChef, int cableManagement, string bonusPrize)
    {
        UILeaderboard uiLeaderboard = GameObject.FindGameObjectWithTag("UILeaderboard").GetComponent<UILeaderboard>();
        uiLeaderboard.MarkusNobelEnabled = true;
        uiLeaderboard.MarkusNoblePoints = points;
        uiLeaderboard.MarkusNobleDeaths = deaths;
        uiLeaderboard.MarkusNobelStats.Insert(0,friendlyKill);
        uiLeaderboard.MarkusNobelStats.Insert(1, powerUp);
        uiLeaderboard.MarkusNobelStats.Insert(2, collectible);
        uiLeaderboard.MarkusNobelStats.Insert(3, antivirusKilled);
        uiLeaderboard.MarkusNobelStats.Insert(4, notOrdered);
        uiLeaderboard.MarkusNobelStats.Insert(5, greetChef);
        uiLeaderboard.MarkusNobelStats.Insert(6, cableManagement);

        uiLeaderboard.MarkusNobleBonusPrize = bonusPrize;
    }
    
    [Command]
    public void CmdSetKenNoloStats(int points, int deaths, int friendlyKill, int powerUp, int collectible, int antivirusKilled, int notOrdered, int greetChef, int cableManagement, string bonusPrize)
    {
        UILeaderboard uiLeaderboard = GameObject.FindGameObjectWithTag("UILeaderboard").GetComponent<UILeaderboard>();
        uiLeaderboard.KenNoloEnabled = true;
        uiLeaderboard.KenNoloPoints = points;
        uiLeaderboard.KenNoloDeaths = deaths;
        uiLeaderboard.KenNoloStats.Insert(0,friendlyKill);
        uiLeaderboard.KenNoloStats.Insert(1, powerUp);
        uiLeaderboard.KenNoloStats.Insert(2, collectible);
        uiLeaderboard.KenNoloStats.Insert(3, antivirusKilled);
        uiLeaderboard.KenNoloStats.Insert(4, notOrdered);
        uiLeaderboard.KenNoloStats.Insert(5, greetChef);
        uiLeaderboard.KenNoloStats.Insert(6, cableManagement);
        
        uiLeaderboard.KenNoloBonusPrize = bonusPrize;
    }
    
    [Command]
    public void CmdSetKamBryllaStats(int points, int deaths, int friendlyKill, int powerUp, int collectible, int antivirusKilled, int notOrdered, int greetChef, int cableManagement, string bonusPrize)
    {
        UILeaderboard uiLeaderboard = GameObject.FindGameObjectWithTag("UILeaderboard").GetComponent<UILeaderboard>();
        uiLeaderboard.KamBryllaEnabled = true;
        uiLeaderboard.KamBryllaPoints = points;
        uiLeaderboard.KamBryllaDeaths = deaths;
        uiLeaderboard.KamBryllalStats.Insert(0,friendlyKill);
        uiLeaderboard.KamBryllalStats.Insert(1, powerUp);
        uiLeaderboard.KamBryllalStats.Insert(2, collectible);
        uiLeaderboard.KamBryllalStats.Insert(3, antivirusKilled);
        uiLeaderboard.KamBryllalStats.Insert(4, notOrdered);
        uiLeaderboard.KamBryllalStats.Insert(5, greetChef);
        uiLeaderboard.KamBryllalStats.Insert(6, cableManagement);
        
        uiLeaderboard.KamBryllaBonusPrize = bonusPrize;
    }
    
    [Command]
    public void CmdSetRaphaelNosunStats(int points, int deaths, int friendlyKill, int powerUp, int collectible, int antivirusKilled, int notOrdered, int greetChef, int cableManagement,  string bonusPrize)
    {
        UILeaderboard uiLeaderboard = GameObject.FindGameObjectWithTag("UILeaderboard").GetComponent<UILeaderboard>();
        uiLeaderboard.RaphaelNosunEnabled = true;
        uiLeaderboard.RaphaelNosunPoints = points;
        uiLeaderboard.RaphaelNosunDeaths = deaths;
        uiLeaderboard.RapahelNosunStats.Insert(0,friendlyKill);
        uiLeaderboard.RapahelNosunStats.Insert(1, powerUp);
        uiLeaderboard.RapahelNosunStats.Insert(2, collectible);
        uiLeaderboard.RapahelNosunStats.Insert(3, antivirusKilled);
        uiLeaderboard.RapahelNosunStats.Insert(4, notOrdered);
        uiLeaderboard.RapahelNosunStats.Insert(5, greetChef);
        uiLeaderboard.RapahelNosunStats.Insert(6, cableManagement);
        
        uiLeaderboard.RaphaelNosunBonusPrize = bonusPrize;
    }

    [Command]
    public void CmdSetTransformTo(GameObject go, Vector3 position){
        go.transform.position = position;
    }
    

    [Command]
    public void CmdSetMesh(GameObject go, bool value)
    {
        go.GetComponent<PlayerMoveset>().meshActive = value;
    }

    [Command]
    public void CmdSetPipeConnected(GameObject go, bool value){
        go.GetComponent<Pipe>().isConnected = value;
    }

    [Command]
    public void CmdSetGrabebd(GameObject go, bool value)
    {
        go.GetComponent<GrabbableMovement>().cubeMovement = value;
    }

    [Command]
    public void CmdSetFirstElectroPipeline(GameObject go, bool value)
    {
        go.GetComponent<Pipe>().isFirst = value;
        if (!value)
        {
            GameObject[] pipes = GameObject.FindGameObjectsWithTag("Pipe");
            foreach (GameObject pipe in pipes)
            {
                if (pipe.transform.name != "PipeLineStart" && pipe.transform.name != "PipeLineEnd")
                {
                    CmdSetPipeConnected(pipe.gameObject, false);

                }
       
            }
        }
    }

    [Command]
    public void CmdSetEndPipeline(GameObject go, bool value)
    {
        go.GetComponent<Pipe>().isEnd = value;
    }

    [Command]
    public void CmdSetMatchFailedCookingTime(GameObject go, bool value)
    {
        go.GetComponent<CookingTimeMatchManager>().matchFailed = value;
    }

    [Command]
    public void CmdSetMatchWon()
    {
        FindObjectOfType<MatchManager>().setMatchWon();
    }

    [Command]
    public void CmdLobby()
    {
        NetworkingManager.NetworkingManager _networkingManager = FindObjectOfType<NetworkingManager.NetworkingManager>();
        _networkingManager.serverChangeScene("Lobby", 0);
    }

    [Command]
    public void CmdPipeReleased()
    {
        StartCoroutine(pipeReleasedCoroutine());
    }
    
    IEnumerator pipeReleasedCoroutine()
    {
        GameObject[] pipes = GameObject.FindGameObjectsWithTag("Pipe");
        for (int i = 0; i < pipes.Length; i++)
        {
            yield return new WaitForSeconds(0.2f);
            foreach (GameObject pipe in pipes)
            {
                if (pipe.transform.name != "PipeLineStart" && pipe.transform.name != "PipeLineEnd")
                {
                    pipe.GetComponent<Pipe>().allPipeReleased();  
                }
           
            }  
        }
        
        

    }
}
