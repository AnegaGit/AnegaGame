using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Money
{
    static public string itemNameCopper = "Copper Coin";
    static public string itemNameSilver = "Silver Coin";
    static public string itemNameGold = "Gold Coin";

    /// <summary>
    /// Returns the text for money
    /// </summary>
    public static string MoneyText(long money)
    {
        string moneyString = "";
        long gold = MoneyGold(money);
        long silver = MoneySilver(money);
        long copper = MoneyCopper(money);
        if (gold > 0)
            moneyString = gold.ToString() + " Gold";
        if (silver > 0)
            moneyString += " " + silver.ToString() + " Silver";
        if (copper > 0)
            moneyString += " " + copper.ToString() + " Copper";
        if (moneyString.Length == 0)
            moneyString = "Nothing";
        return moneyString;
    }

    /// <summary>
    /// Returns the short text for money
    /// </summary>
    public static string MoneyShortText(long money)
    {
        string moneyString = "";
        long gold = MoneyGold(money);
        long silver = MoneySilver(money);
        long copper = MoneyCopper(money);
        if (gold > 0)
            moneyString = gold.ToString() + "g";
        if (silver > 0)
            moneyString += " " + silver.ToString() + "s";
        if (copper > 0)
            moneyString += " " + copper.ToString() + "c";
        if (moneyString.Length == 0)
            moneyString = "--";
        return moneyString;
    }

    /// <summary>
    /// Returns the gold portion money
    /// </summary>
    public static long MoneyGold(long money)
    {
        return (money / 10000);
    }

    /// <summary>
    /// Returns the silver portion money
    /// </summary>
    public static long MoneySilver(long money)
    {
        long gold = (money / 10000);
        money = money - gold * 10000;
        return (money / 100);
    }

    /// <summary>
    /// Returns the copper portion money
    /// </summary>
    public static long MoneyCopper(long money)
    {
        return money % 100;
    }

    /// <summary>
    /// rounds long to 3 valid digits (12345 => 12300; 12354 => 12400)
    /// </summary>
    public static long MoneyRound(long money)
    {
        int digits = money.ToString().Length;
        if (digits <= 3)
        {
            return money;
        }
        else
        {
            long corr = (long)Mathf.Pow(10, digits - 3);
            money = Mathf.RoundToInt((float)money / corr) * corr;
            return money;
        }
    }

    /// <summary>
    /// Add money to players inventory
    /// // if no space for items, drop them
    /// This must be called from Server
    /// </summary>
    public static void AddToInventory(Player player, long money)
    {
        //we cannot add negative money or 0
        if (money <= 0)
            return;

        int gold = (int)MoneyGold(money);
        int silver = (int)MoneySilver(money);
        int copper = (int)MoneyCopper(money);
        int containerId;
        int slotIndex;
        bool hasLostCoins = false;

        if (copper > 0)
        {
            if (player.FindItemInAvailableInventory(itemNameCopper, copper, out containerId, out slotIndex, out bool isEmpty))
            {
                if (isEmpty)
                {
                    ScriptableItem itemData;
                    if (ScriptableItem.dict.TryGetValue(itemNameCopper.GetStableHashCode(), out itemData))
                    {
                        Item item = new Item(itemData);
                        player.inventory.AddItem(item, containerId, slotIndex, copper);
                    }
                }
                else
                {
                    player.inventory.IncreaseAmount(containerId, slotIndex, copper);
                }
            }
            else
            {
                // no place for copper
                ScriptableItem itemData;
                if (ScriptableItem.dict.TryGetValue(itemNameCopper.GetStableHashCode(), out itemData))
                {
                    Item item = new Item(itemData);
                    player.CreateItemOnGround(item, player.transform.position.x, player.transform.position.y, player.transform.position.z, copper);
                    hasLostCoins = true;
                }
            }
        }
        if (silver > 0)
        {
            if (player.FindItemInAvailableInventory(itemNameSilver, silver, out containerId, out slotIndex, out bool isEmpty))
            {
                if (isEmpty)
                {
                    ScriptableItem itemData;
                    if (ScriptableItem.dict.TryGetValue(itemNameSilver.GetStableHashCode(), out itemData))
                    {
                        Item item = new Item(itemData);
                        player.inventory.AddItem(item, containerId, slotIndex, silver);
                    }
                }
                else
                {
                    player.inventory.IncreaseAmount(containerId, slotIndex, silver);
                }
            }
            else
            {
                // no place for silver
                ScriptableItem itemData;
                if (ScriptableItem.dict.TryGetValue(itemNameSilver.GetStableHashCode(), out itemData))
                {
                    Item item = new Item(itemData);
                    player.CreateItemOnGround(item, player.transform.position.x, player.transform.position.y, player.transform.position.z, silver);
                    hasLostCoins = true;
                }
            }
        }
        if (gold > 0)
        {
            if (player.FindItemInAvailableInventory(itemNameGold, gold, out containerId, out slotIndex, out bool isEmpty))
            {
                if (isEmpty)
                {
                    ScriptableItem itemData;
                    if (ScriptableItem.dict.TryGetValue(itemNameGold.GetStableHashCode(), out itemData))
                    {
                        Item item = new Item(itemData);
                        player.inventory.AddItem(item, containerId, slotIndex, gold);
                    }
                }
                else
                {
                    player.inventory.IncreaseAmountLimitless(containerId, slotIndex, gold);
                }
            }
            else
            {
                // no place for copper
                ScriptableItem itemData;
                if (ScriptableItem.dict.TryGetValue(itemNameGold.GetStableHashCode(), out itemData))
                {
                    Item item = new Item(itemData);
                    player.CreateItemOnGround(item, player.transform.position.x, player.transform.position.y, player.transform.position.z, gold);
                    hasLostCoins = true;
                }
            }
        }
        if (hasLostCoins)
            player.Inform("Who said Bricklebrit?  Well, you recognizes that there was no space left in your inventory for the coins.");
    }

    /// <summary>
    /// How many money is in backpack and belt together
    /// </summary>
    public static int AvailableMoney(Player player)
    {
        int containerIndexBackpack = player.containers.IndexOfId(player.ContainerIdOfBackpack());
        ItemSlot itemSlot;
        int foundMoney = 0;
        // search first in the backpack
        if (containerIndexBackpack != -1)
        {
            for (int i = 0; i < player.containers[containerIndexBackpack].slots; i++)
            {
                if (player.inventory.GetItemSlot(player.ContainerIdOfBackpack(), i, out itemSlot))
                {
                    // add value of money
                    if (itemSlot.item.data is MoneyItem)
                    {
                        foundMoney += itemSlot.amount * itemSlot.item.data.price;
                    }
                }
            }
        }
        // and in the belt
        for (int i = GlobalVar.equipmentBelt1; i <= GlobalVar.equipmentBelt6; i++)
        {
            if (player.inventory.GetItemSlot(GlobalVar.containerEquipment, i, out itemSlot))
            {
                // add value of money
                if (itemSlot.item.data is MoneyItem)
                {
                    foundMoney += itemSlot.amount * itemSlot.item.data.price;
                }
            }
        }
        return foundMoney;
    }

    /// <summary>
    /// Remove money from backpack and belt;
    /// false if not enought money available - Server fuction
    /// </summary>
    public static bool TakeFromInventory(Player player, long money)
    {
        //we cannot take negative money or 0
        if (money <= 0)
            return false;
        // all or nothing
        if (money > AvailableMoney(player))
            return false;

        int goldTake = (int)MoneyGold(money);
        int silverTake = (int)MoneySilver(money);
        int copperTake = (int)MoneyCopper(money);
        int goldHave = GlobalFunc.AvailableItem(player, itemNameGold);
        int silverHave = GlobalFunc.AvailableItem(player, itemNameSilver);
        int copperHave = GlobalFunc.AvailableItem(player, itemNameCopper);

        if (copperTake > 0)
        {
            // not enough copper, change a silver and take change
            if (copperHave < copperTake)
            {
                int copperChange = 100 - copperTake;
                AddToInventory(player, copperChange);
                copperHave += copperChange;
                silverTake++;
            }
            else
            {
                GlobalFunc.RemoveItemFromInventory(player, itemNameCopper, copperTake);
                copperHave -= copperTake;
            }
        }
        if (silverTake > 0)
        {
            //can we take copper
            if (copperHave >= 100)
            {
                int copperAsSilver = copperHave / 100;
                int removeCopperAsSilver = Mathf.Min(silverTake, copperAsSilver);
                GlobalFunc.RemoveItemFromInventory(player, itemNameCopper, copperAsSilver * 100);
                silverTake -= removeCopperAsSilver;
                copperHave -= copperAsSilver * 100;
            }
            // still silver to take
            if (silverTake > 0)
            {
                // not enough silver, change a gold and take change
                if (silverHave < silverTake)
                {
                    int silverChange = 100 - silverTake;
                    AddToInventory(player, silverChange * 100);
                    silverHave += silverChange;
                    goldTake++;
                }
                else
                {
                    GlobalFunc.RemoveItemFromInventory(player, itemNameSilver, silverTake);
                    silverHave -= silverTake;
                }

            }
        }
        if (goldTake > 0)
        {
            //can we take copper
            if (copperHave >= 10000)
            {
                int copperAsGold = copperHave / 10000;
                int removeCopperAsGold = Mathf.Min(goldTake, copperAsGold);
                GlobalFunc.RemoveItemFromInventory(player, itemNameCopper, copperAsGold * 10000);
                goldTake -= removeCopperAsGold;
            }
            //still gold to take
            if (goldTake > 0)
            {
                //can we take silver
                if (silverHave >= 100)
                {
                    int silverAsGold = silverHave / 100;
                    int removeSilverAsGold = Mathf.Min(goldTake, silverAsGold);
                    GlobalFunc.RemoveItemFromInventory(player, itemNameSilver, silverAsGold * 100);
                    goldTake -= removeSilverAsGold;
                }
                //still gold to take
                if (goldTake > 0)
                {
                    GlobalFunc.RemoveItemFromInventory(player, itemNameGold, goldTake);
                }
            }
        }
        // no way it was not sucessful
        return true;
    }


    /// <summary>
    /// Strip the player, Remove all money from backpack and belt  - Server fuction
    /// </summary>
    public static void TakeAllFromInventory(Player player)
    {
        TakeFromInventory(player, AvailableMoney(player));
        //we cannot take negative money or 0
    }

    /// <summary>
    /// Change the price according current item state
    /// </summary>
    public static long AdaptToDurabilityAndQuality(long money, int durability, int quality)
    {
        long maxMoney = (long)(money * GlobalVar.priceBestToAverageDurability * GlobalVar.priceBestToAverageQuality);
        float diffD = NonLinearCurves.GetFloat0_1(GlobalVar.priceCurveDurability, durability);
        float diffQ = NonLinearCurves.GetFloat0_1(GlobalVar.priceCurveQuality, quality);
        return MoneyRound((long)(maxMoney * diffD * diffQ));
    }
}
