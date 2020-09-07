using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileState
{
    CLEAN, BUSH, FIRED
}

public class GardenTile : MonoBehaviour
{
    public TileState state;
    
    private void LateUpdate()
    {
        UpdateTileSprite();
    }

    private void UpdateTileSprite()
    {
        if (state == TileState.CLEAN)
        {

        }
        else if (state == TileState.BUSH)
        {

        }
    }
}
