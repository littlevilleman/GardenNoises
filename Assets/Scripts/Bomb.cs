using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : GardenActor
{
    public override void OnStartServer()
    {
        base.OnStartServer();        
        roundsLeft = 0;
    }

    protected override IEnumerator ActionProcess(CellInfo cellInfo)
    {
        //update animation
        yield return new WaitForSeconds(1f);

        yield return base.ActionProcess(cellInfo);
    }
}
