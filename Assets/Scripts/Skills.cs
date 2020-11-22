/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Mirror;

public class Skills
{
    public partial struct SkillInfo
    {
        public Skill skill;
        public int id;
        public string name;
        public string headline;
        public string description;
        public string defaultTool;
        public int learnCurve;
        public int totalSkillTime;
        public float learnTime1;
        public int group;
        public int strengthFactor;
        public int dexterityFactor;
        public int agilityFactor;
        public int perceptionFactor;

        //constructor
        public SkillInfo(Skill skill, string name, string headline, string description, string defaultTool, int learnCurve, int totalSkillTime, int group, int strengthFactor, int dexterityFactor, int agilityFactor, int perceptionFactor)
        {
            this.skill = skill;
            this.id = (int)skill;
            this.name = name;
            this.headline = headline;
            this.description = description;
            this.defaultTool = defaultTool;
            this.learnCurve = learnCurve;
            this.totalSkillTime = totalSkillTime;
            this.learnTime1 = totalSkillTime / NonLinearCurves.GetIntegral(learnCurve);
            this.group = group;
            this.strengthFactor = strengthFactor;
            this.dexterityFactor = dexterityFactor;
            this.agilityFactor = agilityFactor;
            this.perceptionFactor = perceptionFactor;
        }
    }

    public enum Skill
    {
        NoSkill = 0,
        BluntWeapons = 1,
        Swordplay = 2,
        Stabbing = 3,
        LeatherArmor = 4,
        MediumArmor = 5,
        PlateArmour = 6,
        Parry = 7,
        Dodge = 8,
        Mining = 9,
        Lumbering = 10,
        Herbology = 11,
        WaterMagic = 12,
        AirMagic = 13,
        EarthMagic = 14,
        FireMagic = 15
    }


    public const int maxSkills = 16;

    public static SkillInfo[] info =
    {
        //skill, name, headline, description, defaultTool, learnCurve, totalSkillTime, group, strengthFactor, dexterityFactor, agilityFactor, perceptionFactor
        new SkillInfo(Skill.NoSkill, "No Skill", "Crap!","This is the most useless skill ever. There is no application known.","hands",GlobalVar.skillLearnCurveDefault, GlobalVar.skillTotalLearnTimeDefault, 0,1,1,1,1),
        new SkillInfo(Skill.BluntWeapons, "Blunt Weapons", "Ramming your enemy into the ground. Why not?","For this you need weapons without sharp points. This is especially effective for heavily armored enemies. Easily armored enemies are not so easy to overcome.","blunt weapon",GlobalVar.skillLearnCurveDefault, GlobalVar.skillTotalLearnTimeDefault, 1,3,2,1,0),
        new SkillInfo(Skill.Swordplay, "Swordplay", "The dream of every childen and a lot of men.","You can use swords and axes. This is especially good against medium armor but not against heavily armored enemies.","sword",GlobalVar.skillLearnCurveDefault, GlobalVar.skillTotalLearnTimeDefault, 1,2,3,2,1),
        new SkillInfo(Skill.Stabbing, "Stabbing", "Please come closer.","This skill is for small pointy weapons such as daggers or even kitchen knives. Lightly armored enemies are the best targets, while a medium armor is hard to deal with.","dagger",GlobalVar.skillLearnCurveDefault, GlobalVar.skillTotalLearnTimeDefault, 1,1,2,3,2),
        new SkillInfo(Skill.LeatherArmor, "Leather Armor", "Come on, I'm gone.","A protection for every day. Blunt weapons can be easily intercepted, but swords get through.","leather",GlobalVar.skillLearnCurveDefault, GlobalVar.skillTotalLearnTimeDefault, 2,1,2,3,0),
        new SkillInfo(Skill.MediumArmor, "Medium Armor", "Shiny, expensive and useful.","You know how to use the best-looking armor, which is excellent against swords but has too many gaps against knives.","chainmail",GlobalVar.skillLearnCurveDefault, GlobalVar.skillTotalLearnTimeDefault, 2,2,3,1,0),
        new SkillInfo(Skill.PlateArmour, "Plate Armour", "Impenetrable but heavy.","It is an art to wear heavy armor. Swords and axes make you a little but a club is giving you a headache.","iron plate",GlobalVar.skillLearnCurveDefault, GlobalVar.skillTotalLearnTimeDefault, 2,3,1,2,0),
        new SkillInfo(Skill.Parry, "Parry", "There is always something in between.","Shields are not only good for wearing coats of arms. You can also avoid some scratches on the armor. Also, weapons can sometimes block blows. You are always stunned when you parry an attack.","shield",GlobalVar.skillLearnCurveDefault, GlobalVar.skillTotalLearnTimeDefault, 3,3,2,2,0),
        new SkillInfo(Skill.Dodge, "Dodge", "I'll be somewhere else for a while.","The best way to prevent wounds is to avoid enemy weapons. This requires some skill, because your opponent is also watching closely.","feet",GlobalVar.skillLearnCurveDefault, GlobalVar.skillTotalLearnTimeDefault, 3,0,1,3,2),
        new SkillInfo(Skill.Mining, "Mining", "Luck up!","Darkness, tight studs and hard work are your thing. He who is a good miner beckons wealth in many different ways. Experienced miners even know how to find precious diamonds or adamant.","pickaxe",GlobalVar.skillLearnCurveDefault, GlobalVar.skillTotalLearnTimeDefault, 4,2,0,1,2),
        new SkillInfo(Skill.Lumbering, "Lumbering", "Timber!","Wood is the universal building material. So felled trees are needed. For a hard working fellow a single forest may not be enough. Experienced lumberjacks can fell trees, other not even kow they exist. Have you ever heard about a iron wood tree?","fellaxe",GlobalVar.skillLearnCurveDefault, GlobalVar.skillTotalLearnTimeDefault, 4,2,2,1,0),
        new SkillInfo(Skill.Herbology, "Herbology", "Deadly poison or live saving potion, where's the difference?","There is nothing that no herb can cope with. Whether conspicuous or inconspicuous, herbs appear as different as their use. Many a special herb only recognizes the expert.","Herb cutter",GlobalVar.skillLearnCurveDefault, GlobalVar.skillTotalLearnTimeDefault, 4,0,1,1,3),
        new SkillInfo(Skill.WaterMagic, "Water Magic", "Everything flows more or less","Be one with everything that changes. Some call this type of magic the magic of life. Here you will find healing spells. Shields or attacks made of water or ice are rather weak. Masters can even undo a character's death.","wand",GlobalVar.skillLearnCurveDefault, GlobalVar.skillTotalLearnTimeDefault, 5,0,1,2,3),
        new SkillInfo(Skill.AirMagic, "Air Magic", "Let yourself drift, sometimes here, sometimes there.","Be one with the wind. Like the wind, you will be able to get almost anywhere or get everything. Here you will find teleport spells, weak attacks. Masters can build portals and depots.","wand",GlobalVar.skillLearnCurveDefault,     GlobalVar.skillTotalLearnTimeDefault, 5,0,3,1,2),
        new SkillInfo(Skill.EarthMagic, "Earth Magic", "Don't forget a solid foundation","You stand on the ground with both legs and it obeys you. Shape whatever you like. You can feel the mana in the ground, erect or collapse powerful shields and fragile bridges. Earth-based attacks don't go far. Masters can make minions from what is found in the ground.","wand",GlobalVar.skillLearnCurveDefault, GlobalVar.skillTotalLearnTimeDefault, 5,0,2,1,3),
        new SkillInfo(Skill.FireMagic, "Fire Magic", "Attack is the best defense","It's almost as if there is no difference in lighting a campfire or burning an enemy. You are burning with desire to do that. You will find many powerful attacks here, but only weak shields. Masters can knock out overpowered enemies in one fell swoop.","wand",GlobalVar.skillLearnCurveDefault,   GlobalVar.skillTotalLearnTimeDefault, 5,0,1,3,2)
    };

    // max permitted summary points per group
    private static int[] groups =
    {
        5050, //0-dummy group
        7250, //1-weapons
        7250, //2-armor
        6500, //3-dodge and parry
        7600, //4-gathering
        7350  //5-magic
    };

    public static string[] groupText =
    {
        "no skill",
        "weapons",
        "armor",
        "dodge and parry",
        "gathering",
        "magic"
    };

    public static Dictionary<Tuple<Skill, Skill>, float> relations = new Dictionary<Tuple<Skill, Skill>, float>();
    private static void InitializeRelations()
    {
        relations.Clear();
        relations.Add(new Tuple<Skill, Skill>(Skill.BluntWeapons, Skill.LeatherArmor), 1.67f);
        relations.Add(new Tuple<Skill, Skill>(Skill.BluntWeapons, Skill.MediumArmor), 1.00f);
        relations.Add(new Tuple<Skill, Skill>(Skill.BluntWeapons, Skill.PlateArmour), 0.60f);
        relations.Add(new Tuple<Skill, Skill>(Skill.BluntWeapons, Skill.Dodge), 2.00f);
        relations.Add(new Tuple<Skill, Skill>(Skill.Swordplay, Skill.LeatherArmor), 1.00f);
        relations.Add(new Tuple<Skill, Skill>(Skill.Swordplay, Skill.MediumArmor), 0.60f);
        relations.Add(new Tuple<Skill, Skill>(Skill.Swordplay, Skill.PlateArmour), 1.67f);
        relations.Add(new Tuple<Skill, Skill>(Skill.Swordplay, Skill.Dodge), 1.00f);
        relations.Add(new Tuple<Skill, Skill>(Skill.Stabbing, Skill.LeatherArmor), 0.60f);
        relations.Add(new Tuple<Skill, Skill>(Skill.Stabbing, Skill.MediumArmor), 1.67f);
        relations.Add(new Tuple<Skill, Skill>(Skill.Stabbing, Skill.PlateArmour), 1.00f);
        relations.Add(new Tuple<Skill, Skill>(Skill.Stabbing, Skill.Dodge), 0.50f);
    }
    public static float GetRelation(Skill attacker, Skill defender)
    {
        if (relations.Count == 0)
        {
            InitializeRelations();
        }
        if (relations.TryGetValue(new Tuple<Skill, Skill>(attacker, defender), out float result))
        {
            return result;
        }
        return 1f;
    }


    public static int TimePerLevel(Skill skill, int level)
    {
        int skillId = IdFromSkill(skill);
        return (int)(NonLinearCurves.GetValue(info[skillId].learnCurve, level) * info[skillId].learnTime1);
    }

    public static int MaxPointGroup(Skills.Skill skill)
    {
        return MaxPointGroup(Skills.IdFromSkill(skill));
    }
    public static int MaxPointGroup(int skillId)
    {
        return groups[info[skillId].group];
    }

    public static int Level(int experience)
    {
        return Mathf.Clamp(experience / GlobalVar.skillExperiencePerLevel, 0, 100);
    }

    /// <summary>
    /// Id from name, name can be id itself
    /// </summary>
    public static int IdFromName(string name)
    {
        int skillId = -1;
        if (int.TryParse(name, out skillId))
        {
            if (skillId >= Skills.info.Length)
                skillId = -1;
            return skillId;
        }
        else
        {
            for (int id = 0; id < info.Length; id++)
            {
                if (info[id].name == name)
                    return id;
            }
            return -1;
        }
    }

    /// <summary>
    /// Skill from name, NoSkill if not found
    /// </summary>
    public static Skill SkillFromName(string name)
    {
        for (int id = 0; id < info.Length; id++)
        {
            if (info[id].name == name)
            {
                return info[id].skill;
            }
        }
        return Skill.NoSkill;
    }

    public static Skill SkillFromId(int id)
    {
        return (Skill)id;
    }
    public static int IdFromSkill(Skill skill)
    {
        return (int)skill;
    }

    public static string SerializeDefaultSkills(int[] defaultSkills)
    {
        string result = "";
        for (int i = 0; i < defaultSkills.Length; i++)
        {
            if (defaultSkills[i] > 0)
            {
                result += string.Format("{0};{1};", i, defaultSkills[i]);
            }
        }
        return result;
    }

    public static int[] DeserializeDefaultSkills(string serializedDefaultSkill)
    {
        int[] defaultSkills = new int[maxSkills];
        string[] tmp = serializedDefaultSkill.Split(';');

        int i = 0;
        while (i + 2 < tmp.Length)
        {
            if (int.TryParse(tmp[i], out int index))
            {
                if (int.TryParse(tmp[i + 1], out int value) && index < maxSkills)
                {
                    defaultSkills[index] = value;
                }
            }
            i += 2;
        }
        return defaultSkills;
    }

    public static string Name(Skill skill)
    {
        return info[IdFromSkill(skill)].name;
    }
}


[Serializable]
public partial struct SkillExperience
{
    public int id;
    public int experience;

    // constructors
    public SkillExperience(int id, int experience)
    {
        this.id = id;
        this.experience = experience;
    }
}
public class SyncListSkill : SyncListSTRUCT<SkillExperience>
{
    public int IndexOfId(int id)
    {
        return this.FindIndex(x => x.id == id);
    }

    public int ExperienceOfId(int id)
    {
        int index = IndexOfId(id);
        if (index == -1)
            return 0;
        return this[index].experience;
    }

    public int LevelOfId(int id)
    {
        int index = IndexOfId(id);
        if (index == -1)
            return 0;
        return Skills.Level(this[index].experience);
    }

    public int LevelOfSkill(Skills.Skill skill)
    {
        return LevelOfId(Skills.IdFromSkill(skill));
    }

    public void AddExperience(Skills.Skill skillToAdd, int experience)
    {
        AddExperience(Skills.IdFromSkill(skillToAdd), experience);
    }
    public void AddExperience(int skillId, int experience)
    {
        int index = IndexOfId(skillId);
        if (index == -1)
            this.Add(new SkillExperience(skillId, experience));
        else
        {
            // we cannot change a struct value direct
            SkillExperience skill = this[index];
            skill.experience += experience;
            this[index] = skill;
        }
    }

    public void SetExperience(int id, int experience)
    {
        int index = IndexOfId(id);
        if (index == -1)
            this.Add(new SkillExperience(id, experience));
        else
        {
            // we cannot change a struct value direct
            SkillExperience skill = this[index];
            skill.experience = experience;
            this[index] = skill;
        }
    }

    public int PointsInGroup(Skills.Skill skill)
    {
        return PointsInGroup(Skills.IdFromSkill(skill));
    }
    public int PointsInGroup(int id)
    {
        int index = IndexOfId(id);
        if (index == -1)
            return 0;
        int group = Skills.info[index].group;
        int points = 0;
        foreach (SkillExperience skill in this)
        {
            if (Skills.info[skill.id].group == group)
            {
                int level = skill.experience / GlobalVar.skillExperiencePerLevel;
                points += (1 + level) * level / 2;
            }
        }
        return points;
    }
}
