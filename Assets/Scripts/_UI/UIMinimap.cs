/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public partial class UIMinimap : MonoBehaviour
{
    public GameObject panel;
    public float zoomMin = 5;
    public float zoomMax = 50;
    public float zoomStepSize = 5;
    public Text sceneText;
    public Button plusButton;
    public Button minusButton;
    public Camera minimapCamera;
    public GameObject compass;
    public GameObject mapMask;
    public GameObject dayRight;
    public GameObject dayLeft;
    public GameObject nightRight;
    public GameObject nightLeft;
    public GameObject watchHand;
    public Sprite limitedViewMask;

    private bool playerIsKnown = false;
    void Start()
    {
        plusButton.onClick.SetListener(() =>
        {
            minimapCamera.orthographicSize = Mathf.Max(minimapCamera.orthographicSize - zoomStepSize, zoomMin);
        });
        minusButton.onClick.SetListener(() =>
        {
            minimapCamera.orthographicSize = Mathf.Min(minimapCamera.orthographicSize + zoomStepSize, zoomMax);
        });
        // change clock
        InvokeRepeating("UpdateClock", 0, 10.0f);
    }
    void Update()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            if (!playerIsKnown)
            {
                panel.SetActive(true);
                if (player.abilities.compass == Abilities.Nav)
                    compass.SetActive(false);
                if (player.abilities.mentalMap == Abilities.Nav)
                    mapMask.SetActive(false);
                if (player.abilities.mentalMap == Abilities.Poor)
                    mapMask.GetComponent<Image>().sprite = limitedViewMask;
                if (player.abilities.mentalMap != Abilities.Excellent)
                {
                    plusButton.gameObject.SetActive(false);
                    minusButton.gameObject.SetActive(false);
                }
                if (player.abilities.innerClock >= Abilities.Good)
                {
                    GameTime gt = new GameTime();
                    dayLeft.GetComponent<Image>().fillAmount = (float)gt.CurrentDayPortion / 2;
                    dayRight.GetComponent<Image>().fillAmount = (float)gt.CurrentDayPortion / 2;
                    nightLeft.GetComponent<Image>().fillAmount = (float)gt.CurrentNightPortion / 2;
                    nightRight.GetComponent<Image>().fillAmount = (float)gt.CurrentNightPortion / 2;
                }
                playerIsKnown = true;
            }
            minimapCamera.transform.rotation = Quaternion.Euler(90f, player.transform.eulerAngles.y, 0);
            float compassY = player.transform.eulerAngles.y;
            if (player.abilities.compass == Abilities.Poor)
            {
                compassY = ApproximateAngle(compassY, player.divergenceCompass, 90);
            }
            else if (player.abilities.compass == Abilities.Good)
            {
                compassY = ApproximateAngle(compassY, player.divergenceCompass, 45);
            }
            compass.transform.rotation = Quaternion.Euler(0, 0, compassY);
            if(Universal.areaInfos.TryGetValue(player.currentArea, out AreaInfo value))
            {
                sceneText.text = value.displayName;
            }
        }
        else
        {
            panel.SetActive(false);
            playerIsKnown = false;
        }
    }
    void UpdateClock()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            GameTime gt = new GameTime();
            if (player.abilities.innerClock == Abilities.Nav)
            {
                if (gt.Daylight < 0.5 || !player.canSeeSky)
                    watchHand.transform.rotation = Quaternion.Euler(0, 0, 0);
                else if (gt.PartOfDay < 0.5)
                    watchHand.transform.rotation = Quaternion.Euler(0, 0, -135);
                else
                    watchHand.transform.rotation = Quaternion.Euler(0, 0, 135);
            }
            else if (player.abilities.innerClock == Abilities.Poor)
            {
                if (!player.canSeeSky)
                    watchHand.transform.rotation = Quaternion.Euler(0, 0, 0);
                else
                    watchHand.transform.rotation = Quaternion.Euler(0, 0, (float)(-360 * gt.PartOfDay + player.divergenceClock * 50));
            }
            else if (player.abilities.innerClock == Abilities.Good)
            {
                if (!player.canSeeSky)
                    watchHand.transform.rotation = Quaternion.Euler(0, 0, (float)(-360 * gt.PartOfDay + player.divergenceClock * 50));
                else
                    watchHand.transform.rotation = Quaternion.Euler(0, 0, (float)(-360 * gt.PartOfDay + player.divergenceClock * 10));
            }
            else
                watchHand.transform.rotation = Quaternion.Euler(0, 0, (float)(-360 * gt.PartOfDay));

        }
    }
    float ApproximateAngle(float angle, float divergence, int steps)
    {
        float obfuscatedAngle = angle + divergence * steps / 2.0f;
        return ((int)((Mathf.Abs(obfuscatedAngle) + steps / 2.0f) / steps)) * Mathf.Sign(obfuscatedAngle) * steps;
    }
}
