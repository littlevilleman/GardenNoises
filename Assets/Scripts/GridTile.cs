using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;

public class GridTile : Tile
{
    public GardenTileType tileType;

    [Header("Sprites")]
    public Sprite[] m_Sprites;
    public Sprite[] m_EmptySprites;
    public Sprite m_Preview;

    public bool empty;

    // This refreshes itself and other RoadTiles that are orthogonally and diagonally adjacent
    public override void RefreshTile(Vector3Int location, ITilemap tilemap)
    {
        //var player = ClientScene.localPlayer.GetComponent<GardenPlayer>();
        //
        ////check is actor visible at location
        //foreach (GardenActor actor in player.gnomes)
        //{
        //    if (actor.cellIndex.Contains(location))
        //    {
        //
        //        //
        //    }
        //}

        //if ()
        //{
        //
        //}
        //if true then grid cell is null tile
        //if false then grid cell is fill tile

        //refresh aspect with neighbour cells
        for (int yd = -1; yd <= 1; yd++)
        {
            for (int xd = -1; xd <= 1; xd++)
            {
                    //check valid cell
                    Vector3Int position = new Vector3Int(location.x + xd, location.y + yd, location.z);
                    if (tilemap.GetTile(position) != null)
                        tilemap.RefreshTile(position);
            }
        }
    }

    // This determines which sprite is used based on the RoadTiles that are adjacent to it and rotates it to fit the other tiles.
    // As the rotation is determined by the RoadTile, the TileFlags.OverrideTransform is set for the tile.
    public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData)
    {
        int mask = 0;

        //mask position
        mask += IsSameState(tilemap, location + new Vector3Int(0, 1, 0)) ? 1 : 0;   //top
        mask += IsSameState(tilemap, location + new Vector3Int(1, 0, 0)) ? 2 : 0;   //right
        mask += IsSameState(tilemap, location + new Vector3Int(0, -1, 0)) ? 4 : 0;  //bot
        mask += IsSameState(tilemap, location + new Vector3Int(-1, 0, 0)) ? 8 : 0;  //left

        // update tile data
        int index = GetIndex((byte)mask);
        if (index >= 0 && index < m_Sprites.Length)
        {
            tileData.sprite = empty ? m_EmptySprites[index] : m_Sprites[index];
            tileData.color = Color.white;
            var m = tileData.transform;
            tileData.transform = m;
            //tileData.flags = TileFlags.LockTransform;
            tileData.colliderType = colliderType;
        }
        else
        {
            Debug.LogWarning("Not enough sprites in RoadTile instance");
        }
    }

    // This determines if the Tile at the position is the same Outer tile
    virtual protected bool IsSameState(ITilemap tilemap, Vector3Int position)
    {
        var t = tilemap.GetTile(position) as GridTile;

        //is same type of tile
        if (t != null)
        {
            return empty == t.empty;
        }

        return false;
    }

    // The following determines which sprite to use based on the number of adjacent RoadTiles
    protected int GetIndex(byte mask)
    {
        if (mask < 16)
            return mask;
        return 0;
    }

#if UNITY_EDITOR
    //The following is a helper that adds a menu item to create a RoadTile Asset
    [MenuItem("Assets/Create/GridTile")]
    public static void CreateSmartTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Tile", "New Tile", "Asset", "Save Tile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<GridTile>(), path);
    }
#endif
}
