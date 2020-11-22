/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mirror;
public class ScriptableElement : NetworkBehaviour
{
    [Header("Base Properties")]
    public string specialName = "";

    [Header("Tooltip")]
    [TextArea(1, 30)] public string toolTip = "";

    // instantiated tooltip
    Transform canvas;
    GameObject current;

    private void Awake()
    {
        canvas = GameObject.Find("Canvas").transform;
    }

    public string displayName
    {
        get
        {
            string tmp = specialName;
            if (specialName.Length == 0)
                tmp = this.name;
            return tmp;
        }
        set
        {
            if (value == this.name)
                specialName = "";
            else
                specialName = value;
        }
    }
    // Tooltip part
    void CreateToolTip()
    {
        if (!current)
        {
            Player player = Player.localPlayer;
            if (player != null)
            {
                float distance = Vector3.Distance(player.transform.position, transform.position);
                if (distance < player.distanceDetectionPerson)
                {
                    // instantiate
                    current = Instantiate(Universal.ToolTipPrefab, Input.mousePosition, Quaternion.identity);

                    // put to foreground
                    current.transform.SetParent(canvas, true); // canvas
                    current.transform.SetAsLastSibling(); // last one means foreground
                    current.GetComponentInChildren<Text>().text = ToolTip();
                }
            }
        }
    }
    bool isTooltip = false;
    void DisplayToolTip(float delay)
    {
        Invoke("CreateToolTip", delay);
        isTooltip = true;
    }
    void DestroyToolTip()
    {
        // stop any running attempts to show it
        CancelInvoke("CreateToolTip");
        // destroy it
        Destroy(current);
        isTooltip = false;
    }
    public void OnMouseOver()
    {
        if (!Utils.IsCursorOverUserInterface() && !isTooltip)
        {
            DisplayToolTip(0.5f);
        }
        if (Utils.IsCursorOverUserInterface() && isTooltip)
        {
            DestroyToolTip();
        }
    }
    public void OnMouseExit()
    {
        DestroyToolTip();
    }
    void OnDisable()
    {
        DestroyToolTip();
    }
    void OnDestroy()
    {
        DestroyToolTip();
    }
    public virtual string ToolTip()
    {
        // we use a StringBuilder so it is easy to modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        StringBuilder tip = new StringBuilder(toolTip);
        tip.Replace("{NAME}", displayName);
        return tip.ToString();
    }

}
