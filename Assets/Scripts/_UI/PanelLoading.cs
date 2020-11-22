/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/

using UnityEngine;
using UnityEngine.UI;

public class PanelLoading : MonoBehaviour
{
    public GameObject panel;
    public Slider loadProgress;

    private float _blackTime = GlobalVar.loadingBlackSeconds;
    private float _fadeTime = GlobalVar.loadingFadeSeconds;

    private float startTime;
    private bool isShown;
    private CanvasGroup canvasGroup;

    // Start is called before the first frame update
    void Awake()
    {
        canvasGroup = panel.GetComponent<CanvasGroup>();
    }

    public void Activate(float blackTime = GlobalVar.loadingBlackSeconds, float fadeTime = GlobalVar.loadingFadeSeconds)
    {
        this._blackTime = blackTime;
        this._fadeTime = fadeTime;
        panel.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (panel.activeSelf)
        {
            if (!isShown)
            {
                // first after activated
                startTime = Time.time;
                isShown = true;
            }
            float elapsed = Time.time - startTime;
            if (elapsed <= _blackTime)
            {
                canvasGroup.alpha = 1f;
                loadProgress.value = elapsed / _blackTime;
            }
            else if (elapsed > _blackTime + _fadeTime)
            {
                panel.SetActive(false); 
                canvasGroup.alpha = 1f;
            }
            else
            {
                canvasGroup.alpha = 1f - ((elapsed - _blackTime) / _fadeTime);
            }
        }
        else
        {
            isShown = false;
        }
    }
}

