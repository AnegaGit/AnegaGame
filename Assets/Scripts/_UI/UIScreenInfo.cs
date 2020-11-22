/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using UnityEngine;

public class UIScreenInfo : MonoBehaviour
{
    //screenshot
    public GameObject panelScreenshot;
    public float delayScreenshot;
    public float showTimeScreenshot;

    private CanvasGroup canvasGroupScreenshot;
    private bool showScreenshot;
    private float timeScreenshot;

    void Awake()
    {
        canvasGroupScreenshot = panelScreenshot.GetComponent<CanvasGroup>();
    }

    void Update()
    {

        if (showScreenshot)
        {
            float elapsed = Time.time - timeScreenshot;
            if (elapsed > delayScreenshot)
            {
                panelScreenshot.SetActive(true);
                canvasGroupScreenshot.alpha = 1 - (elapsed - delayScreenshot) / showTimeScreenshot;
                if (elapsed > (delayScreenshot + showTimeScreenshot))
                {
                    showScreenshot = false;
                    panelScreenshot.SetActive(false);
                }
            }
        }
    }

    public void Screenshot()
    {
        showScreenshot = true;
        timeScreenshot = Time.time;
    }
}
