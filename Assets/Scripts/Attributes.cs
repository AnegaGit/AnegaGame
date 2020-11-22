/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System;
using UnityEngine;
public partial class Attributes
{
    private const int iConstitution = 0;
    private const int iHealthiness = 1;
    private const int iEssence = 2;
    private const int iMagicAwareness = 3;
    private const int iEndurance = 4;
    private const int iFitness = 5;
    private const int iStrength = 6;
    private const int iDexterity = 7;
    private const int iAgility = 8;
    private const int iIntelligence = 9;
    private const int iPerception = 10;
    private const int iLuck = 11;

    private static int noOfAttributes = 12;

    private bool _isDefault = true;
    public bool isDefault { get { return _isDefault; } }

    public int strength
    {
        get { return listOfAttributes[iStrength].value; }
        set { listOfAttributes[iStrength].value = value; _isDefault = false; }
    }
    public int essence
    {
        get { return listOfAttributes[iEssence].value; }
        set { listOfAttributes[iEssence].value = value; _isDefault = false; }
    }
    public int agility
    {
        get { return listOfAttributes[iAgility].value; }
        set { listOfAttributes[iAgility].value = value; _isDefault = false; }
    }
    public int constitution
    {
        get { return listOfAttributes[iConstitution].value; }
        set { listOfAttributes[iConstitution].value = value; _isDefault = false; }
    }
    public int endurance
    {
        get { return listOfAttributes[iEndurance].value; }
        set { listOfAttributes[iEndurance].value = value; _isDefault = false; }
    }
    public int intelligence
    {
        get { return listOfAttributes[iIntelligence].value; }
        set { listOfAttributes[iIntelligence].value = value; _isDefault = false; }
    }
    public int magicAwareness
    {
        get { return listOfAttributes[iMagicAwareness].value; }
        set { listOfAttributes[iMagicAwareness].value = value; _isDefault = false; }
    }
    public int healthiness
    {
        get { return listOfAttributes[iHealthiness].value; }
        set { listOfAttributes[iHealthiness].value = value; _isDefault = false; }
    }
    public int fitness
    {
        get { return listOfAttributes[iFitness].value; }
        set { listOfAttributes[iFitness].value = value; _isDefault = false; }
    }
    public int luck
    {
        get { return listOfAttributes[iLuck].value; }
        set { listOfAttributes[iLuck].value = value; _isDefault = false; }
    }
    public int dexterity
    {
        get { return listOfAttributes[iDexterity].value; }
        set { listOfAttributes[iDexterity].value = value; _isDefault = false; }
    }
    public int perception
    {
        get { return listOfAttributes[iPerception].value; }
        set { listOfAttributes[iPerception].value = value; _isDefault = false; }
    }


    public int count
    {
        get { return noOfAttributes; }
    }
    public int allocatedTotal
    {
        get { return TotalCount(); }
    }

    public struct Attribute
    {
        public int value;
        public string name;
        public string headline;
        public string description;
    }
    public Attribute[] listOfAttributes = new Attribute[noOfAttributes];


    public Attributes()
    {
        listOfAttributes[iStrength].value = 10;
        listOfAttributes[iStrength].name = "Strength";
        listOfAttributes[iStrength].headline = "Where is the piano, I carry the notes?";
        listOfAttributes[iStrength].description = "It defines how much a character can carry." + Environment.NewLine + "With a strength of 0 you may barely carry your empty bag.";
        listOfAttributes[iEssence].value = 10;
        listOfAttributes[iEssence].name = "Essence";
        listOfAttributes[iEssence].headline = "Abra Ka ... fuck @!x#";
        listOfAttributes[iEssence].description = "It defines the maximal mana points a character can reach." + Environment.NewLine
            + "Since magic is involved in everything, a character with a value of 0 will struggle to survive" + Environment.NewLine
            + "Spells require a certain amount of available mana. Some spells may not be possible if the attribute is not high enough.";
        listOfAttributes[iAgility].value = 10;
        listOfAttributes[iAgility].name = "Agility";
        listOfAttributes[iAgility].headline = "Citius, altius, fortius";
        listOfAttributes[iAgility].description = "It defines the moving speed of the character." + Environment.NewLine + "Slow character may not be able to outran monster.";
        listOfAttributes[iConstitution].value = 10;
        listOfAttributes[iConstitution].name = "Constitution";
        listOfAttributes[iConstitution].headline = "Health is our greatest wealth.";
        listOfAttributes[iConstitution].description = "It defines the maximal available health points." + Environment.NewLine + "You may be killed by a light wind or survive the collision with a dragon.";
        listOfAttributes[iEndurance].value = 10;
        listOfAttributes[iEndurance].name = "Endurance";
        listOfAttributes[iEndurance].headline = "He died on short breath.";
        listOfAttributes[iEndurance].description = "It defines how long a character can operate on full power." + Environment.NewLine + "If you are planning to run to Marathon, better use a higher value.";
        listOfAttributes[iIntelligence].value = 10;
        listOfAttributes[iIntelligence].name = "Intelligence";
        listOfAttributes[iIntelligence].headline = "To say one knows it all is truely a lie.";
        listOfAttributes[iIntelligence].description = "It defines the speed a character learns." + Environment.NewLine + "As lower the value as more time you need to skill.";
        listOfAttributes[iMagicAwareness].value = 10;
        listOfAttributes[iMagicAwareness].name = "Magic Awareness";
        listOfAttributes[iMagicAwareness].headline = "I'm still dry.";
        listOfAttributes[iMagicAwareness].description = "It defines the mana regeneration rate." + Environment.NewLine
            + "Be aware mana regeneration depends on the local conditions as well. There are many regions with low mana load and therefore a relatively reduced mana regeneration. Essence does not influence magic awareness.";
        listOfAttributes[iHealthiness].value = 10;
        listOfAttributes[iHealthiness].name = "Healthiness";
        listOfAttributes[iHealthiness].headline = "Hurting is easy, heal hard.";
        listOfAttributes[iHealthiness].description = "It defines the health regeneration rate." + Environment.NewLine
            + "With a low value it may take hours until your character is healed. The constitution does not influence healthiness.";
        listOfAttributes[iFitness].value = 10;
        listOfAttributes[iFitness].name = "Fitness";
        listOfAttributes[iFitness].headline = "Fast away, quickly back.";
        listOfAttributes[iFitness].description = "It defines the endurance regeneration." + Environment.NewLine + "Endurance does not influence fitness.";
        listOfAttributes[iLuck].value = 10;
        listOfAttributes[iLuck].name = "Luck";
        listOfAttributes[iLuck].headline = "Man forges his own destiny.";
        listOfAttributes[iLuck].description = "It is used in random functions." + Environment.NewLine
            + "A lot of events such as finding special loot, creating high quality items or critical strikes are influenced by luck.";
        listOfAttributes[iDexterity].value = 10;
        listOfAttributes[iDexterity].name = "Dexterity";
        listOfAttributes[iDexterity].headline = "Among the blind the one-eyed is king.";
        listOfAttributes[iDexterity].description = "It defines the quality that can be achieved through an activity." + Environment.NewLine
            + "Essential for master crafter but not to forget for fighter and mages since the max damage depends on the attribute as well.";
        listOfAttributes[iPerception].value = 10;
        listOfAttributes[iPerception].name = "Perception";
        listOfAttributes[iPerception].headline = "People only see what they are prepared to see.";
        listOfAttributes[iPerception].description = "It defines the distance in which the character can recognize something." + Environment.NewLine
            + "Finding hidden items may become difficult with a low perception.";
    }


    public bool CreateFromString(string attributeString = "")
    {
        bool isInputSufficient = true;
        if (attributeString.Length < noOfAttributes)
        {
            attributeString += "000000000000000000000000"; isInputSufficient = false;
        }
        for (int i = 0; i < noOfAttributes; i++)
        {
            listOfAttributes[i].value = ConvertFromString(attributeString.Substring(i * 2, 2));
        }
        _isDefault = false;
        return isInputSufficient;
    }

    private int ConvertFromString(string input)
    {
        int output = -1;
        if (!int.TryParse((string)input, out output))
            output = 0;
        return GlobalFunc.KeepInRange(output, 0, 20);
    }

    public string CreateString()
    {
        String attributeString = "";
        for (int i = 0; i < noOfAttributes; i++)
        {
            attributeString += String.Format("{0,2}", listOfAttributes[i].value);
        }
        return attributeString;
    }

    public bool IsCorrectAssigned(int totalCount)
    {
        return (TotalCount() == totalCount);
    }
    public bool IsCorrectAssigned()
    {
        return IsCorrectAssigned(GlobalVar.attributeTotal);
    }
    public void CreateRandom(int totalCount)
    {
        System.Random rnd = new System.Random();
        for (int i = 0; i < noOfAttributes; i++)
        {
            listOfAttributes[i].value = 0;
        }
        do
        {
            int index = rnd.Next(noOfAttributes);
            if (listOfAttributes[index].value < 20)
                listOfAttributes[index].value += 1;
        }
        while (TotalCount() < totalCount);
    }

    private int TotalCount()
    {
        int totalCount = 0;
        for (int i = 0; i < noOfAttributes; i++)
        {
            totalCount += listOfAttributes[i].value;
        }
        return totalCount;
    }

    public void ChangeValue(string name, int change)
    {
        for (int i = 0; i < noOfAttributes; i++)
        {
            if (listOfAttributes[i].name == name)
            {
                listOfAttributes[i].value = Mathf.Clamp(listOfAttributes[i].value + change, 0, GlobalVar.attributeMax);
                break;
            }
        }
    }

    public int CombinedAction(Skills.Skill skill)
    {
        int skillId = Skills.IdFromSkill(skill);
        return CombinedAction(Skills.info[skillId].strengthFactor, Skills.info[skillId].dexterityFactor, Skills.info[skillId].agilityFactor, Skills.info[skillId].perceptionFactor);
    }
    public int CombinedAction(int strengthFactor, int dexterityFactor, int agilityFactor, int perceptionFactor)
    {
        int sumFactors = strengthFactor + dexterityFactor + agilityFactor + perceptionFactor;
        if (sumFactors <= 0)
        {
            return 1;
        }
        int sumAbilities = strengthFactor * strength + dexterityFactor * dexterity + agilityFactor * agility + perceptionFactor * perception;
        return (int)(sumAbilities / sumFactors);
    }
}