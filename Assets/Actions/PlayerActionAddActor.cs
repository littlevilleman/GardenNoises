using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerActionAddActor : PlayerAction<PlayerActionAddActorInfo>
{
    ArmyType armyType;

    public override void Initialize(PlayerActionAddActorInfo playerActionInfo, NetworkIdentity playerOwner)
    {
        base.Initialize(playerActionInfo, playerOwner);
        armyType = playerActionInfo.armyType;
    }

    //[Server]
    public override IEnumerator Launch()
    {
        foreach(GardenPlayer player in ServerManager.instance.networkManager.players)
        {
            //if (player == owner)
                //player.GetComponent<GardenPlayer>().RpcSetInGarden(ArmyType armyType);
            //else
            //    player.GetComponent<GardenPlayer>().RpcSetInEnemyGarden();
        }

        yield return null;
    }
}
