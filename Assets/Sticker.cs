using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public abstract class Sticker : NetworkBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    protected RectTransform rt;
    protected Image image;
    protected Vector3 initialLocation;
    protected Transform initialContainer;

    //booty box 
    public GameObject stickerActor;

    //cell location
    public Vector3Int cellSize;
    protected Vector3Int cellOffset;
    protected Vector3Int draggingIndex;
    //protected Vector3Int lastValidIndex;

    public Sprite initialSprite;
    public Sprite draggingSprite;

    //state
    public bool inGame;
    public bool isDragging;

    //context bounds
    protected Vector3Int contextBoundsMax = new Vector3Int(9, 9, 0);
    protected Vector3Int contextBoundsMin = new Vector3Int(0, 0, 0); 

    protected void Awake()
    {
        //init values
        rt = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        
        //dragging offsets depending of size
        cellOffset.x = cellSize.x == 2 ? 1 : cellSize.x == 3 ? 2 : 0;
        cellOffset.y = cellSize.y == 2 ? 1 : cellSize.y == 3 ? 2 : 0;

        //init dragging index
        draggingIndex = new Vector3Int(-15, -15, 0);
    }

    [ClientRpc]
    virtual public void RpcInitializeSticker()
    {
        if (!hasAuthority) return;

        //set parent container
        initialContainer = cellSize.x == 1 ? UIManager.instance.horizontalStickersArea : UIManager.instance.verticalStickersArea;
        transform.SetParent(initialContainer);

        //reset aspect
        transform.localScale = new Vector3(1f, 1f, 1f);
        GetComponent<UnityEngine.UI.Image>().SetNativeSize();

    }

    virtual public void OnBeginDrag(PointerEventData eventData)
    {
        if (inGame) return;

        isDragging = true;

        //show army grid
        GardenManager.instance.armyGridBoundsMap.GetComponent<Animator>().SetBool("show", true);
        //UIManager.instance.initialPhaseTutorial.GetComponent<Animator>().SetBool("show", false);
    }

    virtual public void OnDrag(PointerEventData eventData)
    {
        if (inGame) return;

        //get cell index location
        Vector3Int _newIndex = GardenManager.instance.grid.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        
        //check has moved
        if (_newIndex == draggingIndex)
            return;

        //check is valid cell
        if (_newIndex.y < 0 || _newIndex.x > 9)
        {
            //take back sticker to inventory
            TakeBack();
            return;
        }

        image.sprite = draggingSprite;

        //clamp index to garden bounds with sticker cell offset
        _newIndex.x = Mathf.Clamp(_newIndex.x, contextBoundsMin.x, contextBoundsMax.x - cellOffset.x);
        _newIndex.y = Mathf.Clamp(_newIndex.y, contextBoundsMin.y, contextBoundsMax.y - cellOffset.y);
        _newIndex.z = 0;

        //check is valid cell index
        if (!IsValidLocation(_newIndex))
            return;

        //set in dragging area and update dragging position
        draggingIndex = _newIndex;
        rt.SetParent(UIManager.instance.stickersTemporalArea);
        transform.position = Camera.main.WorldToScreenPoint(GardenManager.instance.grid.GetCellCenterWorld(draggingIndex));
    }

    virtual public void OnEndDrag(PointerEventData eventData)
    {
        if (inGame) return;

        //check is valid cell
        if (!IsValidLocation(draggingIndex))
        {
            //take back sticker to inventory
            TakeBack();
            return;
        }

        //set in game
        SetInGame(draggingIndex);
    }

    public virtual void ForceSetInGame()
    {
        Vector3Int _chanceLocation = new Vector3Int(-15, -15, 0);

        //look for random valid location
        do
        {
            _chanceLocation.x = UnityEngine.Random.Range(0, 9 - cellOffset.x);
            _chanceLocation.y = UnityEngine.Random.Range(0, 9 - cellOffset.y);
        }
        while (!IsValidLocation(_chanceLocation));

        Debug.Log("CHANCE LOCATION " + _chanceLocation);
        SetInGame(_chanceLocation);

    }

    void SetInGame(Vector3Int location)
    {
        //set in game
        Vector3Int [] cellIndex = CreateCellIndexFromLocation(location);
        ClientScene.localPlayer.GetComponent<GardenPlayer>().CmdSetInGame(stickerActor.GetComponent<NetworkIdentity>().assetId, cellSize, cellIndex);
        
        //army grid & tutorial visibility
        GardenManager.instance.armyGridBoundsMap.GetComponent<Animator>().SetBool("show", false);
        UIManager.instance.initialPhaseTutorial.GetComponent<Animator>().SetBool("show", !ClientScene.localPlayer.GetComponent<GardenPlayer>().turnDone);
        
        if(location.x >= 0)
        {
            GardenManager.instance.UpdateArmyGrid(cellIndex, true);
            GardenManager.instance.UpdateScrubGrid(cellIndex);
        }

        //end drag
        isDragging = false;
        inGame = true;
        gameObject.SetActive(false);
    }

    void TakeBack()
    {
        rt.SetParent(initialContainer);
        rt.localPosition = initialLocation;
        image.sprite = initialSprite;
    }

    //check is valid cell index
    protected bool IsValidLocation(Vector3Int source)
    {
        for (int ix = 0; ix < cellSize.x; ix++)
        {
            for (int iy = 0; iy < cellSize.y; iy++)
            {
                var t = GardenManager.instance.armyGridBoundsMap.GetTile(source + new Vector3Int(ix, iy, 0)) as DynamicTile;

                if (t != null && t.tileType == GardenTileType.GRID_INNER)
                    return false;
            }
        }
        return true;
    }

    protected Vector3Int[] CreateCellIndexFromLocation(Vector3Int source)
    {
        Vector3Int[] _newCellIndex = new Vector3Int[cellSize.x * cellSize.y];

        int _indexRef = 0;

        for (int x = 0; x < cellSize.x; x++)
        {
            for (int y = 0; y < cellSize.y; y++)
            {
                _newCellIndex[_indexRef] = source + new Vector3Int(x, y, 0);
                _indexRef++;
            }
        }

        return _newCellIndex;
    }
}
