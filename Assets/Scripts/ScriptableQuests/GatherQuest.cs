/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// a simple gather quest example
using UnityEngine;
using System.Text;
[CreateAssetMenu(menuName="uMMORPG/Quest/Gather Quest", order=999)]
public class GatherQuest : ScriptableQuest
{
    [Header("Fulfillment")]
    public ScriptableItem gatherItem;
    public int gatherAmount;
    // fulfillment /////////////////////////////////////////////////////////////
    public override bool IsFulfilled(Player player, Quest quest)
    {
        return gatherItem != null &&
               player.InventoryCount(new Item(gatherItem),player.ContainerIdOfBackpack()) >= gatherAmount;
    }
    public override void OnCompleted(Player player, Quest quest)
    {
        // remove gathered items from player's inventory
        if (gatherItem != null)
            player.InventoryRemove(new Item(gatherItem), gatherAmount,player.ContainerIdOfBackpack());
    }
    // tooltip /////////////////////////////////////////////////////////////////
    public override string ToolTip(Player player, Quest quest)
    {
        // we use a StringBuilder so it is easy to modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        StringBuilder tip = new StringBuilder(base.ToolTip(player, quest));
        tip.Replace("{GATHERAMOUNT}", gatherAmount.ToString());
        if (gatherItem != null)
        {
            int gathered = player.InventoryCount(new Item(gatherItem),player.ContainerIdOfBackpack());
            tip.Replace("{GATHERITEM}", gatherItem.name);
            tip.Replace("{GATHERED}", Mathf.Min(gathered, gatherAmount).ToString());
        }
        return tip.ToString();
    }
}
