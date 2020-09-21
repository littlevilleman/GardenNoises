using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ArmyType
{
    GNOME, BOOTY_BOX, BOOTY
}

[System.Serializable]
public class PlayerActionAddActorInfo : PlayerActionInfo
{
    public ArmyType armyType;    
}