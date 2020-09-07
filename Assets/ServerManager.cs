using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum GameState
{
    PLAYER_MATCHING, INITIAL_PHASE, ROUND_PHASE, ROUND_RESULTS_PHASE, GAME_END
}

public enum PlayView
{
    RESULTS, NORMAL, SIDE
}


public class ServerManager : NetworkBehaviour
{
    //singleton instance
    public static ServerManager instance;

    [HeaderAttribute("Game state")]
    public GameState state;

    [HeaderAttribute("Managers")]
    public GardenNetworkManager networkManager;

    [HeaderAttribute("References")]
    public Sprite gridCursorReference;
    public DynamicTile grassTileReference;
    public Tile holeTileReference;
    public Sprite emptyBootyBox1x2Reference;
    public Sprite emptyBootyBox1x3Reference;
    public Sprite emptyBootyBox2x1Reference;
    public Sprite emptyBootyBox2x2Reference;
    public Sprite emptyBootyBox3x1Reference;

    public GameObject treatOfferPrefab;

    public List<OfferInfo> offers = new List<OfferInfo>();

    //process
    IEnumerator phaseProcess;
    IEnumerator actionProcess;

    float INITIAL_PHASE_TIME = 10f;
    float ROUND_PHASE_TIME = 15f;
    float ROUND_RESULTS_PHASE_TIME = 1f;

    //[SyncVar]
    float phaseTime;

    //[SyncVar]
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
    void Update()
    {
        switch (state)
        {
            //player matching phase
            case GameState.PLAYER_MATCHING:
                //wait for players connecion
                if (networkManager.numPlayers == 2 || (networkManager.numPlayers == 1 && singlePlayerTogle.isOn))
                {
                    InitializePlayers();
                    state = GameState.INITIAL_PHASE;
                }
                break;
            //initial phase
            case GameState.INITIAL_PHASE:
                if (phaseProcess == null)
                {
                    phaseProcess = LaunchInitialPhase();
                    StartCoroutine(phaseProcess);
                }
                break;
            //play phase
            case GameState.ROUND_PHASE:
                if (phaseProcess == null)
                {
                    phaseProcess = LaunchRoundPhase();
                    StartCoroutine(phaseProcess);
                }
                break;
            //results phase
            case GameState.ROUND_RESULTS_PHASE:
                if (phaseProcess == null)
                {
                    phaseProcess = LaunchRoundResultsPhase();
                    StartCoroutine(phaseProcess);
                }
                break;
            //game end phase
            case GameState.GAME_END:
                break;
        }
    }
    
    [Server]
    public void LaunchMatch()
    {
        state = GameState.PLAYER_MATCHING;
    }

    public UnityEngine.UI.Toggle singlePlayerTogle;

    [Server]
    IEnumerator LaunchInitialPhase()
    {
        //initial phase time
        state = GameState.INITIAL_PHASE;

        //rpc force switch view to side
        foreach (GardenPlayer player in networkManager.players)
        {
            player.RpcForceSwitchView(PlayView.SIDE);
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

            foreach (GardenPlayer player in networkManager.players)
            {
                player.RpcUpdatePhaseTime(phaseTime / INITIAL_PHASE_TIME);
            }

            yield return null;
        }

        //force turn done
        if (!GetPlayersReady())
        {
            foreach(GardenPlayer player in networkManager.players)
            {
                Debug.Log("force turn done");
                if (!player.turnDone)
                {
                    //Force end turn
                    player.RpcForceTurnDone();
                }
            }
        }

        //initial delay
        yield return new WaitForSeconds(1f);

        //end phase
        phaseTime = 0f;
        state = GameState.ROUND_RESULTS_PHASE;
        phaseProcess = null;
    }

    [Server]
    IEnumerator LaunchRoundPhase()
    {
        state = GameState.ROUND_PHASE;
        phaseTime = 0f;

        //force players switch view to side & reset turnDone value
        foreach (GardenPlayer player in networkManager.players)
        {
            player.turnDone = false;
            player.RpcForceSwitchView(PlayView.NORMAL);
        }

        //initial delay
        yield return new WaitForSeconds(1f);

        phaseTime = ROUND_PHASE_TIME;

        //wait for phase time expires or players turn done - 30s
        while (phaseTime > 0f)
        {
            //update time
            phaseTime -= Time.unscaledDeltaTime;

            if (GetPlayersReady()) phaseTime = 0f;

            foreach (GardenPlayer player in networkManager.players)
            {
                player.RpcUpdatePhaseTime(phaseTime / ROUND_PHASE_TIME);
            }

            yield return null;
        }


        //delay
        phaseTime = 0f;
        yield return new WaitForSeconds(1f);

        //end phase
        state = GameState.ROUND_RESULTS_PHASE;
        phaseProcess = null;

    }

    [Server]
    IEnumerator LaunchRoundResultsPhase()
    {
        state = GameState.ROUND_RESULTS_PHASE;

        //phaseTime = ROUND_RESULTS_PHASE_TIME;

        //phaseTime -= Time.unscaledDeltaTime;

        //force players switch view to side & reset turnDone value
        foreach (GardenPlayer player in networkManager.players)
        {
            player.RpcForceSwitchView(PlayView.RESULTS);
        }

        yield return new WaitForSeconds(1f);

        //TODO: Check and wait for results updates in clients
        foreach (GardenPlayer player in networkManager.players)
        {
            List<GardenActor> actorsToClear = new List<GardenActor>();

            foreach(GardenActor actor in player.armyList)
            {
                if (actor as Bomb)
                {
                    //TODO command in bomb
                    GardenPlayer targetPlayer = networkManager.GetEnemyPlayer(player.netId);
                    CellInfo cellInfo = new CellInfo { cellIndex = actor.cellIndex[0] };

                    //show actor to enemy
                    actor.visible = true;
                    actor.RpcUpdateActor();

                    //check target player
                    if (targetPlayer != null)
                    {
                        //check actor intercepted
                        NetworkIdentity targetActor = targetPlayer.GetCellActor(actor.cellIndex[0]);
                        if (targetActor != null)
                        {
                            //set enemy actor visible
                            targetActor.GetComponent<GardenActor>().visible = true;
                            targetActor.GetComponent<GardenActor>().RpcUpdateActor();
                        }
                    }

                    actor.RpcLaunchAction(cellInfo);
                    actor.inAction = true;

                    while (actor.inAction) yield return null;

                    actor.Die();
                    if(actor.roundsLeft == 0)
                        actorsToClear.Add(actor);

                    yield return new WaitForSeconds(2.5f);
                }
            }

            foreach(GardenActor clearingActor in actorsToClear)
            {
                player.armyList.Remove(clearingActor);
            }

            yield return null;
        }

        //player gains booty currency dependeing on their in-game booty
        foreach (GardenPlayer player in networkManager.players)
        {
            player.RpcGainRoundBootyCurrency();
            player.RpcForceResetTreating(round);

            round++;
            player.RpcStartNewRound(round);
        }


        yield return null;

        //end phase
        phaseTime = 0f;
        state = GameState.ROUND_PHASE;
        phaseProcess = null;

    }

    [Server]
    bool GetPlayersReady()
    {
        foreach (GardenPlayer player in networkManager.players)
        {
            if (!player.turnDone) return false;
        }
        return true;
    }

    [Server]
    void ShowResultMsg(string msg)
    {
        foreach (GardenPlayer player in networkManager.players)
        {
            player.RpcUpdateResults(msg);
        }
    }


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

    void InitializePlayers()
    {
        foreach (GardenPlayer player in networkManager.players)
        {
            player.RpcInitializePlayerContent();
        }
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
