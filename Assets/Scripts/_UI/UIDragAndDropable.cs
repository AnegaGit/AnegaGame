/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Drag and Drop support for UI elements. Drag and Drop actions will be sent to
// the local player GameObject.
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class UIDragAndDropable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    // drag options
    public PointerEventData.InputButton button = PointerEventData.InputButton.Left;
    public GameObject drageePrefab;
    public int container;
    public int slot;
    GameObject currentlyDragged;
    // status
    public bool dragable = true;
    public bool dropable = true;
    [HideInInspector] public bool draggedToSlot = false;
    public void OnBeginDrag(PointerEventData d)
    {
        // one mouse button is enough for dnd
        if (dragable && d.button == button)
        {
            // load current
            currentlyDragged = Instantiate(drageePrefab, transform.position, Quaternion.identity);
            currentlyDragged.GetComponent<Image>().sprite = GetComponent<Image>().sprite;
            currentlyDragged.transform.SetParent(transform.root, true); // canvas
            currentlyDragged.transform.SetAsLastSibling(); // move to foreground
            // disable button while dragging so onClick isn't fired if we drop a
            // slot on itself
            GetComponent<Button>().interactable = false;
        }
    }
    public void OnDrag(PointerEventData d)
    {
        // one mouse button is enough for drag and drop
        if (dragable && d.button == button)
            // move current
            currentlyDragged.transform.position = d.position;
    }
    // called after the slot's OnDrop
    public void OnEndDrag(PointerEventData d)
    {
        // delete dragged icon in any case
        Destroy(currentlyDragged);
        // one mouse button is enough for drag and drop
        if (dragable && d.button == button)
        {
            // try destroy if not dragged to a slot (flag will be set by slot)
            // message is sent to drag and drop handler for game specifics
            // -> only if dropping it into nirvana. do nothing if we just drop
            //    it on a panel. otherwise item slots are cleared if we
            //    accidentally drop it on the panel between two slots
            if (!draggedToSlot && d.pointerEnter == null)
            {
                if (tag=="EquipmentSlot" || tag=="InventorySlot")
                {
                    UIDragAndDropable dropDragable = d.pointerDrag.GetComponent<UIDragAndDropable>();
                    if (dropDragable != null && dropDragable.dragable)
                    {
                    Player.localPlayer.SendMessage("OnDragAndDrop_PutAway",
                                                   new int[,] { { dropDragable.container, dropDragable.slot }, { (int)d.position.x, (int)d.position.y } },
                                                   SendMessageOptions.DontRequireReceiver);
                    }
                }
                else
                    // send a drag and clear message like
                    // OnDragAndClear_Spellbar({index})
                    Player.localPlayer.SendMessage("OnDragAndClear_" + tag,
                                                   name.ToInt(),
                                                   SendMessageOptions.DontRequireReceiver);
            }
            // reset flag
            draggedToSlot = false;
            // enable button again
            GetComponent<Button>().interactable = true;
        }
    }
    // d.pointerDrag is the object that was dragged
    public void OnDrop(PointerEventData d)
    {
        // one mouse button is enough for drag and drop
        if (dropable && d.button == button)
        {
            // was the dropped GameObject a UIDragAndDropable and was it dragable?
            // (Unity calls OnDrop even if .dragable was false)
            UIDragAndDropable dropDragable = d.pointerDrag.GetComponent<UIDragAndDropable>();
            if (dropDragable != null && dropDragable.dragable)
            {
                // let the dragable know that it was dropped onto a slot
                dropDragable.draggedToSlot = true;
                // only do something if we didn't drop it on itself. this way we
                // don't have to ignore raycasts etc.
                // message is sent to drag and drop handler for game specifics
                if (dropDragable != this)
                {
                    // send a drag and drop message like
                    // OnDragAndDrop_Spellbar_Inventory({from, to})
                    Player.localPlayer.SendMessage("OnDragAndDrop_" + dropDragable.tag + "_" + tag,
                                                   new int[,] { {dropDragable.container, dropDragable.slot }, {container, slot } },
                                                   SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
    void OnDisable()
    {
        Destroy(currentlyDragged);
    }
    void OnDestroy()
    {
        Destroy(currentlyDragged);
    }
}
