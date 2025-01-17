﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GlobalUtils
{
    //context bounds
    public Vector3Int contextBoundsMax = new Vector3Int(9, 9, 0);
    public Vector3Int contextBoundsMin = new Vector3Int(0, 0, 0);
    public static Vector3Int NULL_INDEX = new Vector3Int(-15, -15, 0);

    public static Tile[] FillTileArray(Vector3Int [] cellIndex, Tile tileReference)
    {
        //delete previous cells
        Tile[] tiles = new Tile[cellIndex.Length];

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = tileReference;
        }

        return tiles;
    }

    public static Vector3Int GetInverseIndex(Vector3Int index)
    {
        if (index.x >= 0)
            index -= new Vector3Int(10, 0, 0);
        else
            index += new Vector3Int(10, 0, 0);
    
        return index;
    }


    public static SyncIndex GetInverseIndex(SyncIndex cellIndex)
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
