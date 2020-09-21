using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISyncIndex
{

}

[System.Serializable]
public class SyncIndex : SyncList<Vector3Int>
{
}
