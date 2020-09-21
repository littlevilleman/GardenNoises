using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BootyBox : GardenActor
{
    public SyncListActor bootyList = new SyncListActor();

    new protected void Awake()
    {
        base.Awake();
        bootyList.Callback += OnBootyListChanged;
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        _renderer.sprite = GardenManager.instance.GetBoxSprite(cellSize);

        if (bootyList.Count > 0) return;

        //CmdCreateBooty();
        if (isServer)
            CreateBooty();
        else
            CmdCreateBooty();
    }

    void OnBootyListChanged(SyncListActor.Operation op, int index, NetworkIdentity oldItem, NetworkIdentity newItem)
    {
        if (!hasAuthority) return;

        switch (op)
        {
            case SyncListActor.Operation.OP_ADD:
                ClientScene.localPlayer.GetComponent<GardenPlayer>().CmdAddBooty(newItem);
                break;
            case SyncListActor.Operation.OP_REMOVEAT:
                ClientScene.localPlayer.GetComponent<GardenPlayer>().CmdRemoveBooty(oldItem);
                break;
        }
    }

    [Command]
    public void CmdCreateBooty()
    {
        CreateBooty();
    }

    public void CreateBooty()
    {
        foreach (Vector3Int index in cellIndex)
        {
            Booty booty = GardenPool.instance.GetFromPool(GardenPool.instance.bootyPrefab.GetComponent<NetworkIdentity>().assetId).GetComponent<Booty>();
            //update actor and grids in client view
            booty.transform.SetParent(transform);

            // spawn actor on client, custom spawn handler is called
            NetworkServer.Spawn(booty.gameObject, connectionToClient);
            booty.cellIndex.Add(index);

            //add actor to actors list
            bootyList.Add(booty.GetComponent<NetworkIdentity>());
        }
    }
}
