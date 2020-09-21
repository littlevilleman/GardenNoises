using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GardenManager : MonoBehaviour
{
    public static GardenManager instance;

    public Grid grid;

    //tilemaps
    [HeaderAttribute("Maps")]
    public Tilemap staticMap; // 0
    public Tilemap holesMap; // 5
    public Tilemap scrubsMap; // 10
    public Tilemap staticGridBoundsMap; // 50
    public Tilemap armyGridBoundsMap; // 50

    public Tilemap enemyScrubsMap;

    //grid cursor debug
    public Transform gridCursorTx;

    [HeaderAttribute("References")]
    public Sprite gridCursorReference;
    public Tile holeTileReference;
    public Sprite emptyBootyBox1x2Reference;
    public Sprite emptyBootyBox1x3Reference;
    public Sprite emptyBootyBox2x1Reference;
    public Sprite emptyBootyBox2x2Reference;
    public Sprite emptyBootyBox3x1Reference;

    public DynamicTile gridInnerTileReference;
    public DynamicTile gridOuterTileReference;
    public DynamicTile grassTileReference;

    private void Awake()
    {
        if (instance != null)
            Destroy(this);

        instance = this;
    }

    public void UpdateArmyGrid(Vector3Int[] cellIndex, bool fillCell)
    {
        foreach (Vector3Int v in cellIndex)
        {
            armyGridBoundsMap.SetTile(v, gridInnerTileReference);
            armyGridBoundsMap.RefreshTile(v);
        }
    }

    public void UpdateScrubGrid(Vector3Int[] cellIndex)
    {
        var _map = scrubsMap;

        if (cellIndex[0].x < 0)
            _map = enemyScrubsMap;

        foreach (Vector3Int v in cellIndex)
        {
            Debug.Log("CELL " + v);
            _map.SetTile(v, grassTileReference);
            _map.RefreshTile(v);
        }
    }

    public Sprite GetBoxSprite(Vector3Int boxSize)
    {
        //horizontal
        if (boxSize.y == 1)
        {
            if (boxSize.x == 2)
                return GardenManager.instance.emptyBootyBox1x2Reference;
            else
                return GardenManager.instance.emptyBootyBox1x3Reference;
        }
        //square & vertical
        else if (boxSize.y == 2)
        {
            if (boxSize.x == 1)
                return GardenManager.instance.emptyBootyBox2x1Reference;
            else
                return GardenManager.instance.emptyBootyBox2x2Reference;
        }
        //vertical
        else if (boxSize.y == 3)
        {
            return GardenManager.instance.emptyBootyBox3x1Reference;
        }

        return null;
    }
}
