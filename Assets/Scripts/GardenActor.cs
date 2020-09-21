using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GardenActor : NetworkVisibility
{
    protected SpriteRenderer _renderer;
    protected Animator animator;

    public SyncIndex cellIndex = new SyncIndex();

    [SyncVar]
    public Vector3Int cellSize;

    [SyncVar(hook = nameof(OnSetVisible))]
    public bool visible = false;

    [SyncVar(hook = nameof(OnLaunchAction))]
    public bool inAction = false;

    [SyncVar]
    public int roundsLeft = -1; //-1 = permanent

    public IEnumerator _actionProcess;
       
    protected void Awake()
    {
        animator = GetComponent<Animator>();
        _renderer = GetComponent<SpriteRenderer>();
        cellIndex.Callback += OnCellIndexChanged;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    void OnSetVisible(bool oldVisible, bool newVisible)
    {
        GetComponent<NetworkIdentity>().RebuildObservers(false);
    }

    public override bool OnCheckObserver(NetworkConnection conn)
    {
        if (cellIndex.Count == 0) return false;

        //check is owner or is visible
        return hasAuthority || visible;
    }

    public override void OnRebuildObservers(HashSet<NetworkConnection> observers, bool initialize)
    {
        if (cellIndex.Count == 0) return;

        if (!visible) return;

        NetworkIdentity newObserver;

        if (hasAuthority)
            newObserver = MatchManager.instance.GetEnemyPlayer(ClientScene.localPlayer.netId);
        else
            newObserver = ClientScene.localPlayer;

        observers.Add(newObserver.connectionToClient);
    }
    
    void OnCellIndexChanged(SyncIndex.Operation op, int index, Vector3Int oldItem, Vector3Int newItem)
    {
        if (op == SyncIndex.Operation.OP_ADD)
        {
            if (index == 0) UpdateLocalPosition();
        }
    }

    [ClientRpc]
    public void RpcUpdateActor()
    {
        UpdateLocalPosition();

        animator.SetBool("isEnemy", !hasAuthority);
    }

    void UpdateLocalPosition()
    {
        var _position = GardenManager.instance.grid.GetCellCenterWorld(cellIndex[0]);

        if (!hasAuthority)
        {
            _position = GardenManager.instance.grid.GetCellCenterWorld(GlobalUtils.GetInverseIndex(cellIndex[0]));
        }

        transform.position = _position;
    }

    void OnLaunchAction(bool oldIsAction, bool newIsAction)
    {
        animator.SetBool("action", newIsAction);
    }

    [ClientRpc]
    virtual public void RpcLaunchAction(CellInfo cellInfo)
    {
        //update cells
        List<Vector3Int> localCells = new List<Vector3Int>(cellIndex);

        if (!hasAuthority)
            localCells = new List<Vector3Int>(GlobalUtils.GetInverseIndex(cellIndex));

        GardenManager.instance.UpdateScrubGrid(localCells.ToArray());
    }

    virtual protected IEnumerator ActionProcess(CellInfo cellInfo)
    {
        yield return null;
        
        _actionProcess = null;
    }

    public void Die()
    {
        visible = false;        
        GetComponent<NetworkIdentity>().RemoveClientAuthority();
        GetComponent<NetworkIdentity>().RebuildObservers(true);
        GardenPool.instance.PutBackInNetworkPool(gameObject, GetComponent<NetworkIdentity>().assetId);
        NetworkServer.UnSpawn(gameObject);
    }

}
