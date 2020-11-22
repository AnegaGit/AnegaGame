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
using Mirror;
public class UILatency : MonoBehaviour
{
    public Text latencyText;
    public float goodThreshold = 0.3f;
    public float okayThreshold = 2;
    public Color goodColor = Color.green;
    public Color okayColor = Color.yellow;
    public Color badColor = Color.red;
    void Update()
    {
        // change color based on status
        if (NetworkTime.rtt <= goodThreshold)
            latencyText.color = goodColor;
        else if (NetworkTime.rtt <= okayThreshold)
            latencyText.color = okayColor;
        else
            latencyText.color = badColor;
        // show latency in milliseconds
        latencyText.text = Mathf.Round((float)NetworkTime.rtt * 1000) + "ms";
    }
}
