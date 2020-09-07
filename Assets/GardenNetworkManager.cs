using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GardenNetworkManager : NetworkManager
{
    public List<GardenPlayer> players;

    public Transform hostPlayerSpawn;
    public Transform remotePlayerSpawn;

    public GameObject worldReference;


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

        //player.GetComponent<GardenPlayer>().RpcTakeStickersAuthority();

        NetworkServer.AddPlayerForConnection(conn, player);
    }

    void OnClientConnect(NetworkConnection conn, ConnectMessage msg)
    {
        Debug.Log(Time.time + "Connected to server: " + conn);
    }

    void OnServerConnect(NetworkConnection conn, ConnectMessage msg)
    {
        Debug.Log("New client connected: " + conn);
    }

    public GardenPlayer GetEnemyPlayer(uint playerNetId)
    {
        if (numPlayers == 1) return null;

        if (playerNetId == players[0].netId)
            return players[1];

        return players[0];

    }
}
