using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TreatOffer : NetworkBehaviour
{
    protected RectTransform rt;

    public GameObject stickerPrefab;
    public int cost;

    public TMP_Text costText;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    [ClientRpc]
    public void RpcResetOffer(int round)
    {
        if (!hasAuthority) return;

        //set parent container
        transform.SetParent(UIManager.instance.offersArea);

        //reset aspect
        rt.localScale = new Vector3(1f, 1f, 1f);
        GetComponent<UnityEngine.UI.Image>().SetNativeSize();

        RefreshOffer();
    }

    public void RefreshOffer()
    {
        bool _available = ClientScene.localPlayer.GetComponent<GardenPlayer>().bootyCurrency >= cost;
        costText.text = "<color=#" + (_available ? "08415c" : "cc2936") + ">" + cost + "</color>";
        GetComponent<Button>().interactable = _available;
        
    }

    public void OnClickOffer()
    {
        ClientScene.localPlayer.GetComponent<GardenPlayer>().CmdBuyItem(stickerPrefab.GetComponent<NetworkIdentity>().assetId, cost);
        transform.parent = GardenPool.instance.stickersAreaReference;
        GardenPool.instance.UnSpawnActor(gameObject);

    }

    private void OnDestroy()
    {
        Die();
    }

    public void Die()
    {
        GardenPool.instance.UnSpawnActor(gameObject);
        NetworkServer.UnSpawn(gameObject);
    }
}
