using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public enum ActorType
{
    GNOME, BOX, BOOTY, BOMB, SQUIRRELL, PARROT
}

public struct ActorInfo
{
    public uint netId;
    public Vector3Int[] cellIndex;
    public ActorType type;
}

[System.Serializable]
public class SyncListActor : SyncList<NetworkIdentity>{}

public class GardenPlayer : NetworkBehaviour
{
    // player details
    public string playerName;
    

    public SyncListActor ownedActorsList = new SyncListActor();

    [SyncVar(hook = nameof(OnView))]
    public PlayerView view;

    [SyncVar(hook = nameof(OnBootyCurrency))]
    public int bootyCurrency;

    [SyncVar(hook = nameof(OnTurnDone))]
    public bool turnDone;

    public override void OnStartClient()
    {
        if (!isLocalPlayer) return;

        //add listener controls
        UIManager.instance.sideButton.onClick.AddListener(OnClickSideView);
        UIManager.instance.turnButton.onClick.AddListener(OnClickTurnDone);
    }

    void Update()
    {
        if (!isLocalPlayer) return;
            MoveGridCursor();
    }

    //switch ui state
    public void OnClickSideView()
    {
        //switch view
        if (view == PlayerView.NORMAL) view = PlayerView.SIDE;
        else if (view == PlayerView.SIDE) view = PlayerView.NORMAL;

        UIManager.instance.SwitchView(view);

        UpdateView();
    }

    void UpdateView()
    {
        if (!isLocalPlayer) return;

        //show booty boxes only at side view
        foreach (NetworkIdentity _box in ownedActorsList)
        {
            if(_box.GetComponent<BootyBox>())
                _box.GetComponent<Animator>().SetBool("show", view == PlayerView.SIDE);
        }
    }

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

    //TURN DONE
    public void OnClickTurnDone()
    {
        if (!isLocalPlayer) return;

        CmdSwitchTurnDone();        
    }

    [Command]
    public void CmdSwitchTurnDone()
    {
        turnDone = !turnDone;    
    }
    
    public void OnTurnDone(bool oldTurnDone, bool newTurnDone)
    {
        if (!isLocalPlayer) return;

        //check initial stickers
        if (newTurnDone)
            foreach (Sticker sticker in UIManager.instance.sideBar.GetComponentsInChildren<Sticker>())
            {
                if (!sticker.inGame) sticker.ForceSetInGame();
            }

        UIManager.instance.SwitchTurnDone(newTurnDone);
    }

    [ClientRpc]
    public void RpcInitializePlayerContent()
    {
        if (!isLocalPlayer) return;

        //create initial stickers
        foreach (GameObject stickerPrefab in GardenPool.instance.bootyBoxStickerPrefabs)
        {
            GameObject _sticker = GardenPool.instance.GetFromPool(stickerPrefab);
            _sticker.GetComponent<Sticker>().InitializeSticker();
        }

        for (int i= 0; i< 3; i++)
        {
            GameObject _sticker = GardenPool.instance.GetFromPool(GardenPool.instance.gnomeStickerPrefab);
            _sticker.GetComponent<Sticker>().InitializeSticker();
        }
    }

    //VIEW
    void OnView(PlayerView oldView, PlayerView newView)
    {        
        UIManager.instance.SwitchView(newView);
        
        UpdateView();
    }
    
    //SERVER VALUES

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
    public void RpcStartNewRound(int round)
    {
        //gain booty currency
        GainRoundBootyCurrency();

        //reset treating offers
        ResetOffers(round);

        UIManager.instance.turnButton.image.sprite = UIManager.instance.turnButtonNormalSprite;
        UIManager.instance.offersPanel.gameObject.SetActive(true);
        UIManager.instance.initialStickersPanel.gameObject.SetActive(false);
    }

    //CURRENCY    
    public void GainRoundBootyCurrency()
    {
        if (!isLocalPlayer) return;

        int _currency = bootyCurrency;

        foreach (NetworkIdentity booty in ownedActorsList)
        {
            if (booty.GetComponent<Booty>())
                _currency++;
        }

        CmdSetBootyCurrency(_currency);
    }

    void ResetOffers(int round)
    {
        if (!isLocalPlayer) return;

        //clear old offers
        foreach (TreatOffer offer in UIManager.instance.offersArea.GetComponentsInChildren<TreatOffer>())
        {
            offer.Die();
        }

        GameObject _offerPrefab = GardenPool.instance.GetRandomOfferByRound(round);

        //add new offers by round
        for (int i = 0; i < 3; i++)
        {
            GameObject offer = GardenPool.instance.GetFromPool(_offerPrefab);
            offer.GetComponent<TreatOffer>().ResetOffer(round);
        }
    }

    public void OnBootyCurrency(int oldCurrency, int newCurrency)
    {
        if (!isLocalPlayer) return;

        UIManager.instance.UpdateBootyCurrency(newCurrency);
    }

    [Command]
    void CmdSetBootyCurrency(int currency)
    {
        bootyCurrency = currency;
    }

    [Command]
    public virtual void CmdPlayActor(System.Guid _assetId, Vector3Int _cellSize, Vector3Int[] _cellIndex)
    {
        //set up actor in server
        GardenActor actor = GardenPool.instance.GetFromPool(_assetId).GetComponent<GardenActor>();

        // spawn actor on client, custom spawn handler is called
        NetworkServer.Spawn(actor.gameObject, connectionToClient);

        actor.cellSize = _cellSize;
        actor.cellIndex.AddRange(_cellIndex);

        //add actor to player list
        if (!(actor as BootyBox))
            ownedActorsList.Add(actor.GetComponent<NetworkIdentity>());
    }

    [Command]
    public void CmdLoseCurrency(int cost)
    {
        bootyCurrency -= cost;
    }
    
    [Command]
    void CmdSetActorVisible(NetworkIdentity actor)
    {
        actor.GetComponent<GardenActor>().visible = true;
    }
    
    [ClientRpc]
    public void RPCUpdateScrubsMap(Vector3Int [] index)
    {
        GardenManager.instance.UpdateScrubGrid(index);
    }

    [Command]
    public void CmdAddBooty(NetworkIdentity booty)
    {
        ownedActorsList.Add(booty);
    }

    [Command]
    public void CmdRemoveBooty(NetworkIdentity booty)
    {
        ownedActorsList.Remove(booty);
    }
}
