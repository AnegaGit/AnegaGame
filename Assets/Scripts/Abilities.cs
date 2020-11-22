/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System;
using UnityEngine;
public partial class Abilities
{
    private const int iCompass = 0;
    private const int iMentalMap = 1;
    private const int iInnerClock = 2;
    private const int iDarkVision = 3;
    private const int iReadAndWrite = 4;
    private const int iVoiceControl = 5;
    private const int iDiagnosis = 6;
    private const int iStorekeeper = 7;
    private const int iHandscale = 8;
    private const int iBestprice = 9;
    private const int iWaterproof = 10;
    private const int iRoadrunner = 11;
    private static int noOfAbilities = 12;

    public int compass
    {
        get { return listOfAbilities[iCompass].value; }
        set { listOfAbilities[iCompass].value = value; }
    }
    public int mentalMap
    {
        get { return listOfAbilities[iMentalMap].value; }
        set { listOfAbilities[iMentalMap].value = value; }
    }
    public int innerClock
    {
        get { return listOfAbilities[iInnerClock].value; }
        set { listOfAbilities[iInnerClock].value = value; }
    }
    public int darkVision
    {
        get { return listOfAbilities[iDarkVision].value; }
        set { listOfAbilities[iDarkVision].value = value; }
    }
    public int readAndWrite
    {
        get { return Excellent; }// listOfAbilities[iReadAndWrite].value; }
        set { listOfAbilities[iReadAndWrite].value = value; }
    }
    public int voiceControl
    {
        get { return listOfAbilities[iVoiceControl].value; }
        set { listOfAbilities[iVoiceControl].value = value; }
    }
    public int diagnosis
    {
        get { return listOfAbilities[iDiagnosis].value; }
        set { listOfAbilities[iDiagnosis].value = value; }
    }
    public int storekeeper
    {
        get { return listOfAbilities[iStorekeeper].value; }
        set { listOfAbilities[iStorekeeper].value = value; }
    }
    public int handscale
    {
        get { return listOfAbilities[iHandscale].value; }
        set { listOfAbilities[iHandscale].value = value; }
    }
    public int bestprice
    {
        get { return listOfAbilities[iBestprice].value; }
        set { listOfAbilities[iBestprice].value = value; }
    }
    public int waterproof
    {
        get { return listOfAbilities[iWaterproof].value; }
        set { listOfAbilities[iWaterproof].value = value; }
    }
    public int roadrunner
    {
        get { return listOfAbilities[iRoadrunner].value; }
        set { listOfAbilities[iRoadrunner].value = value; }
    }
    public int count
    {
        get { return noOfAbilities; }
    }
    public int allocatedTotal
    {
        get { return TotalCount(); }
    }

    public struct Ability
    {
        public int value;
        public string name;
        public string headline;
        public string descriptionNav;
        public string descriptionPoor;
        public string descriptionGood;
        public string descriptionExcellent;
    }
    public Ability[] listOfAbilities = new Ability[noOfAbilities];


    public const int Nav = 0;
    public const int Poor = 1;
    public const int Good = 2;
    public const int Excellent = 3;

    public Abilities()
    {
        listOfAbilities[iCompass].value = 0;
        listOfAbilities[iCompass].name = "Compass";
        listOfAbilities[iCompass].headline = "Shall we turn to left or right?";
        listOfAbilities[iCompass].descriptionNav = "<i>The town is 5 minutes away and I'm lost!</i>" + Environment.NewLine + "You have no idea where north might be.";
        listOfAbilities[iCompass].descriptionPoor = "<i>That's where we comming from … but...</i>" + Environment.NewLine + "Your compass knows 4 directions only.";
        listOfAbilities[iCompass].descriptionGood = "<i>That way looks better.</i>" + Environment.NewLine + "Your compass knows already 8 directions.";
        listOfAbilities[iCompass].descriptionExcellent = "<i>Follow me, I know the way!</i>" + Environment.NewLine + "Your compass shows exact north.";
        listOfAbilities[iMentalMap].value = 0;
        listOfAbilities[iMentalMap].name = "Mental Map";
        listOfAbilities[iMentalMap].headline = "Lost in a maze?";
        listOfAbilities[iMentalMap].descriptionNav = "<i>What you see is what you get.</i>" + Environment.NewLine + "You don't have a feeling for the area around yourself.";
        listOfAbilities[iMentalMap].descriptionPoor = "<i>Do not stalk from behind!</i>" + Environment.NewLine + "You feeling for the area around yourself is limited to the area in front of your eyes.";
        listOfAbilities[iMentalMap].descriptionGood = "<i>All aroud clear.</i>" + Environment.NewLine + "Your feeling for the area around yourself has a limited range.";
        listOfAbilities[iMentalMap].descriptionExcellent = "<i>Bird eyes view!</i>" + Environment.NewLine + "Your feeling for the area around yourself can be extended to a larger area.";
        listOfAbilities[iInnerClock].value = 0;
        listOfAbilities[iInnerClock].name = "Inner Clock";
        listOfAbilities[iInnerClock].headline = "What time is it?";
        listOfAbilities[iInnerClock].descriptionNav = "<i>Summer nights are shorter - you are kidding me!</i>" + Environment.NewLine + "You know whether it is morning, afternoon or night.At least if you can see the sky. In a mine or dungeon it's always night.";
        listOfAbilities[iInnerClock].descriptionPoor = "<i>10 or 11 o'clock - what does it matter?</i>" + Environment.NewLine + "With an accuracy of about 3 hours you can define the current time.At least if you can see the sky. In a mine or dungeon it's always night.";
        listOfAbilities[iInnerClock].descriptionGood = "<i>It's 7:00, sunrise will be soon.</i>" + Environment.NewLine + "With an accuracy of about half an hour you know the time. At least if you can see the sky. In a mine or dungeon the accuracy drops to 3 hours.";
        listOfAbilities[iInnerClock].descriptionExcellent = "<i>5 to 12, for sure!</i>" + Environment.NewLine + "Wherever you are, you know the time.";
        listOfAbilities[iDarkVision].value = 0;
        listOfAbilities[iDarkVision].name = "Dark Vision";
        listOfAbilities[iDarkVision].headline = "Everything went black.";
        listOfAbilities[iDarkVision].descriptionNav = "<i>Is there a hand in front of your face?</i>" + Environment.NewLine + "Without a strong light source you are lost in the night. Everything might become black at night.";
        listOfAbilities[iDarkVision].descriptionPoor = "<i>The stars are sometimes enough.</i>" + Environment.NewLine + "A little light light the stars show you some of your environment. But in a dungeon everything might become black.";
        listOfAbilities[iDarkVision].descriptionGood = "<i>Cat Eye</i>" + Environment.NewLine + "As long as there is any light source you can can clearly recognize the environment. But in a dungeon everything might become black.";
        listOfAbilities[iDarkVision].descriptionExcellent = "<i>Torches are overrated.</i>" + Environment.NewLine + "You don't need any light source, even in the most dark dungeon.";
        listOfAbilities[iReadAndWrite].value = 0;
        listOfAbilities[iReadAndWrite].name = "Read and Write";
        listOfAbilities[iReadAndWrite].headline = "Your crosses here, please!";
        listOfAbilities[iReadAndWrite].descriptionNav = "<i>Learning by heart is the nuts and bolts. I'm really good at it.</i>" + Environment.NewLine + "Letters are a mystery. You are illiterate and can not even read signposts.";
        listOfAbilities[iReadAndWrite].descriptionPoor = "<i>E... En... Entrance</i>" + Environment.NewLine + "You can read labels and signposts but neither parchments nor books. You cannot write.";
        listOfAbilities[iReadAndWrite].descriptionGood = "<i>I don't like blue fingers.</i>" + Environment.NewLine + "You can read everything and write labels and signposts.";
        listOfAbilities[iReadAndWrite].descriptionExcellent = "<i>Those with education</i>" + Environment.NewLine + "You can read and write everything.";
        listOfAbilities[iVoiceControl].value = 0;
        listOfAbilities[iVoiceControl].name = "Voice control";
        listOfAbilities[iVoiceControl].headline = "Say that again, please!";
        listOfAbilities[iVoiceControl].descriptionNav = "<i>Coughs …</i>" + Environment.NewLine + "You are barely understandable. Most will think you are dumb.";
        listOfAbilities[iVoiceControl].descriptionPoor = "<i>Louder!</i>" + Environment.NewLine + "You shouldn't try to speak to more people. And you better run instead call for help.";
        listOfAbilities[iVoiceControl].descriptionGood = "<i>I understand.</i>" + Environment.NewLine + "You don't have any issues with communication.";
        listOfAbilities[iVoiceControl].descriptionExcellent = "<i>Be quiet!</i>" + Environment.NewLine + "Your voice carries far.";
        listOfAbilities[iDiagnosis].value = 0;
        listOfAbilities[iDiagnosis].name = "Health diagnosis";
        listOfAbilities[iDiagnosis].headline = "The next please.";
        listOfAbilities[iDiagnosis].descriptionNav = "<i>He is still alive!</i>" + Environment.NewLine + "You can just tell if someone else is alive.";
        listOfAbilities[iDiagnosis].descriptionPoor = "<i>So much blood, where does it all come from?</i>" + Environment.NewLine + "You can not recognize more than the obvious health state.These states are: unharmed, slightly wounded, wounded, badly wounded, near death";
        listOfAbilities[iDiagnosis].descriptionGood = "<i>Be careful when they take the knife!</i>" + Environment.NewLine + "You can estimate the health status of each person examined relatively well.";
        listOfAbilities[iDiagnosis].descriptionExcellent = "<i>Doctors are great as long as you don't need them.</i>" + Environment.NewLine + "You can see the state of health of each person being examined accurately.";
        listOfAbilities[iStorekeeper].value = 0;
        listOfAbilities[iStorekeeper].name = "Storekeeper";
        listOfAbilities[iStorekeeper].headline = "Dragons are known for theire propensity to hoard.";
        listOfAbilities[iStorekeeper].descriptionNav = "<i>I do not need much.</i>" + Environment.NewLine + "The number of items your character can have is very limited. You will almost not be able to increase the size of the initial storage space." + Environment.NewLine + "All different depots and bags are counted. A stack of items counts as one.";
        listOfAbilities[iStorekeeper].descriptionPoor = "<i>Order is half the life, searching remains.</i>" + Environment.NewLine + "Your character has a decent storage capacity. It should be sufficient for a normal live. You better throw away not needed items from time to time." + Environment.NewLine + "All different depots and bags are counted. A stack of items counts as one.";
        listOfAbilities[iStorekeeper].descriptionGood = "<i>I'm not a compulsive hoarder!</i>" + Environment.NewLine + "It will take a long time before the character reaches its storage capacity." + Environment.NewLine + "All different depots and bags are counted. A stack of items counts as one.";
        listOfAbilities[iStorekeeper].descriptionExcellent = "<i>It must be anywhere!</i>" + Environment.NewLine + "Your character can become a well - known merchant. The number of storage locations in its depots is huge." + Environment.NewLine + "All different depots and bags are counted. A stack of items counts as one.";
        listOfAbilities[iHandscale].value = 0;
        listOfAbilities[iHandscale].name = "Hand scale";
        listOfAbilities[iHandscale].headline = "Sometimes you don't realize the weight of a burden you've been carrying until you feel the weight of its release.";
        listOfAbilities[iHandscale].descriptionNav = "<i>If it is big, it must be heavy.</i>" + Environment.NewLine + "Somehow you have no feeling for weights at all. Larger items probably weigh more." + Environment.NewLine + "You will be surprised again and again if you can not lift your bag anymore.";
        listOfAbilities[iHandscale].descriptionPoor = "<i>Simplicity is an exact between too heavy and too light.</i>" + Environment.NewLine + "You have an idea of what weighs things." + Environment.NewLine + "You can also estimate the weight of your bag, though not very exactly.";
        listOfAbilities[iHandscale].descriptionGood = "<i>This is exact one kilo, more or less.</i>" + Environment.NewLine + "You know the weight of objects. But not more accurate than a single number." + Environment.NewLine + "The weight of your bag you can estimate very accurately.";
        listOfAbilities[iHandscale].descriptionExcellent = "<i>This stone bag weighs two grams too little!</i>" + Environment.NewLine + "It seems like you have a natural scale. You can determine the weight of objects and your bag exactly.";
        listOfAbilities[iBestprice].value = 0;
        listOfAbilities[iBestprice].name = "Bestprice";
        listOfAbilities[iBestprice].headline = "Now is the best time to be rich, and the best time to be poor.";
        listOfAbilities[iBestprice].descriptionNav = "<i>The few copper do nothing.</i>" + Environment.NewLine + "Stationary merchants interest you little. These will very rarely sell you things at the lowest price or buy things at the highest price.";
        listOfAbilities[iBestprice].descriptionPoor = "<i>Seize the opportunity if it offers itself.</i>" + Environment.NewLine + "From time to time stationary merchants offer you the best price. It is up to you to recognize and use the opportunity.";
        listOfAbilities[iBestprice].descriptionGood = "<i>Do not buy tomorrow what you can buy today.</i>" + Environment.NewLine + "You are rarely forced to buy or sell immediately. If the price is better tomorrow, you can wait. Stationary merchants often offer you the best price.";
        listOfAbilities[iBestprice].descriptionExcellent = "<i>Time is money.</i>" + Environment.NewLine + "Traceable prices are particularly important to you as a professional merchant. Whether you are buying or selling, stationary merchants will almost always offer you the best possible price.";
        listOfAbilities[iBestprice].value = 0;
        listOfAbilities[iWaterproof].name = "Waterproof";
        listOfAbilities[iWaterproof].headline = "There are more people who died because they drank too much water than too much poison.";
        listOfAbilities[iWaterproof].descriptionNav = "<i>I never touch water</i>" + Environment.NewLine + "You fear water so much that you will never put a foot in the smallest puddle. To get across a small creek, you need a bridge.";
        listOfAbilities[iWaterproof].descriptionPoor = "<i>Safety first!</i>" + Environment.NewLine + "You slow down drastically when your boots touch water. There could be some danger in the water.";
        listOfAbilities[iWaterproof].descriptionGood = "<i>I could if I only wanted to.</i>" + Environment.NewLine + "You slow down a bit in the water. But you have to be careful because of smooth stones or holes." + Environment.NewLine+ "You can just swim in deep water, albeit very slowly.";
        listOfAbilities[iWaterproof].descriptionExcellent = "<i>Was there just a creek?</i>" + Environment.NewLine + "Water practically does not slow you down." + Environment.NewLine + "You can swim in deep water and reach islands in the ocean.";
        listOfAbilities[iWaterproof].value = 0;
        listOfAbilities[iRoadrunner].name = "Roadrunner";
        listOfAbilities[iRoadrunner].headline = "Every road leads to More";
        listOfAbilities[iRoadrunner].descriptionNav = "<i>This is not natural!</i>" + Environment.NewLine + "Artificial ways are suspect to you. You even choose them preferentially, but you don't walk there so quickly.";
        listOfAbilities[iRoadrunner].descriptionPoor = "<i>Where's the difference?</i>" + Environment.NewLine + "You get along on roads and paths just as quickly as in the forest or on a meadow.";
        listOfAbilities[iRoadrunner].descriptionGood = "<i>Come on!</i>" + Environment.NewLine + "You will walk a little faster on roads and paths than anywhere else.";
        listOfAbilities[iRoadrunner].descriptionExcellent = "<i>Don't be so slow.</i>" + Environment.NewLine + "If there is a road or a path, you will reach your destination much faster.";
        listOfAbilities[iRoadrunner].value = 0;
        //listOfAbilities[ixxx].name = "";
        //listOfAbilities[ixxx].headline = "";
        //listOfAbilities[ixxx].descriptionNav = "<i></i>" +Environment.NewLine + "";
        //listOfAbilities[ixxx].descriptionPoor = "<i></i>" +Environment.NewLine + "";
        //listOfAbilities[ixxx].descriptionGood = "<i></i>" +Environment.NewLine + "";
        //listOfAbilities[ixxx].descriptionExcellent = "<i></i>" +Environment.NewLine + "";
        //listOfAbilities[ixxx].value = 0;
    }


    public bool CreateFromString(string abilityString = "")
    {
        bool isInputSufficient = true;
        if (abilityString.Length < noOfAbilities)
        {
            abilityString += "0000000000000"; isInputSufficient = false;
        }
        for (int i = 0; i < noOfAbilities; i++)
        {
            listOfAbilities[i].value = ConvertFromString(abilityString.Substring(i, 1));
        }
        return isInputSufficient;
    }

    private int ConvertFromString(string input)
    {
        int output = -1;
        if (!int.TryParse((string)input, out output))
            output = 0;
        return GlobalFunc.KeepInRange(output, 0, 3);
    }

    public string CreateString()
    {
        String abilityString = "";
        for (int i = 0; i < noOfAbilities; i++)
        {
            abilityString += String.Format("{0,1}", listOfAbilities[i].value);
        }
        return abilityString;
    }

    public bool IsCorrectAssigned()
    {
        int totalCount = 0;
        int minCount = 0;
        int maxCount = 0;
        for (int i = 0; i < noOfAbilities; i++)
        {
            totalCount += listOfAbilities[i].value;
            if (listOfAbilities[i].value == Nav) minCount++;
            if (listOfAbilities[i].value == Excellent) maxCount++;
        }

        if (totalCount != GlobalVar.abilityTotal
            ||
            minCount < GlobalVar.abilityMinMin
            ||
            maxCount < GlobalVar.abilityMinMax)
            return false;
        return true;
    }

    public void CreateRandom()
    {
        System.Random rnd = new System.Random();
        for (int i = 0; i < noOfAbilities; i++)
        {
            listOfAbilities[i].value = 1;
        }
        do
        {
            listOfAbilities[rnd.Next(noOfAbilities)].value = 0;
        }
        while (TotalCount() > noOfAbilities - GlobalVar.abilityMinMin);
        do
        {
            int index = rnd.Next(noOfAbilities);
            if (listOfAbilities[index].value > 0)
                listOfAbilities[index].value = 4;
        }
        while (TotalCount() < noOfAbilities - GlobalVar.abilityMinMin + 3 * GlobalVar.abilityMinMax);
        do
        {
            int index = rnd.Next(noOfAbilities);
            if (listOfAbilities[index].value != 0 && listOfAbilities[index].value < 4)
                listOfAbilities[index].value += 1;
        }
        while (TotalCount() < GlobalVar.abilityTotal - GlobalVar.abilityMinMin + noOfAbilities);
        for (int i = 0; i < noOfAbilities; i++)
        {
            if (listOfAbilities[i].value > 0)
                listOfAbilities[i].value -= 1;
        }
    }

    private int TotalCount()
    {
        int totalCount = 0;
        for (int i = 0; i < noOfAbilities; i++)
        {
            totalCount += listOfAbilities[i].value;
        }
        return totalCount;
    }

    public void ChangeValue(string abilityName, bool increase)
    {
        for (int i = 0; i < count; i++)
        {
            if (abilityName == listOfAbilities[i].name)
            {
                listOfAbilities[i].value = Mathf.Clamp(listOfAbilities[i].value + (increase ? 1 : -1), Abilities.Nav, Abilities.Excellent);
                break;
            }
        }
    }
}