using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IPlayerAction : MonoBehaviour
{
    //[Server]
    public abstract IEnumerator Launch();
}
