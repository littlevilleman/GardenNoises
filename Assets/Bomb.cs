using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : GardenActor
{
    protected override IEnumerator ActionProcess(CellInfo cellInfo)
    {
        //update animation
        animator.SetBool("action", true);
        yield return new WaitForSeconds(1f);

        yield return base.ActionProcess(cellInfo);
    }
}
