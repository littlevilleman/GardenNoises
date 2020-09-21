using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    [Header("UI Main")]
    public RectTransform roundBar;
    public RectTransform sideBar;
    public RectTransform resultsBar;

    [Header("Containers")]
    //panel references
    public RectTransform initialStickersPanel;
    public RectTransform initialBootyStickersPanel;
    public RectTransform initialArmyStickersPanel;
    public RectTransform armyPanel;
    public RectTransform offersPanel;
    public RectTransform stickerActionsPanel;

    //area references
    public RectTransform stickersTemporalArea;
    public RectTransform horizontalStickersArea;
    public RectTransform verticalStickersArea;
    public RectTransform offersArea;

    [Header("Buttons")]
    //action buttons
    public Button sideButton;
    public Button turnButton;

    [Header("Text")]
    //text references
    public TMP_Text resultsText;
    public TMP_Text bootyCurrencyText;

    [Header("Images")]
    //progress bar references
    public Image timeProgressBar;

    [Header("Sprites")]
    //Side button sprites
    public Sprite sideButtonNormalSprite;
    public Sprite sideButtonFocusSprite;
    public Sprite sideButtonEnableSprite;

    //turn button sprites
    public Sprite turnButtonNormalSprite;
    public Sprite turnButtonFocusSprite;
    public Sprite turnButtonEnableSprite;

    //local control values
    Vector3 sideBarInitialLocation;
    Animator sideBarAnimator;
    Animator actionPanelAnimator;

    //tutorial
    public Text initialPhaseTutorial;

    public static UIManager instance;
    
    void Awake()
    {
        //singleton
        if (instance != null)
            Destroy(this);

        instance = this;

        sideBarInitialLocation = sideBar.localPosition;
        sideBarAnimator = sideBar.GetComponent<Animator>();
        actionPanelAnimator = sideBar.GetComponent<Animator>();
        initialStickersPanel.gameObject.SetActive(true);
    }

    //switch ui state
    public void SwitchView(PlayerView view)
    {
        Debug.Log("currentResolution" + Screen.currentResolution);
        //TODO: move to start
        if (Screen.width > 1920)
            sideBar.anchorMax = sideBar.anchorMin = new Vector2(.91f, .5f);


        //camera & maps visibility
        GardenManager.instance.staticGridBoundsMap.GetComponent<Animator>().SetBool("show", view == PlayerView.SIDE);
        GardenManager.instance.scrubsMap.GetComponent<Animator>().SetBool("show", view != PlayerView.SIDE);
        CameraManager.instance.cameraAnimator.SetBool("showSide", view == PlayerView.SIDE);

        //ui & tutorial visibility
        //initialPhaseTutorial.GetComponent<Animator>().SetBool("show", view == PlayView.SIDE);
        sideBarAnimator.SetBool("show", view == PlayerView.SIDE);
        stickerActionsPanel.gameObject.SetActive(view != PlayerView.RESULTS);
        turnButton.gameObject.SetActive(view != PlayerView.RESULTS);
        resultsBar.gameObject.SetActive(view == PlayerView.RESULTS);

        sideButton.image.sprite = view == PlayerView.SIDE ? sideButtonEnableSprite : sideButtonNormalSprite;
        turnButton.image.sprite = view == PlayerView.SIDE ? turnButtonEnableSprite : turnButtonNormalSprite;
        //sideButton.image.sprite = view == PlayView.SIDE ? sideButtonEnableSprite : sideButtonNormalSprite;

        resultsText.text = "<color=#fef9ef>Player01</color><color=#ffcb77> has planted a bomb at</color><color=#cc2936> Player02</color><color=#ffcb77> garden</color>";
    }
    
    public void UpdateTimeProgress(float time)
    {
        timeProgressBar.fillAmount = time;
    }

    public void UpdateBootyCurrency(int bootyCurrency)
    {
        bootyCurrencyText.text = "" + bootyCurrency;

        foreach (TreatOffer offer in GetComponentsInChildren<TreatOffer>())
        {
            offer.Refresh();
        }
    }

    public void SwitchTurnDone(bool turnDone)
    {
        CameraManager.instance.cameraAnimator.SetBool("showSide", false);
        sideBarAnimator.SetBool("show", !turnDone);
        turnButton.image.sprite = turnButtonEnableSprite;
        //sideButton.interactable = !turnDone;
    }
}
