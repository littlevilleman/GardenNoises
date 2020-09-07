using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BootyBox : GardenActor
{

    new protected void Awake()
    {
        base.Awake();
    }

    [ClientRpc]
    public override void RpcUpdateActor()
    {
        base.RpcUpdateActor();

        GetComponent<SpriteRenderer>().sprite = InitializeBoxSprite();

        var _fixedCellIndex = cellIndex;

        if (owner != ClientScene.localPlayer)
            _fixedCellIndex = GetInverseCellIndex(cellIndex);

        for (int i = 0; i < _fixedCellIndex.Count; i++)
        {
            var bootyTx = transform.GetChild(3 - i);
            bootyTx.position = GardenManager.instance.grid.GetCellCenterWorld(_fixedCellIndex[i]);
            bootyTx.gameObject.SetActive(true);
        }

        //TODO: Refresh enemy player maps
        float delay = 0f;

        foreach(Transform booty in transform)
        {
            booty.GetComponent<SpriteRenderer>().enabled = visible || owner == ClientScene.localPlayer;
            booty.GetComponent<Animator>().SetFloat("delay", delay);
            delay += .1f;
        }
    }

    Sprite InitializeBoxSprite()
    {
         Sprite _sprite = null;

        //horizontal
        if (boxSize.y == 1)
        {
            if (boxSize.x == 2)
                _sprite = ServerManager.instance.emptyBootyBox1x2Reference;
            else
                _sprite = ServerManager.instance.emptyBootyBox1x3Reference;
        }
        //square & vertical
        else if (boxSize.y == 2)
        {
            if (boxSize.x == 1)
                _sprite = ServerManager.instance.emptyBootyBox2x1Reference;
            else
                _sprite = ServerManager.instance.emptyBootyBox2x2Reference;
        }
        //vertical
        else if (boxSize.y == 3)
        {
            _sprite = ServerManager.instance.emptyBootyBox3x1Reference;
        }

        return _sprite;
    }

    public int GetBootyCount()
    {
        int bootyCount = 0;

        foreach(Transform booty in transform)
        {
            if (booty.gameObject.activeInHierarchy)
            {
                bootyCount += 1;
            }
        }

        return bootyCount;
    }

}
