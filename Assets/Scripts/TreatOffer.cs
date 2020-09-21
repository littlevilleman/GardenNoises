using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TreatOffer : MonoBehaviour
{
    public GameObject offerPrefab;

    RectTransform rt;

    public int cost;

    public TMP_Text costText;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    public void ResetOffer(int round)
    {        
        //reset transform
        transform.SetParent(UIManager.instance.offersArea);
        rt.localScale = new Vector3(1f, 1f, 1f);
        GetComponent<UnityEngine.UI.Image>().SetNativeSize();

        //reset content
        Refresh();
    }

    public void Refresh()
    {
        bool _available = ClientScene.localPlayer.GetComponent<GardenPlayer>().bootyCurrency >= cost;
        costText.text = "<color=#" + (_available ? "08415c" : "cc2936") + ">" + cost + "</color>";
        GetComponent<Button>().interactable = _available;
    }

    public void OnClickOffer()
    {
        GameObject sticker = GardenPool.instance.GetFromPool(offerPrefab);

        sticker.GetComponent<Sticker>().InitializeSticker();

        ClientScene.localPlayer.GetComponent<GardenPlayer>().CmdLoseCurrency(cost);

        Die();
    }

    public void Die()
    {
        GardenPool.instance.PutBackInLocalPool(gameObject);
    }
}
