using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum MatchState
{
    PLAYER_MATCHING, INITIAL_PHASE, ROUND_PHASE, ROUND_RESULTS_PHASE, GAME_END
}

public enum PlayerView
{
    RESULTS, NORMAL, SIDE
}

public class MatchManager : NetworkBehaviour
{
    //singleton instance
    public static MatchManager instance;

    List<GardenPlayer> players;

    [HeaderAttribute("Game state")]
    MatchState state;

    //process
    IEnumerator phaseProcess;
    IEnumerator actionProcess;

    float INITIAL_PHASE_TIME = 10f;
    float ROUND_PHASE_TIME = 15f;
    float ROUND_RESULTS_PHASE_TIME = 1f;

    float phaseTime;

    int round;

    // Start is called before the first frame update
    void Awake()
    {
        //singleton
        if (instance != null)
            Destroy(this);

        instance = this;
    }
    
    [Server]
    public void InitializeMatch(List<GardenPlayer> _players)
    {
        players = _players;

        foreach (GardenPlayer player in players)
        {
            player.RpcInitializePlayerContent();
        }

        state = MatchState.INITIAL_PHASE;
    }

    [Server]
    void Update()
    {
        switch (state)
        {
            //initial phase
            case MatchState.INITIAL_PHASE:
                if (phaseProcess == null)
                {
                    phaseProcess = LaunchInitialPhase();
                    StartCoroutine(phaseProcess);
                }
                break;
            //play phase
            case MatchState.ROUND_PHASE:
                if (phaseProcess == null)
                {
                    phaseProcess = LaunchRoundPhase();
                    StartCoroutine(phaseProcess);
                }
                break;
            //results phase
            case MatchState.ROUND_RESULTS_PHASE:
                if (phaseProcess == null)
                {
                    phaseProcess = LaunchRoundResultsPhase();
                    StartCoroutine(phaseProcess);
                }
                break;
            //game end phase
            case MatchState.GAME_END:
                break;
        }
    }
    
    [Server]
    IEnumerator LaunchInitialPhase()
    {
        //initial phase time
        state = MatchState.INITIAL_PHASE;

        //rpc force switch view to side
        foreach (GardenPlayer player in players)
        {
            player.view = PlayerView.SIDE;
            //player.RpcForceSwitchView(PlayerView.SIDE);
        }

        //initial delay
        yield return new WaitForSeconds(1f);
        
        //phase time
        phaseTime = INITIAL_PHASE_TIME;

        //wait for phase time expires or players turn done - 30s
        while (phaseTime > 0f && !GetPlayersReady())
        {
            //update time
            phaseTime -= Time.unscaledDeltaTime;

            foreach (GardenPlayer player in players)
            {
                player.RpcUpdatePhaseTime(phaseTime / INITIAL_PHASE_TIME);
            }

            yield return null;
        }

        //force turn done
        foreach (GardenPlayer player in players)
        {
            player.turnDone = true;
        }

        phaseTime = 0f;

        //initial delay
        yield return new WaitForSeconds(1f);

        //end phase
        state = MatchState.ROUND_RESULTS_PHASE;
        phaseProcess = null;
    }

    [Server]
    IEnumerator LaunchRoundPhase()
    {
        state = MatchState.ROUND_PHASE;
        phaseTime = 0f;

        //force players switch view to side & reset turnDone value
        foreach (GardenPlayer player in players)
        {
            player.turnDone = false;
            player.view = PlayerView.SIDE;
        }

        //initial delay
        yield return new WaitForSeconds(1f);

        phaseTime = ROUND_PHASE_TIME;

        //wait for phase time expires or players turn done - 30s
        while (phaseTime > 0f && !GetPlayersReady())
        {
            //update time
            phaseTime -= Time.unscaledDeltaTime;

            foreach (GardenPlayer player in players)
            {
                player.RpcUpdatePhaseTime(phaseTime / INITIAL_PHASE_TIME);
            }

            yield return null;
        }

        //delay
        phaseTime = 0f;
        yield return new WaitForSeconds(1f);

        //end phase
        state = MatchState.ROUND_RESULTS_PHASE;
        phaseProcess = null;
    }

    [Server]
    IEnumerator LaunchRoundResultsPhase()
    {
        state = MatchState.ROUND_RESULTS_PHASE;

        //force players switch view to side
        foreach (GardenPlayer player in players)
        {
            player.view = PlayerView.RESULTS;
        }

        //starting delay
        yield return new WaitForSeconds(1f);

        //TODO: Check and wait for results updates in clients
        foreach (GardenPlayer player in players)
        {
            var actorsToClear = new List<NetworkIdentity>();

            foreach(NetworkIdentity actorNetIdentity in player.ownedActorsList)
            {
                GardenActor actor = actorNetIdentity.GetComponent<GardenActor>();

                //launch bomb
                if (actor as Bomb)
                    yield return LaunchBomb(actor as Bomb, player.GetComponent<NetworkIdentity>());

                //check actor rounds left
                actor.roundsLeft --;
                if (actor.roundsLeft == 0)
                {
                    actorsToClear.Add(actorNetIdentity);
                    actor.GetComponent<GardenActor>().Die();
                }
            }

            //clear actors
            foreach(NetworkIdentity clearingActor in actorsToClear)
            {
                player.ownedActorsList.Remove(clearingActor);
            }

            yield return null;
        }

        //player gains booty currency dependeing on their in-game booty
        foreach (GardenPlayer player in players)
        {
            round++;
            player.RpcStartNewRound(round);
        }


        yield return null;

        //end phase
        phaseTime = 0f;
        state = MatchState.ROUND_PHASE;
        phaseProcess = null;

    }

    IEnumerator LaunchBomb(Bomb actor, NetworkIdentity player)
    {
        NetworkIdentity _targetPlayer = GetEnemyPlayer(player.netId);
        NetworkIdentity targetActor = null;
        CellInfo cellInfo = new CellInfo { cellIndex = actor.cellIndex[0] };
        
        //check target player
        if (_targetPlayer != null)
        {
            //show actor
            actor.visible = true;
            actor.RpcUpdateActor();

            yield return new WaitForSeconds(.25f);

            //check target actor
            targetActor = GetActorInCell(_targetPlayer, GlobalUtils.GetInverseIndex(actor.cellIndex[0]));
            if (targetActor != null)
            {
                //reveal target actor
                targetActor.GetComponent<GardenActor>().visible = true;
                targetActor.GetComponent<Animator>().SetBool("die", true);
                targetActor.GetComponent<GardenActor>().RpcUpdateActor();
                yield return new WaitForSeconds(.25f);
            }
        }

        //launch bomb
        actor.inAction = true;
        actor.RpcLaunchAction(cellInfo);// (cellInfo);

        yield return new WaitForSeconds(2.5f);
    }

    [Server]
    NetworkIdentity GetActorInCell(NetworkIdentity targetPlayer, Vector3Int _cellIndex)
    {
        foreach (NetworkIdentity actor in targetPlayer.GetComponent<GardenPlayer>().ownedActorsList)
        {
            if (actor.GetComponent<GardenActor>().cellIndex.Contains(_cellIndex))
            {
                return actor;
            }
        }
        return null;
    }

    [Server]
    public NetworkIdentity GetEnemyPlayer(uint playerNetId)
    {
        if (players.Count < 2) return null;

        if (playerNetId == players[0].netId)
            return players[1].GetComponent<NetworkIdentity>();

        return players[0].GetComponent<NetworkIdentity>();

    }
    
    [Server]
    public NetworkConnection GetEnemyConnection(int connectionId)
    {
        if (players.Count < 2) return null;

        if (connectionId == players[0].connectionToClient.connectionId)
            return players[1].GetComponent<NetworkIdentity>().connectionToClient;

        return players[0].GetComponent<NetworkIdentity>().connectionToClient;

    }
    [Server]
    bool GetPlayersReady()
    {
        foreach (GardenPlayer player in players)
        {
            if (!player.turnDone) return false;
        }
        return true;
    }

    [Server]
    void ShowResultMsg(string msg)
    {
        foreach (GardenPlayer player in players)
        {
            player.RpcUpdateResults(msg);
        }
    }
    
    [Server]
    string GetResultMessage(RESULT_MSG_TYPE msgType)
    {
        string resultMsg = "";

        switch (msgType)
        {
            case RESULT_MSG_TYPE.BOMB:
                resultMsg = "<color=#fef9ef>Player01</color> has planted a bomb at <color=#cc2936>Player02</color> garden";
                break;
        }

        return resultMsg;
    }

    public int GetRound()
    {
        return round;
    }

    public void SetRound(int round)
    {
        if (ClientScene.localPlayer.isServer)
            return;

        this.round =  round;
    }
    
}
public enum RESULT_MSG_TYPE
{
    BOMB
}
