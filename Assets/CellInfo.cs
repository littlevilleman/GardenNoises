using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CellInfo
{
    public Vector3Int cellIndex;

    public NetworkIdentity targetNetId;

    public bool hasScrubs; //has scrub
    public bool isHole;
    public bool isLake;

    //public System.Guid tileAssetId;

}
