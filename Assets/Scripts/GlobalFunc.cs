/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;


/// <summary>
/// Static fuctions
/// </summary>
static class GlobalFunc
{
    static System.Random random = new System.Random();

    /// <summary>
    /// Guarantee min &lt;= value &lt;= max
    /// </summary>
    /// <returns>value, adapted if necessary</returns>
    public static int KeepInRange(int value, int min, int max)
    {
        if (value < min) value = min;
        if (value > max) value = max;
        return value;
    }
    public static float KeepInRange(float value, float min, float max)
    {
        if (value < min) value = min;
        if (value > max) value = max;
        return value;
    }
    public static double KeepInRange(double value, double min, double max)
    {
        if (value < min) value = min;
        if (value > max) value = max;
        return value;
    }

    /// <summary>
    /// Check if min &lt;= value &lt;= max
    /// </summary>
    /// <returns>value, adapted if necessary</returns>
    public static bool IsInRange(int value, int min, int max)
    {
        return ((value >= min) && (value <= max));
    }
    public static bool IsInRange(double value, double min, double max)
    {
        return ((value >= min) && (value <= max));
    }

    /// <summary>
    /// First character in string to upper
    /// </summary>
    /// <example>"a nice function" => "A nice function"</example>
    public static string FirstToUpper(string value)
    {
        return value[0].ToString().ToUpper() + value.Substring(1);
    }

    /// <summary>
    /// Relative position to min in range min .. max
    /// </summary>
    /// <returns>0 .. 1</returns>
    public static double ProportionFromValue(double position, double min, double max)
    {
        if (max > min)
        {
            position = KeepInRange(position, min, max);
        }
        else
        {
            position = KeepInRange(position, max, min);
        }
        return (position - min) / (max - min);
    }

    /// <summary>
    /// Value in min .. max according proportion (0..1)
    /// min can be > max
    /// </summary>
    /// <param name="proportion0_1">0 .. 1</param>
    /// <returns>min .. max</returns>
    public static double ValueFromProportion(double proportion0_1, double min, double max)
    {
        proportion0_1 = KeepInRange(proportion0_1, 0d, 1d);
        return min + (max - min) * proportion0_1;
    }
    public static float ValueFromProportion(double proportion0_1, float min, float max)
    {
        return (float)ValueFromProportion(proportion0_1, (double)min, (double)max);
    }

    /// <summary>
    /// Verify if name is allowed
    /// </summary>
    public static bool IsAllowedDisplayedName(string displayedName)
    {
        // not too long?
        // only contains letters,space and ' and not empty (+)?
        return displayedName.Length <= GlobalVar.displayedNameMaxLength &&
               Regex.IsMatch(displayedName, @"^[a-zA-Z '.]+$");
    }
    public static bool IsAllowedDisplayedNameExtended(string displayedName)
    {
        // longer and numbers allowed additionally
        return displayedName.Length <= GlobalVar.displayedNameMaxLengthExtended &&
               Regex.IsMatch(displayedName, @"^[a-zA-Z0-9 '.]+$");
    }

    /// <summary>
    /// Returns the text for the range
    /// </summary>
    public static string ExamineLimitText(float value, GlobalVar.ExamineLimit[] examineLimits)
    {
        int i;
        for (i = 0; i < examineLimits.Length - 1; i++)
        {
            if (value < examineLimits[i].limit)
                break;
        }
        return examineLimits[i].text;
    }

    /// <summary>
    /// Returns the weight according to player ability
    /// </summary>
    public static string WeightText(long gram, int abilityWeight)
    {
        if (abilityWeight == Abilities.Nav)
        {
            return "";
        }
        else if (abilityWeight == Abilities.Poor)
        {
            return ExamineLimitText((float)gram / GlobalVar.weightTextLimit, GlobalVar.weightText);
        }
        else
        {
            return string.Format("It weights about {0} {1}", RoundToSignificantDigits((gram > 1000 ? gram / 1000f : gram),abilityWeight - 1), (gram > 1000 ? "kg" : "g"));
        }
    }

    /// <summary>
    /// Round to significat digits
    /// </summary>
    public static string RoundToSignificantDigits(double d, int digits)
    {
        if (d == 0.0)
        {
            return "0";
        }
        else
        {
            double leftSideNumbers = Math.Floor(Math.Log10(Math.Abs(d))) + 1;
            double scale = Math.Pow(10, leftSideNumbers);
            double result = scale * Math.Round(d / scale, digits, MidpointRounding.AwayFromZero);

            // Clean possible precision error.
            if ((int)leftSideNumbers >= digits)
            {
                return Math.Round(result, 0, MidpointRounding.AwayFromZero).ToString();
            }
            else
            {
                return Math.Round(result, digits - (int)leftSideNumbers, MidpointRounding.AwayFromZero).ToString();
            }
        }
    }

    /// <summary>
    /// Create a new item
    /// </summary>
    public static bool CreateNewItem(string itemName, int summonedHealth, int summonedLevel, int durability, int quality, string miscellaneous, out Item item)
    {
        ScriptableItem itemData;
        if (ScriptableItem.dict.TryGetValue(itemName.GetStableHashCode(), out itemData))
        {
            item = new Item(itemData);
            item.data1 = summonedHealth;
            item.data2 = summonedLevel;
            item.durability = durability;
            item.quality = quality;
            item.miscellaneousSync = miscellaneous;
            return true;
        }
        else
        {
            item = new Item();
            LogFile.WriteLog(LogFile.LogLevel.Error, "Create new item failed: '" + itemName + "' Item doesn't exist.");
            return false;
        }
    }

    /// <summary>
    /// Is this slot an equipment slot (nether hand nor belt)
    /// </summary>
    public static bool IsEquipment(int slot)
    {
        return IsEquipment(GlobalVar.containerEquipment, slot);
    }
    public static bool IsEquipment(int container, int slot)
    {
        return (!IsInBelt(container, slot) && !IsInHand(container, slot));
    }

    /// <summary>
    /// Is this slot a belt slot
    /// </summary>
    public static bool IsInBelt(int slot)
    {
        return IsInBelt(GlobalVar.containerEquipment, slot);
    }
    public static bool IsInBelt(int container, int slot)
    {
        return (container == GlobalVar.containerEquipment && slot >= GlobalVar.equipmentBelt1 && slot <= GlobalVar.equipmentBelt6);
    }

    /// <summary>
    /// Is this slot a hand
    /// </summary>
    public static bool IsInHand(int slot)
    {
        return IsInHand(GlobalVar.containerEquipment, slot);
    }
    public static bool IsInHand(int container, int slot)
    {
        return (container == GlobalVar.containerEquipment && (slot == GlobalVar.equipmentRightHand || slot == GlobalVar.equipmentLeftHand));
    }

    /// <summary>
    /// Get index of other hand
    /// </summary>
    public static int OtherHand(int firstHand)
    {
        if (firstHand == GlobalVar.equipmentRightHand)
            return GlobalVar.equipmentLeftHand;
        return GlobalVar.equipmentRightHand;
    }

    /// <summary>
    /// Has the player a wanditem in any hand, returns -1 or id of hand
    /// </summary>
    public static int hasWandInHand(Player player)
    {
        if (hasWandInHand(player, GlobalVar.equipmentRightHand) > -1)
        {
            return GlobalVar.equipmentRightHand;
        }
        else
        {
            if (hasWandInHand(player, GlobalVar.equipmentLeftHand) > -1)
            {
                return GlobalVar.equipmentLeftHand;
            }
            else
            {
                return -1;
            }
        }
    }
    public static int hasWandInHand(Player player, int hand)
    {
        if (player.inventory.GetItemSlot(GlobalVar.containerEquipment, hand, out ItemSlot itemSlot))
        {
            if (itemSlot.item.data is WeaponItem) //>>> must be WandItem, replace once it exists
            {
                return hand;
            }
        }
        return -1;
    }

    /// <summary>
    /// Count all items available, belt and backpack
    /// </summary>
    public static int AvailableItem(Player player, string itemName)
    {
        int containerIndexBackpack = player.containers.IndexOfId(player.ContainerIdOfBackpack());
        ItemSlot itemSlot;
        int foundItems = 0;
        // search first in the backpack
        if (containerIndexBackpack != -1)
        {
            for (int i = 0; i < player.containers[containerIndexBackpack].slots; i++)
            {
                if (player.inventory.GetItemSlot(player.ContainerIdOfBackpack(), i, out itemSlot))
                {
                    // count
                    if (itemSlot.item.name == itemName)
                    {
                        foundItems += itemSlot.amount;
                    }
                }
            }
        }
        // and in the belt
        for (int i = GlobalVar.equipmentBelt1; i <= GlobalVar.equipmentBelt6; i++)
        {
            if (player.inventory.GetItemSlot(GlobalVar.containerEquipment, i, out itemSlot))
            {
                // count
                if (itemSlot.item.name == itemName)
                {
                    foundItems += itemSlot.amount;
                }
            }
        }
        return foundItems;
    }

    /// <summary>
    /// Remove amount items from backpack and equipment
    /// </summary>
    public static int RemoveItemFromInventory(Player player, string itemName, int amount)
    {
        int containerIndexBackpack = player.containers.IndexOfId(player.ContainerIdOfBackpack());
        ItemSlot itemSlot;
        // search first in the backpack, reverse
        if (containerIndexBackpack != -1)
        {
            for (int i = player.containers[containerIndexBackpack].slots - 1; i >= 0; i--)
            {
                if (player.inventory.GetItemSlot(player.ContainerIdOfBackpack(), i, out itemSlot))
                {
                    // remove as much as needed and possible
                    if (itemSlot.item.name == itemName)
                    {
                        amount -= player.inventory.DecreaseAmount(player.ContainerIdOfBackpack(), i, amount);
                        if (amount <= 0)
                            return 0;
                    }
                }
            }
        }
        // and in the belt
        for (int i = GlobalVar.equipmentBelt6; i >= GlobalVar.equipmentBelt1; i--)
        {
            if (player.inventory.GetItemSlot(GlobalVar.containerEquipment, i, out itemSlot))
            {
                // remove as much as needed and possible
                if (itemSlot.item.name == itemName)
                {
                    amount -= player.inventory.DecreaseAmount(GlobalVar.containerEquipment, i, amount);
                    if (amount <= 0)
                        return 0;
                }
            }
        }
        // and the equipped items at last
        for (int i = 0; i < GlobalVar.equipmentBelt1; i++)
        {
            if (player.inventory.GetItemSlot(GlobalVar.containerEquipment, i, out itemSlot))
            {
                // remove as much as needed and possible
                if (itemSlot.item.name == itemName)
                {
                    amount -= player.inventory.DecreaseAmount(GlobalVar.containerEquipment, i, amount);
                    if (amount <= 0)
                        return 0;
                }
            }
        }
        return amount;
    }

    /// <summary>
    /// degrade the item by 1 per random
    /// </summary>
    public static void DegradeItem(Player player, int containerId, int slotIndex, float useTime)
    {
        if (player.inventory.GetItemSlot(containerId, slotIndex, out ItemSlot itemSlot))
        {
            // test if we degrade one step
            if (RandomLowerLimit0_1(useTime / itemSlot.item.data.maxDurability * 100.0))
            {
                itemSlot.item.durability -= 1;
                if (itemSlot.item.durability <= 0)
                {
                    // remove item
                    player.inventory.Remove(containerId, slotIndex);
                    player.Inform(string.Format("The item {0} breaks. You have no need for the garbage.", itemSlot.item.data.displayName));
                }
                else
                {
                    if (itemSlot.item.durability < PlayerPreferences.warningDurabilityLow)
                    {
                        player.Inform(string.Format("The item {0} will sooner or later break. Maybe you should fix it?", itemSlot.item.data.displayName));
                    }
                    player.inventory.AddOrReplace(itemSlot);
                }
            }
        }
    }

    /// <summary>
    /// create shuffled list of ints using inside out version of the Fisher-Yates shuffle
    /// </summary>
    public static int[] Shuffle(int n)
    {
        var result = new int[n];
        for (var i = 0; i < n; i++)
        {
            var j = random.Next(0, i + 1);
            if (i != j)
            {
                result[i] = result[j];
            }
            result[j] = i;
        }
        return result;
    }

    /// <summary>
    /// Is a random value 0..1 lower the limit?
    /// </summary>
    public static bool RandomLowerLimit0_1(double limit)
    {
        return random.NextDouble() < limit;
    }

    /// <summary>
    /// Random integer min ... max (incluing min and max)
    /// </summary>
    public static int RandomInRange(int min, int max)
    {
        if (min > max + 1)
        {
            int tmp = max;
            max = min;
            min = tmp;
        }
        return random.Next(min, max + 1);
    }
    /// <summary>
    /// Random float min ... max (incluing min and max)
    /// </summary>
    public static float RandomInRange(double min, double max)
    {
        if (min > max)
        {
            double tmp = max;
            max = min;
            min = tmp;
        }
        return (float)(random.NextDouble() * (max - min) + min);
    }

    /// <summary>
    /// Random value 1 +- 0.5*range
    /// </summary>
    public static float RandomObfuscation(double range)
    {
        return (float)(1 + ((random.NextDouble() - 0.5) * UnityEngine.Mathf.Clamp01((float)range)));
    }

    /// <summary>
    /// baseValue +- random range
    /// </summary>
    public static float RandomObfuscation(double baseValue, double range)
    {
        return (float)(baseValue + ((random.NextDouble() * 2) - 1) * range);
    }

    /// <summary>
    /// Random luck facktor according attribute
    /// <para>0 .. maxLuck</para>
    /// </summary>
    public static float LuckFactor(Player player, float luckPortion, float maxLuck = GlobalVar.luckHighMax)
    {
        return LuckFactor(player.attributes.luck, luckPortion, maxLuck);
    }
    /// <summary>
    /// Random luck facktor according attribute
    /// <para>0 .. maxLuck</para>
    /// </summary>
    public static float LuckFactor(int attributeLuck, float luckPortion, float maxLuck = GlobalVar.luckHighMax)
    {
        luckPortion = UnityEngine.Mathf.Clamp01(luckPortion);
        float partion = (float)random.NextDouble();
        float lowLuckLimit = (21 - attributeLuck) * luckPortion / 21f;
        float highLuckLimit = 1 - luckPortion + lowLuckLimit;
        //low luck
        if (partion < lowLuckLimit)
        {
            return (float)NonLinearCurves.GetInterimDouble0_1(GlobalVar.luckLowNonlinear, partion / lowLuckLimit);
        }
        // high luck
        else if (partion > highLuckLimit)
        {
            return 1.0f + NonLinearCurves.FloatFromCurvePosition(GlobalVar.luckHighNonlinear, partion, highLuckLimit, 1.0f, 1.0f, maxLuck - 1.0f);
        }
        return 1.0f;
    }

    /// <summary>
    /// Creates the same sandom value allover the system
    /// </summary>
    public static int UnifiedRandom(int maxValueExclusive, int itemSpecificValue = 0, int accuracyInSeconds = GlobalVar.unifiedRandomAccuracy)
    {
        TimeSpan span = DateTime.Now.Subtract(new DateTime(2010, 1, 1, 0, 0, 0));
        int seed = (int)(span.TotalSeconds / accuracyInSeconds) + itemSpecificValue;
        System.Random rnd = new System.Random(seed);
        return rnd.Next(0, maxValueExclusive);
    }

    /// <summary>
    /// set the old value = newValue and return true, if equal return changed||false
    /// </summary>
    public static string EqualAndConvert(ref bool changed, string oldValue, string newValue)
    {
        if (newValue == oldValue)
        {
            // changed is unchanged
            return oldValue;
        }
        changed = true;
        return newValue;
    }
    /// <summary>
    /// set the old value = newValue and return true, if equal return changed||false
    /// </summary>
    public static int EqualAndConvert(ref bool changed, int oldValue, string newValue)
    {
        if (int.TryParse(newValue, out int newInt))
        {
            if (newInt == oldValue)
            {
                // changed is unchanged
                return oldValue;
            }
            changed = true;
            return newInt;
        }
        else
        {
            // changed is unchanged
            return oldValue;
        }
    }
    /// <summary>
    /// set the old value = newValue and return true, if equal return changed||false
    /// </summary>
    public static float EqualAndConvert(ref bool changed, float oldValue, string newValue)
    {
        // make sure no coma
        if (float.TryParse(newValue, out float newFloat))
        {
            if (newFloat == oldValue)
            {
                // changed is unchanged
                return oldValue;
            }
            changed = true;
            return newFloat;
        }
        else
        {
            // changed is unchanged
            return oldValue;
        }
    }
    /// <summary>
    /// set the old value = newValue and return true, if equal return changed||false
    /// </summary>
    public static bool EqualAndConvert(ref bool changed, bool oldValue, string newValue)
    {
        if (bool.TryParse(newValue, out bool newBool))
        {
            if (newBool == oldValue)
            {
                // changed is unchanged
                return oldValue;
            }
            changed = true;
            return newBool;
        }
        else
        {
            // changed is unchanged
            return oldValue;
        }
    }
    /// <summary>
    /// set the old value = newValue and return true, if equal return changed||false
    /// </summary>
    public static Skills.Skill EqualAndConvert(ref bool changed, Skills.Skill oldValue, string newValue)
    {
        Skills.Skill newSkill = Skills.SkillFromName(newValue);
        if (Skills.Skill.NoSkill == newSkill)
        {
            // changed is unchanged
            return oldValue;
        }
        else
        {
            if (newSkill == oldValue)
            {
                // changed is unchanged
                return oldValue;
            }
            changed = true;
            return newSkill;
        }
    }

    /// <summary>
    /// Converts a color into a 6 Byte HEX string rgb
    /// </summary>
    public static string ColorToHex(Color32 color)
    {
        string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        return hex;
    }

    /// <summary>
    /// Coverts a 6 or 8 Byte HEX string to color
    /// </summary>
    public static Color HexToColor(string hex)
    {
        hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
        hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
        byte a = 255;//assume fully visible unless specified in hex
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        //Only use alpha if the string has enough characters
        if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }
        return new Color32(r, g, b, a);
    }

    /// <summary>
    /// Clear Debug log console
    /// </summary>
    /// <remarks>https://forum.unity.com/threads/solved-unity-2017-1-0f3-trouble-cleaning-console-via-code.484079/</remarks>
    public static void ClearLogConsole()
    {
        var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");

        var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

        clearMethod.Invoke(null, null);
    }

    /// <summary>
    /// Find first bit position == 1
    /// </summary>
    public static int FirstBitPosition(int value)
    {
        if (value < 1) return 0;
        int n = 0;
        while ((value & 1) != 1)
        {
            value >>= 1;
            ++n;
        }
        return n;
    }

    /// <summary>
    /// Is bit in mask 0(false) or 1(true)
    /// </summary>
    public static bool HasBitSet(int mask, int bit)
    {
        if (mask < 1 || bit > 31) return false;
        return (mask & (1 << bit)) != 0;
    }

    /// <summary>
    /// Set bit in mask to default 1
    /// </summary>
    public static int SetBit(int mask, int bit, bool target = true)
    {
        if (target)
            return mask |= (1 << bit);
        else
            return RemoveBit(mask, bit);
    }

    /// <summary>
    /// Set bit in mask to 0
    /// </summary>
    public static int RemoveBit(int mask, int bit)
    {
        return mask & ~(1 << bit);
    }

    /// <summary>
    /// Returns mask for the specific bit
    /// </summary>
    public static int MaskForBit(int bit)
    {
        if (bit < 0) return 0;
        return 1 << bit;
        //return SetBit(0, bit);
    }
}

public static class IListExtensions
{
    static System.Random random = new System.Random();

    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    public static void Shuffle<T>(this IList<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = random.Next(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
}

