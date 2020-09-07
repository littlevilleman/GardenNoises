using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GardenManager : MonoBehaviour
{
    public static GardenManager instance;

    public Grid grid;

    //tilemaps
    public Tilemap staticMap; // 0
    public Tilemap holesMap; // 5
    public Tilemap scrubsMap; // 10


    public Tilemap enemyScrubsMap; 

    //grid bounds
    public Tilemap staticGridBoundsMap; // 50
    public Tilemap armyGridBoundsMap; // 50

    //grid cursor debug
    public Transform gridCursorTx;

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
}
