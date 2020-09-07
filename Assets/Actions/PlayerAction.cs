using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionState
{
    STANDBY, ACTIVE, DEAD
}

public enum ActionType
{
    ADD_ACTOR, PLAY_BOMB, PLAY_SQUIRREL,
}
public abstract class PlayerAction : PlayerAction<PlayerActionInfo>
{

}

public abstract class PlayerAction<T> : IPlayerAction where T : PlayerActionInfo
{    
    //action parameters
    public NetworkIdentity owner;
    //public SyncIndex cellIndex;
    public int turnsLeft;

    public ActionState state;

    //[Server]
    virtual public void Initialize(T playerActionInfo, NetworkIdentity playerOwner)
    {
        owner = playerOwner;
        //cellIndex = playerActionInfo.targetCellIndex;
        turnsLeft = playerActionInfo.actionTurns;
    }

    //[Server]
    private void Update()
    {
        switch (state)
        {
            case ActionState.STANDBY:
                StandbyPhase();
                break;
            case ActionState.ACTIVE:
                ActivePhase();
                break;
            case ActionState.DEAD:
                GoToPool();
                break;
        }
    }

    //[Server]
    public abstract override IEnumerator Launch();

    virtual protected void StandbyPhase()
    {

    }

    virtual protected void ActivePhase()
    {

    }

    virtual protected void GoToPool()
    {
        gameObject.SetActive(false);
    }
}
