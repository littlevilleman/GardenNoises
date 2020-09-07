using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActionBomb : PlayerAction<PlayerActionInfo>
{
    //[Server]
    public override IEnumerator Launch()
    {
        state = ActionState.ACTIVE;

        yield return null;
    }
}
