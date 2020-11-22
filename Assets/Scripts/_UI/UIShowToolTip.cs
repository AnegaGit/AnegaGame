/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Instantiates a tooltip while the cursor is over this UI element.
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class UIShowToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea(1, 30)] public string text = "";
    // instantiated tooltip
    GameObject current;
    Text displayText;
    void CreateToolTip()
    {
        // instantiate
        current = Instantiate(Universal.ToolTipPrefab, Input.mousePosition, Quaternion.identity);
        // put to foreground
        current.transform.SetParent(transform.root, true); // canvas
        current.transform.SetAsLastSibling(); // last one means foreground
        displayText = current.transform.Find("ToolTipPanel/Text").gameObject.GetComponent<Text>();
    }
    void ShowToolTip(float delay)
    {
        Invoke("CreateToolTip", delay);
    }
    void DestroyToolTip()
    {
        // stop any running attempts to show it
        CancelInvoke("CreateToolTip");
        // destroy it
        Destroy(current);
    }
    public void OnPointerEnter(PointerEventData d)
    {
        ShowToolTip(0.5f);
    }
    public void OnPointerExit(PointerEventData d)
    {
        DestroyToolTip();
    }
    void Update()
    {
        // always copy text to tooltip. it might change dynamically when
        // swapping items etc., so setting it once is not enough.
        if (current) displayText.text = text;
    }
    void OnDisable()
    {
        DestroyToolTip();
    }
    void OnDestroy()
    {
        DestroyToolTip();
    }
}
