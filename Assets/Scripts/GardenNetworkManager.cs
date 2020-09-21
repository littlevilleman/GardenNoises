using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GardenNetworkManager : NetworkManager
{
    public MatchManager matchManager;

    public List<GardenPlayer> players;

    public Transform hostPlayerSpawn;
    public Transform remotePlayerSpawn;

    public GameObject worldReference;

    public UnityEngine.UI.Toggle singlePlayerDebug;

    public GameObject matchManagerPrefab;


    private void OnServerInitialized()
    {
        players = new List<GardenPlayer>(2);

        //ClientScene.RegisterPrefab(worldReference);
        NetworkClient.RegisterHandler<ConnectMessage>(OnClientConnect);
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        //initialize new player
        Transform start = numPlayers == 0 ? hostPlayerSpawn : remotePlayerSpawn;
        GameObject player = Instantiate(playerPrefab, hostPlayerSpawn.position, hostPlayerSpawn.rotation);
        players.Add(player.GetComponent<GardenPlayer>());

        NetworkServer.AddPlayerForConnection(conn, player);

        if (singlePlayerDebug.isOn || numPlayers == 2)
        {
            matchManager = Instantiate(matchManagerPrefab).GetComponent<MatchManager>();
                                
            NetworkServer.Spawn(matchManager.gameObject);

            matchManager.InitializeMatch(players);
        }
    }

    void OnClientConnect(NetworkConnection conn, ConnectMessage msg)
    {
        Debug.Log(Time.time + "Connected to server: " + conn);
    }

    void OnServerConnect(NetworkConnection conn, ConnectMessage msg)
    {
        Debug.Log("New client connected: " + conn);
    }
}
