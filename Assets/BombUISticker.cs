﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombUISticker : Sticker
{
    new protected void Awake()
    {
        base.Awake();
        contextBoundsMin = new Vector3Int(-10, 0, 0);
        contextBoundsMax = new Vector3Int(-1, 9, 0);
    }

    public override void RpcInitializeSticker()
    {
        if (!hasAuthority) return;

        //set parent container
        foreach (Transform t in UIManager.instance.stickerActionsPanel)
        {
            if (t.childCount == 0)
            {
                initialContainer = t;
                break;
            }
        }

        transform.SetParent(initialContainer);

        //reset aspect
        transform.localScale = new Vector3(1f, 1f, 1f);
        rt.localPosition = new Vector3(0f, 0f, 0f);
        GetComponent<UnityEngine.UI.Image>().SetNativeSize();
        image.sprite = initialSprite;


    }

}