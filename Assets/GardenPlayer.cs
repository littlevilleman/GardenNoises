using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class GardenPlayer : NetworkBehaviour
{


    // player details
    public string playerName;

    //[SyncVar]
    //public Hashtable gardenInfo;
    
    [SyncVar]
    public bool turnDone;
    
    public PlayView view;

    //army    
    public List<GardenActor> armyList = new List<GardenActor>();

    //public List<BootyBox> bootyBoxes;
    public List<TreatOffer> currentOffers;

    //treating
    public int bootyCurrency;

    //actions
    public int avaliableBombs;
    public int avaliableSquirrels;
    public int avaliableGnomes;
    
    //[SyncVar(hook = "OnColorChange")]
    Color color;

    public override void OnStartClient()
    {
        if (!isLocalPlayer) return;

        //add listener controls
        UIManager.instance.sideButton.onClick.AddListener(SwitchView);
        UIManager.instance.turnButton.onClick.AddListener(SwitchTurnDone);
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        MoveGridCursor();
    }

    //switch ui state
    public void SwitchView()
    {
        //switch view
        if (view == PlayView.NORMAL) view = PlayView.SIDE;
        else if (view == PlayView.SIDE) view = PlayView.NORMAL;

        UIManager.instance.SwitchView(view);

        UpdateView();
    }

    void UpdateView()
    {
        if (!isLocalPlayer) return;

        //show booty boxes only at side view
        foreach (GardenActor _box in armyList)
        {
            if(_box as BootyBox)
                _box.GetComponent<Animator>().SetBool("show", view == PlayView.SIDE);
        }
    }

    [Client]
    void MoveGridCursor()
    {
        //move cursor
        Vector3Int v = GardenManager.instance.grid.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));

        v.x = Mathf.Clamp(v.x, 0, 9);
        v.y = Mathf.Clamp(v.y, 0, 9);

        GardenManager.instance.gridCursorTx.position = GardenManager.instance.grid.GetCellCenterWorld(v);

        //cursor click
        if (Input.GetMouseButtonDown(0))
        {
            Vector3Int _cursorCellIndex = GardenManager.instance.grid.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            //playerScrubMap.SetTile(_cursorCellIndex, grassTileReference);
            //playerHoleMap.SetTile(_cursorCellIndex, holeTileReference);
        }   //
    }

    [ClientRpc]
    public void RpcInitializePlayerContent()
    {
        //create initial stickers
        foreach (GameObject sticker in GardenPool.instance.bootyBoxStickerPrefabs)
        {
            CmdBuyItem(sticker.GetComponent<NetworkIdentity>().assetId, 0);
        }

        for (int i= 0; i< 3; i++)
        {
            CmdBuyItem(GardenPool.instance.gnomeStickerAssetId, 0);
        }
    }


    [ClientRpc]
    public void RpcForceSwitchView(PlayView toView)
    {
        if (view == toView)
            return;

        view = toView;

        UIManager.instance.SwitchView(view);

        UpdateView();
    }

    [ClientRpc]
    public void RpcUpdatePhaseTime(float timePercent)
    {
        UIManager.instance.UpdateTimeProgress(timePercent);
    }

    [ClientRpc]
    public void RpcUpdateResults(string resultMsg)
    {
    }
    
    [ClientRpc]
    public void RpcGainRoundBootyCurrency()
    {
        int _roundCurrency = 0;

        foreach(GardenActor box in armyList)
        {
            if (box as BootyBox)
                _roundCurrency += (box as BootyBox).GetBootyCount();
        }
        bootyCurrency += _roundCurrency;

        UIManager.instance.UpdateBootyCurrency(bootyCurrency);
    }


    [ClientRpc]
    public void RpcStartNewRound(int round)
    {
        ServerManager.instance.SetRound(round);
        UIManager.instance.turnButton.image.sprite = UIManager.instance.turnButtonNormalSprite;
        UIManager.instance.offersPanel.gameObject.SetActive(true);
        UIManager.instance.initialStickersPanel.gameObject.SetActive(false);
    }

    [ClientRpc]
    public void RpcForceTurnDone()
    {
        foreach (Sticker sticker in UIManager.instance.sideBar.GetComponentsInChildren<Sticker>())
        {
            if (!sticker.inGame) sticker.ForceSetInGame();
        }

        SwitchTurnDone();
    }

    [ClientRpc]
    public void RpcForceResetTreating(int round)
    {
        CmdResetOffers(round);
    }

    [Command]
    public virtual void CmdSetInGame(System.Guid _assetId, Vector3Int _cellSize, Vector3Int[] _cellIndex)
    {
        //pull actor from pool
        GardenActor actor = GardenPool.instance.GetFromPool(_assetId).GetComponent<GardenActor>();

        // Set up actor on server
        actor.GetComponent<GardenActor>().owner = GetComponent<NetworkIdentity>();
        actor.GetComponent<GardenActor>().boxSize = _cellSize;
        actor.GetComponent<GardenActor>().cellIndex.Clear();
        actor.GetComponent<GardenActor>().cellIndex.AddRange(_cellIndex);

        // spawn actor on client, custom spawn handler is called
        NetworkServer.Spawn(actor.gameObject, _assetId);

        //update actor and grids in client view
        actor.GetComponent<GardenActor>().RpcUpdateActor();
        
        //add actor to player list
        armyList.Add(actor.GetComponent<GardenActor>());
    }

    public NetworkIdentity GetCellActor(Vector3Int _cellIndex)
    {
        foreach (GardenActor actor in armyList)
        {
            if (actor.cellIndex.Contains(_cellIndex))
            {
                return actor.GetComponent<NetworkIdentity>();
            }
        }

        return null;
    }

    [Command]
    void CmdResetOffers(int round)
    {
        foreach (TreatOffer offer in currentOffers)
        {
            offer.Die();
        }

        currentOffers.Clear();

        for (int i = 0; i < 3; i++)
        {
            TreatOffer offer = GardenPool.instance.GetFromPool(GardenPool.instance.GetRandomOfferByRound(round).GetComponent<NetworkIdentity>().assetId).GetComponent<TreatOffer>();
            
            NetworkServer.Spawn(offer.gameObject, connectionToClient);

            offer.RpcResetOffer(round);

            currentOffers.Add(offer);
        }
    }

    [Command]
    public void CmdBuyItem(System.Guid assetId, int cost)
    {
        GameObject sticker = GardenPool.instance.GetFromPool(assetId);

        NetworkServer.Spawn(sticker, assetId, connectionToClient);
        
        sticker.GetComponent<Sticker>().RpcInitializeSticker();

        bootyCurrency -= cost;

        UIManager.instance.UpdateBootyCurrency(bootyCurrency);
    }

    [ClientRpc]
    public void RPCUpdateScrubsMap(Vector3Int [] index)
    {
        GardenManager.instance.UpdateScrubGrid(index);
    }

    public void SwitchTurnDone()
    {
        if (turnDone)
            return;
        //TODO: turndone back
                
        turnDone = true;

        foreach (Sticker sticker in UIManager.instance.sideBar.GetComponentsInChildren<Sticker>())
        {
            if (!sticker.inGame) sticker.ForceSetInGame();
        }

        UIManager.instance.SwitchTurnDone(turnDone);
    }
}
