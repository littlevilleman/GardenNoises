using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GardenActor : NetworkBehaviour
{
    protected Renderer _renderer;
    protected Animator animator;

    public SyncIndex cellIndex = new SyncIndex();
    
    [SyncVar]
    public Vector3Int boxSize;

    [SyncVar]
    public NetworkIdentity owner;

    [SyncVar]
    public bool visible = false;
    
    [SyncVar]
    public int roundsLeft = -1; //-1 = permanent

    public IEnumerator _actionProcess;

    [SyncVar]
    public bool inAction;

    protected void Awake()
    {
        animator = GetComponent<Animator>();
        _renderer = GetComponent<SpriteRenderer>();
    }

    [ClientRpc]
    public virtual void RpcUpdateActor()
    {
        //update position
        Vector3 _position = GardenManager.instance.grid.GetCellCenterWorld(cellIndex[0]);

        if (owner != ClientScene.localPlayer)
            _position = GardenManager.instance.grid.GetCellCenterWorld(GlobalUtils.GetInverseGardenCellIndex(cellIndex[0]));

        transform.position = _position;

        //visibility
        _renderer.enabled = visible || owner == ClientScene.localPlayer;
    }

    [ClientRpc]
    virtual public void RpcLaunchAction(CellInfo cellInfo)
    {
        //update cells
        List<Vector3Int> localCells = new List<Vector3Int>(cellIndex);

        if (owner != ClientScene.localPlayer)
            localCells = new List<Vector3Int>(GetInverseCellIndex(cellIndex));

        GardenManager.instance.UpdateScrubGrid(localCells.ToArray());

        _actionProcess = ActionProcess(cellInfo);
        StartCoroutine(_actionProcess);
    }

    virtual protected IEnumerator ActionProcess(CellInfo cellInfo)
    {
        yield return null;

        _actionProcess = null;
        inAction = false;
    }

    private void OnDestroy()
    {
        Die();
    }

    public void Die()
    {
        GardenPool.instance.UnSpawnActor(gameObject);
        NetworkServer.UnSpawn(gameObject);
    }
    
    public static SyncIndex GetInverseCellIndex(SyncIndex cellIndex)
    {
        var inverse = new SyncIndex();
        var i = 0;

        foreach (Vector3Int _index in cellIndex)
        {
            var _invertIndex = _index;

            if (_index.x >= 0)
                _invertIndex -= new Vector3Int(10, 0, 0);
            else
                _invertIndex += new Vector3Int(10, 0, 0);

            inverse.Add(_invertIndex);
            i++;
        }

        return inverse;
    }
}
