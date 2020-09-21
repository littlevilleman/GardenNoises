using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GnomeUISticker : Sticker
{
    public override void InitializeSticker()
    {        
        transform.SetParent(UIManager.instance.initialArmyStickersPanel);

        //reset aspect
        transform.localScale = new Vector3(1f, 1f, 1f);
        rt.localPosition = new Vector3(0f, 0f, 0f);
        GetComponent<UnityEngine.UI.Image>().SetNativeSize();
        image.sprite = initialSprite;
    }
}
