/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// Saves Character Data in a SQLite database. We use SQLite for serveral reasons
//
// - SQLite is file based and works without having to setup a database server
//   - We can 'remove all ...' or 'modify all ...' easily via SQL queries
//   - A lot of people requested a SQL database and weren't comfortable with XML
//   - We can allow all kinds of character names, even chinese ones without
//     breaking the file system.
// - We will need MYSQL or similar when using multiple server instances later
//   and upgrading is trivial
// - XML is easier, but:
//   - we can't easily read 'just the class of a character' etc., but we need it
//     for character selection etc. often
//   - if each account is a folder that contains players, then we can't save
//     additional account info like password, banned, etc. unless we use an
//     additional account.xml file, which overcomplicates everything
//   - there will always be forbidden file names like 'COM', which will cause
//     problems when people try to create accounts or characters with that name
//
// About item mall coins:
//   The payment provider's callback should add new orders to the
//   character_orders table. The server will then process them while the player
//   is ingame. Don't try to modify 'coins' in the character table directly.
//
// Tools to open sqlite database files:
//   Windows/OSX program: http://sqlitebrowser.org/
//   Firefox extension: https://addons.mozilla.org/de/firefox/addon/sqlite-manager/
//   Webhost: Adminer/PhpLiteAdmin
//
// About performance:
// - It's recommended to only keep the SQlite connection open while it's used.
//   MMO Servers use it all the time, so we keep it open all the time. This also
//   allows us to use transactions easily, and it will make the transition to
//   MYSQL easier.
// - Transactions are definitely necessary:
//   saving 100 players without transactions takes 3.6s
//   saving 100 players with transactions takes    0.38s
// - Using tr = conn.BeginTransaction() + tr.Commit() and passing it through all
//   the functions is ultra complicated. We use a BEGIN + END queries instead.
//
// Some benchmarks:
//   saving 100 players unoptimized: 4s
//   saving 100 players always open connection + transactions: 3.6s
//   saving 100 players always open connection + transactions + WAL: 3.6s
//   saving 100 players in 1 'using tr = ...' transaction: 380ms
//   saving 100 players in 1 BEGIN/END style transactions: 380ms
//   saving 100 players with XML: 369ms
//
// Build notes:
// - requires Player settings to be set to '.NET' instead of '.NET Subset',
//   otherwise System.Data.dll causes ArgumentException.
// - requires sqlite3.dll x86 and x64 version for standalone (windows/mac/linux)
//   => found on sqlite.org website
// - requires libsqlite3.so x86 and armeabi-v7a for android
//   => compiled from sqlite.org amalgamation source with android ndk r9b linux
using UnityEngine;
using UnityEditor;
using Mirror;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Mono.Data.Sqlite; // copied from Unity/Mono/lib/mono/2.0 to Plugins
public partial class Database
{
    // database path: Application.dataPath is always relative to the project,
    // but we don't want it inside the Assets folder in the Editor (git etc.),
    // instead we put it above that.
    // we also use Path.Combine for platform independent paths
    // and we need persistentDataPath on android
#if UNITY_EDITOR
    static string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Database.sqlite");
#elif UNITY_ANDROID
    static string path = Path.Combine(Application.persistentDataPath, "Database.sqlite");
#elif UNITY_IOS
    static string path = Path.Combine(Application.persistentDataPath, "Database.sqlite");
#else
    static string path = Path.Combine(Application.dataPath, "Database.sqlite");
#endif
    static SqliteConnection connection;
    // constructor /////////////////////////////////////////////////////////////
    static Database()
    {
        // create database file if it doesn't exist yet
        if (!File.Exists(path))
            SqliteConnection.CreateFile(path);
        // open connection
        connection = new SqliteConnection("URI=file:" + path);
        connection.Open();
        // create tables if they don't exist yet or were deleted
        // [PRIMARY KEY is important for performance: O(log n) instead of O(n)]
        ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS characters (
                            id INTEGER NOT NULL PRIMARY KEY,
                            account TEXT NOT NULL,
                            class TEXT NOT NULL,
                            displayName TEXT NOT NULL,
                            x REAL NOT NULL,
                            y REAL NOT NULL,
                            z REAL NOT NULL,
                            health INTEGER NOT NULL,
                            injury INTEGER NOT NULL,
                            mana INTEGER NOT NULL,
                            stamina INTEGER NOT NULL,
                            skilltime INTEGER NOT NULL,
                            playtime INTEGER NOT NULL,
                            online TEXT NOT NULL,
                            abilities TEXT NOT NULL,
                            attributes TEXT NOT NULL,
                            apperance TEXT NOT NULL,
                            gmstate TEXT NOT NULL,
                            faction INTEGER NOT NULL,
                            lastplayed  DATETIME NOT NULL DEFAULT(DATETIME('now')),
                            created  DATETIME NOT NULL DEFAULT(DATETIME('now')),
                            deleted INTEGER NOT NULL DEFAULT (0))");
        ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS character_names (
                            charid INTEGER NOT NULL,
                            namedcharid INTEGER NOT NULL,
                            displayName TEXT NOT NULL,
                            state INTEGER NOT NULL DEFAULT(2),
                            PRIMARY KEY(charid, namedcharid ))");
        // [PRIMARY KEY is important for performance: O(log n) instead of O(n)]
        ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS character_inventory (
                            containerid INTEGER NOT NULL,
                            charid INTEGER NOT NULL,
                            slot INTEGER NOT NULL,
                            name TEXT NOT NULL,
                            amount INTEGER NOT NULL,
                            data1 INTEGER NOT NULL,
                            data2 INTEGER NOT NULL,
                            data3 INTEGER NOT NULL,
                            durability INTEGER NOT NULL,
                            quality INTEGER NOT NULL,
                            miscellaneous TEXT NOT NULL,
                            PRIMARY KEY(containerid, charid, slot))");
        ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS character_container (
                            containerid INTEGER NOT NULL,
                            charid INTEGER NOT NULL,
                            type INTEGER NOT NULL,
                            name TEXT NOT NULL,
                            slots INTEGER NOT NULL,
                            containers INTEGER NOT NULL,
                            miscellaneous TEXT NOT NULL,
                            PRIMARY KEY(containerid, charid))");
        ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS character_skills (
                            charid INTEGER NOT NULL,
                            skillid INTEGER NOT NULL,
                            experience INTEGER NOT NULL,
                            PRIMARY KEY(charid, skillid))");

        // [PRIMARY KEY is important for performance: O(log n) instead of O(n)]
        ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS character_spells (
                            character TEXT NOT NULL,
                            name TEXT NOT NULL,
                            castTimeEnd REAL NOT NULL,
                            cooldownEnd REAL NOT NULL,
                            PRIMARY KEY(character, name))");
        // [PRIMARY KEY is important for performance: O(log n) instead of O(n)]
        ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS character_buffs (
                            character TEXT NOT NULL,
                            name TEXT NOT NULL,
                            level REAL NOT NULL,
                            buffTimeEnd REAL NOT NULL,
                            PRIMARY KEY(character, name))");
        // [PRIMARY KEY is important for performance: O(log n) instead of O(n)]
        ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS character_quests (
                            charid INTEGER NOT NULL,
                            name TEXT NOT NULL,
                            field0 INTEGER NOT NULL,
                            completed INTEGER NOT NULL,
                            PRIMARY KEY(charid, name))");
        // INTEGER PRIMARY KEY is auto incremented by sqlite if the
        // insert call passes NULL for it.
        // [PRIMARY KEY is important for performance: O(log n) instead of O(n)]
        ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS character_orders (
                            orderid INTEGER PRIMARY KEY,
                            character TEXT NOT NULL,
                            coins INTEGER NOT NULL,
                            processed INTEGER NOT NULL)");
        // [PRIMARY KEY is important for performance: O(log n) instead of O(n)]
        // guild members are saved in a separate table because instead of in a
        // characters.guild field because:
        // * guilds need to be resaved independently, not just in CharacterSave
        // * kicked members' guilds are cleared automatically because we drop
        //   and then insert all members each time. otherwise we'd have to
        //   update the kicked member's guild field manually each time
        // * it's easier to remove / modify the guild feature if it's not hard-
        //   coded into the characters table
        ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS character_guild (
                            character TEXT NOT NULL PRIMARY KEY,
                            guild TEXT NOT NULL,
                            rank INTEGER NOT NULL)");
        // add index on guild to avoid full scans when loading guild members
        ExecuteNonQuery("CREATE INDEX IF NOT EXISTS character_guild_by_guild ON character_guild (guild)");
        // guild master is not in guild_info in case we need more than one later
        // [PRIMARY KEY is important for performance: O(log n) instead of O(n)]
        ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS guild_info (
                            name TEXT NOT NULL PRIMARY KEY,
                            notice TEXT NOT NULL)");
        // [PRIMARY KEY is important for performance: O(log n) instead of O(n)]
        ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS accounts (
                            name TEXT NOT NULL PRIMARY KEY,
                            password TEXT NOT NULL,
                            email TEXT NOT NULL,
                            banned INTEGER NOT NULL DEFAULT (0),
                            created  DATETIME NOT NULL DEFAULT(DATETIME('now')))");

        // NPC trade data
        // in database to organize easily
        // List of all available items
        ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS ang_item_info (
                            name TEXT NOT NULL PRIMARY KEY,
                            baseprice INTEGER NOT NULL)");
        // List of all available merchants
        ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS ang_npcmerchant_info (
                            name TEXT NOT NULL PRIMARY KEY,
                            buyallsellitems INTEGER NOT NULL)");
        // List of all buy and sell groups
        ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS ang_buysellgroup (
                            id INTEGER AUTO_INCREMENT PRIMARY KEY,
                            name TEXT NOT NULL,
                            npcmerchant TEXT NOT NULL,
                            sort INTEGER NOT NULL,
                            isbuy INTEGER NOT NULL,
                            pricelevel REAL NOT NULL)");
        // List of all buy and sell items
        ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS ang_buysellitems (
                            id INTEGER AUTO_INCREMENT PRIMARY KEY,
                            buysellgroup INTEGER NOT NULL,
                            itemname TEXT NOT NULL,
                            durability INTEGER NOT NULL,
                            quality INTEGER NOT NULL,
                            pricelevel REAL NOT NULL)");
    }

    // helper functions ////////////////////////////////////////////////////////
    // run a query that doesn't return anything
    public static void ExecuteNonQuery(string sql, params SqliteParameter[] args)
    {
        using (SqliteCommand command = new SqliteCommand(sql, connection))
        {
            foreach (SqliteParameter param in args)
                command.Parameters.Add(param);
            command.ExecuteNonQuery();
        }
    }
    // run a query that returns a single value
    public static object ExecuteScalar(string sql, params SqliteParameter[] args)
    {
        using (SqliteCommand command = new SqliteCommand(sql, connection))
        {
            foreach (SqliteParameter param in args)
                command.Parameters.Add(param);
            return command.ExecuteScalar();
        }
    }
    // run a query that returns several values
    // note: sqlite has long instead of int, so use Convert.ToInt32 etc.
    public static List<List<object>> ExecuteReader(string sql, params SqliteParameter[] args)
    {
        List<List<object>> result = new List<List<object>>();
        using (SqliteCommand command = new SqliteCommand(sql, connection))
        {
            foreach (SqliteParameter param in args)
                command.Parameters.Add(param);
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                // the following code causes a SQL EntryPointNotFoundException
                // because sqlite3_column_origin_name isn't found on OSX and
                // some other platforms. newer mono versions have a workaround,
                // but as long as Unity doesn't update, we will have to work
                // around it manually. see also GetSchemaTable function:
                // https://github.com/mono/mono/blob/master/mcs/class/Mono.Data.Sqlite/Mono.Data.Sqlite_2.0/SQLiteDataReader.cs
                //
                //result.Load(reader); (DataTable)
                while (reader.Read())
                {
                    object[] buffer = new object[reader.FieldCount];
                    reader.GetValues(buffer);
                    result.Add(buffer.ToList());
                }
            }
        }
        return result;
    }
    // account data ////////////////////////////////////////////////////////////
    public static bool IsValidAccount(string account, string password)
    {
        // this function can be used to verify account credentials in a database
        // or a content management system.
        //
        // for example, we could setup a content management system with a forum,
        // news, shop etc. and then use a simple HTTP-GET to check the account
        // info, for example:
        //
        //   var request = new WWW("example.com/verify.php?id="+id+"&amp;pw="+pw);
        //   while (!request.isDone)
        //       print("loading...");
        //   return request.error == null && request.text == "ok";
        //
        // where verify.php is a script like this one:
        //   <?php
        //   // id and pw set with HTTP-GET?
        //   if (isset($_GET['id']) && isset($_GET['pw'])) {
        //       // validate id and pw by using the CMS, for example in Drupal:
        //       if (user_authenticate($_GET['id'], $_GET['pw']))
        //           echo "ok";
        //       else
        //           echo "invalid id or pw";
        //   }
        //   ?>
        //
        // or we could check in a MYSQL database:
        //   var dbConn = new MySql.Data.MySqlClient.MySqlConnection("Persist Security Info=False;server=localhost;database=notas;uid=root;password=" + dbpwd);
        //   var cmd = dbConn.CreateCommand();
        //   cmd.CommandText = "SELECT id FROM accounts WHERE id='" + account + "' AND pw='" + password + "'";
        //   dbConn.Open();
        //   var reader = cmd.ExecuteReader();
        //   if (reader.Read())
        //       return reader.ToString() == account;
        //   return false;
        //
        // as usual, we will use the simplest solution possible:
        // create account if not exists, compare password otherwise.
        // no CMS communication necessary and good enough for an Indie MMORPG.
        // not empty?
        if (!Utils.IsNullOrWhiteSpace(account) && !Utils.IsNullOrWhiteSpace(password))
        {
            List<List<object>> table = ExecuteReader("SELECT password, banned FROM accounts WHERE name=@name", new SqliteParameter("@name", account));
            if (table.Count == 1)
            {
                // account exists. check password and ban status.
                List<object> row = table[0];
                return (string)row[0] == password && (long)row[1] == 0;
            }
            else
            {
                // account doesn't exist. create it.
                ExecuteNonQuery("INSERT INTO accounts (name, password, email) VALUES (@name, @password, @email)",
                    new SqliteParameter("@name", account),
                    new SqliteParameter("@password", password),
                    new SqliteParameter("@email", "dummy@mail"));
                return true;
            }
        }
        return false;
    }
    // character data //////////////////////////////////////////////////////////
    public static bool CharacterExists(int characterId)
    {
        // checks deleted ones too so we don't end up with duplicates if we un-
        // delete one
        return ((long)ExecuteScalar("SELECT Count(*) FROM characters WHERE id=@id", new SqliteParameter("@id", characterId))) == 1;
    }
    public static bool DisplayNameExists(string displayName)
    {
        // checks deleted ones too so we don't end up with duplicates if we un-
        // delete one
        return ((long)ExecuteScalar("SELECT Count(*) FROM characters WHERE displayName=@displayName", new SqliteParameter("@displayName", displayName))) == 1;
    }
    public static void CharacterDelete(int characterId)
    {
        // soft delete the character so it can always be restored later
        ExecuteNonQuery("UPDATE characters SET deleted=1 WHERE id=@id", new SqliteParameter("@id", characterId));
    }
    // returns the list of character ids for that account
    // => all the other values can be read with CharacterLoad!
    public static List<int> CharactersForAccount(string account)
    {
        List<int> result = new List<int>();
        List<List<object>> table = ExecuteReader("SELECT id FROM characters WHERE account=@account AND deleted=0", new SqliteParameter("@account", account));
        foreach (List<object> row in table)
            result.Add(Convert.ToInt32((long)row[0]));
        return result;
    }
    static void LoadInventory(Player player)
    {
        // fill clear inventory for reload purpose
        player.inventory.Clear();
        player.containers.Clear();

        // load containers first
        List<List<object>> tableContainer = ExecuteReader("SELECT containerid, type, name, slots, containers, miscellaneous " +
                                                         "FROM character_container WHERE charid=@charid",
            new SqliteParameter("@charid", player.id));
        foreach (List<object> row in tableContainer)
        {
            int containerid = Convert.ToInt32((long)row[0]);
            int type = Convert.ToInt32((long)row[1]);
            string containerName = (string)row[2];
            int slots = Convert.ToInt32((long)row[3]);
            int containers = Convert.ToInt32((long)row[4]);
            string miscellaneous = (string)row[5];
            player.containers.Add(new Container(containerid, type, slots, containers, containerName, miscellaneous));
        }

        // then load valid items and put into the list
        // (one big query is A LOT faster than querying each slot separately)
        List<List<object>> table = ExecuteReader("SELECT name, slot, containerid, amount, data1, data2, data3, durability, quality, miscellaneous " +
                                                 "FROM character_inventory WHERE charid=@charid",
            new SqliteParameter("@charid", player.id));
        foreach (List<object> row in table)
        {
            string itemName = (string)row[0];
            int slot = Convert.ToInt32((long)row[1]);
            int container = Convert.ToInt32((long)row[2]);
            bool load = true;

            ScriptableItem itemData;
            if (ScriptableItem.dict.TryGetValue(itemName.GetStableHashCode(), out itemData))
            {
                Item item = new Item(itemData);
                int amount = Convert.ToInt32((long)row[3]);
                item.data1 = Convert.ToInt32((long)row[4]);
                item.data2 = Convert.ToInt32((long)row[5]);
                item.data3 = Convert.ToInt32((long)row[6]);
                item.durability = Convert.ToInt32((long)row[7]);
                item.quality = Convert.ToInt32((long)row[8]);
                item.miscellaneousSync = (string)row[9];

                // light: always off on load and don't load single use burning items
                if (item.data is LightItem)
                {
                    LightItem le = (LightItem)item.data;
                    if (le.canExtinguished || item.data2 == 0)
                        item.data2 = 0;
                    else
                        load = false;
                }

                if (load)
                    player.inventory.Add(new ItemSlot(item, container, slot, amount));
            }
            else LogFile.WriteLog(LogFile.LogLevel.Error, "LoadInventory: skipped item " + itemName + " for " + player.name + " because it doesn't exist anymore. If it wasn't removed intentionally then make sure it's in the Resources folder.");
        }
    }
    static void LoadSkills(Player player)
    {
        List<List<object>> table = ExecuteReader("SELECT charid, skillid, experience FROM character_skills WHERE charid=@charid", new SqliteParameter("@charid", player.id));
        foreach (List<object> row in table)
        {
            SkillExperience skill = new SkillExperience();
            skill.id = Convert.ToInt32((long)row[1]);
            skill.experience = Convert.ToInt32((long)row[2]);
            player.skills.Add(skill);
        }
    }

    static void LoadSpells(Player player)
    {
        // load spells based on spell templates (the others don't matter)
        // -> this way any spell changes in a prefab will be applied
        //    to all existing players every time (unlike item templates
        //    which are only for newly created characters)
        // fill all slots first
        foreach (ScriptableSpell spellData in player.spellTemplates)
            player.spells.Add(new Spell(spellData));
        // then load learned spells and put into their slots
        // (one big query is A LOT faster than querying each slot separately)
        List<List<object>> table = ExecuteReader("SELECT name, castTimeEnd, cooldownEnd FROM character_spells WHERE character=@character", new SqliteParameter("@character", player.name));
        foreach (List<object> row in table)
        {
            string spellName = (string)row[0];
            int index = player.spells.FindIndex(spell => spell.name == spellName);
            if (index != -1) //spell exists in spelltemplates
            {
                Spell spell = player.spells[index];
                // castTimeEnd and cooldownEnd are based on NetworkTime.time
                // which will be different when restarting a server, hence why
                // we saved them as just the remaining times. so let's convert
                // them back again.
                spell.castTimeEnd = (float)row[1] + NetworkTime.time;
                spell.cooldownEnd = (float)row[2] + NetworkTime.time;
                player.spells[index] = spell;
            }
            else
            {
                ScriptableSpell spellData;
                if (ScriptableSpell.dict.TryGetValue(spellName.GetStableHashCode(), out spellData))
                {
                    Spell spell = new Spell(spellData);
                    player.spells.Add(spell);
                }
                else
                {
                    LogFile.WriteLog(LogFile.LogLevel.Error,string.Format("LoadSpells: skipped spell {0} for {1} because it doesn't exist anymore. If it wasn't removed intentionally then make sure it's in the Resources folder."
                        ,spellName
                        ,player.name));
                }
            }
        }
    }
    static void LoadBuffs(Player player)
    {
        // load buffs
        // note: no check if we have learned the spell for that buff
        //       since buffs may come from other people too
        List<List<object>> table = ExecuteReader("SELECT name, level, buffTimeEnd FROM character_buffs WHERE character=@character", new SqliteParameter("@character", player.name));
        foreach (List<object> row in table)
        {
            string buffName = (string)row[0];
            ScriptableSpell spellData;
            if (ScriptableSpell.dict.TryGetValue(buffName.GetStableHashCode(), out spellData))
            {
                Buff buff = new Buff((BuffSpell)spellData, 0);
                // buffTimeEnd is based on NetworkTime.time, which will be
                // different when restarting a server, hence why we saved
                // them as just the remaining times. so let's convert them
                // back again.
                buff.level = (float)row[1];
                buff.buffTimeEnd = (float)row[2] + NetworkTime.time;
                player.buffs.Add(buff);
            }
            else
            {
                LogFile.WriteLog(LogFile.LogLevel.Error, string.Format("LoadBuffs: skipped buff {0} for {1} because it doesn't exist anymore. If it wasn't removed intentionally then make sure it's in the Resources folder."
                    ,buffName,player.name));
            }
        }
    }
    static void LoadQuests(Player player)
    {
        // load quests
        int charid = player.id;
        List<List<object>> table = ExecuteReader("SELECT name, field0, completed FROM character_quests WHERE charid=@charid", new SqliteParameter("@charid", charid));
        foreach (List<object> row in table)
        {
            string questName = (string)row[0];
            ScriptableQuest questData;
            if (ScriptableQuest.dict.TryGetValue(questName.GetStableHashCode(), out questData))
            {
                Quest quest = new Quest(questData);
                quest.field0 = Convert.ToInt32((long)row[1]);
                quest.completed = ((long)row[2]) != 0; // sqlite has no bool
                player.quests.Add(quest);
            }
            else
            {
                LogFile.WriteLog(LogFile.LogLevel.Error, string.Format("LoadQuests: skipped quest {0} for [1} because it doesn't exist anymore. If it wasn't removed intentionally then make sure it's in the Resources folder."
              , questName, player.name));
            }
        }
    }
    static void LoadGuild(Player player)
    {
        // in a guild?
        string guild = (string)ExecuteScalar("SELECT guild FROM character_guild WHERE character=@character", new SqliteParameter("@character", player.name));
        if (guild != null)
        {
            // load guild info
            player.guildName = guild;
            List<List<object>> table = ExecuteReader("SELECT notice FROM guild_info WHERE name=@guild", new SqliteParameter("@guild", guild));
            if (table.Count == 1)
            {
                List<object> row = table[0];
                player.guild.notice = (string)row[0];
            }
            // load members list
            List<GuildMember> members = new List<GuildMember>();
            table = ExecuteReader("SELECT character, rank FROM character_guild WHERE guild=@guild", new SqliteParameter("@guild", player.guildName));
            foreach (List<object> row in table)
            {
                GuildMember member = new GuildMember();
                member.name = (string)row[0];
                member.rank = (GuildRank)Convert.ToInt32((long)row[1]);
                member.online = Player.onlinePlayers.ContainsKey(member.name);
                members.Add(member);
            }
            player.guild.members = members.ToArray(); // guild.AddMember each time is too slow because array resizing
        }
    }
    static void LoadNames(Player player)
    {
        int charid = player.id;
        string tmpSync = "";
        List<List<object>> table = ExecuteReader("SELECT namedcharid, displayName, state FROM character_names WHERE charid=@charid", new SqliteParameter("@charid", charid));
        foreach (List<object> row in table)
        {
            tmpSync += ((long)row[0]).ToString() + "#";
            tmpSync += (string)row[1] + "#";
            tmpSync += ((long)row[2]).ToString() + "#";
        }
        player.knownNamesSnyc = tmpSync;
    }
    public static GameObject CharacterLoad(int characterId, List<Player> prefabs)
    {
        List<List<object>> table = ExecuteReader("SELECT * FROM characters WHERE id=@id AND deleted=0", new SqliteParameter("@id", characterId));
        if (table.Count == 1)
        {
            List<object> mainrow = table[0];
            // instantiate based on the class name
            string className = (string)mainrow[2];
            Player prefab = prefabs.Find(p => p.name == className);
            if (prefab != null)
            {
                GameObject go = GameObject.Instantiate(prefab.gameObject);
                Player player = go.GetComponent<Player>();

                player.name = Player.NameFromId(Convert.ToInt32((long)mainrow[0]));
                player.account = (string)mainrow[1];
                player.className = (string)mainrow[2];
                player.displayName = (string)mainrow[3];
                float x = (float)mainrow[4];
                float y = (float)mainrow[5];
                float z = (float)mainrow[6];
                Vector3 position = new Vector3(x, y, z);
                int health = Convert.ToInt32((long)mainrow[7]);
                int injury = Convert.ToInt32((long)mainrow[8]);
                int mana = Convert.ToInt32((long)mainrow[9]);
                int stamina = Convert.ToInt32((long)mainrow[10]);
                player.skillTotalTime = (long)mainrow[11];
                player.playtime = (long)mainrow[12];
                player.abilitiesSync = (string)mainrow[14];
                player.attributesSync = (string)mainrow[15];
                player.apperanceSync = (string)mainrow[16];
                player.gmState = GameMaster.CorrectSyncString((string)mainrow[17]);
                player.faction = Convert.ToInt32((long)mainrow[18]);

                // try to teleport to loaded position.
                // => teleport might fail if we changed the world since last save
                //    so we reset to start position if not on navmesh
                player.TeleportToPosition(position, 0, false);
                if (!player.agent.isOnNavMesh)
                {
                    Transform start = Universal.DefaultSpawn;
                    player.TeleportToPosition(start.position, 0, false);
                    LogFile.WriteLog(LogFile.LogLevel.Warning, player.name + " invalid position in database reset to default");
                }
                LoadInventory(player);
                LoadSkills(player);
                LoadSpells(player);
                LoadBuffs(player);
                LoadQuests(player);
                LoadGuild(player);
                LoadNames(player);

                // assign health / mana after max values were fully loaded
                // (they depend on equipment, buffs, etc.)
                player.InitializeCharacter(false);
                player.health = health;
                player.injury = injury;
                player.mana = mana;
                player.stamina = stamina;
                return go;
            }
            else LogFile.WriteLog(LogFile.LogLevel.Error, "Initialization found no prefab for class: " + className);
        }
        return null;
    }
    static void SaveInventory(Player player)
    {
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        int charid = player.id;
        ExecuteNonQuery("DELETE FROM character_inventory WHERE charid=@charid", new SqliteParameter("@charid", charid));
        ExecuteNonQuery("DELETE FROM character_container WHERE charid=@charid", new SqliteParameter("@charid", charid));
        for (int i = 0; i < player.containers.Count; i++)
        {
            Container container = player.containers[i];
            ExecuteNonQuery("INSERT INTO character_container VALUES (@containerid, @charid, @type, @name, @slots, @containers, @miscellaneous)",
                            new SqliteParameter("@containerid", container.id),
                            new SqliteParameter("@charid", charid),
                            new SqliteParameter("@type", container.type),
                            new SqliteParameter("@name", container.name),
                            new SqliteParameter("@slots", container.slots),
                            new SqliteParameter("@containers", container.containers),
                            new SqliteParameter("@miscellaneous", container.miscellaneousSync));
        }
        for (int i = 0; i < player.inventory.Count; ++i)
        {
            ItemSlot slot = player.inventory[i];
            if (slot.amount > 0) // only relevant items to save queries/storage/time
                ExecuteNonQuery("INSERT INTO character_inventory VALUES ( @containerid, @charid,@slot, @name, @amount, @data1, @data2, @data3, @durability, @quality, @miscellaneous)",
                                new SqliteParameter("@containerid", slot.container),
                                new SqliteParameter("@charid", charid),
                                new SqliteParameter("@slot", slot.slot),
                                new SqliteParameter("@name", slot.item.itemName),
                                new SqliteParameter("@amount", slot.amount),
                                new SqliteParameter("@data1", slot.item.data1),
                                new SqliteParameter("@data2", slot.item.data2),
                                new SqliteParameter("@data3", slot.item.data3),
                                new SqliteParameter("@durability", slot.item.durability),
                                new SqliteParameter("@quality", slot.item.quality),
                                new SqliteParameter("@miscellaneous", slot.item.miscellaneousSync));
        }
    }

    static void SaveSkills(Player player)
    {
        // remove old entries first, then add all new ones
        ExecuteNonQuery("DELETE FROM character_skills WHERE charid=@charid", new SqliteParameter("@charid", player.id));
        foreach (SkillExperience skill in player.skills)
        {
            if (skill.experience > 0)
            {

                ExecuteNonQuery("INSERT INTO character_skills VALUES (@charid, @skillid, @experience)",
                                new SqliteParameter("@charid", player.id),
                                new SqliteParameter("@skillid", skill.id),
                                new SqliteParameter("@experience", skill.experience));
            }
        }
    }

    static void SaveSpells(Player player)
    {
        // spells: remove old entries first, then add all new ones
        ExecuteNonQuery("DELETE FROM character_spells WHERE character=@character", new SqliteParameter("@character", player.name));
        foreach (Spell spell in player.spells)
        {
            // note: this does NOT work when trying to save character data
            //       shortly before closing the editor or game because
            //       NetworkTime.time is 0 then.
            ExecuteNonQuery("INSERT INTO character_spells VALUES (@character, @name, @castTimeEnd, @cooldownEnd)",
                            new SqliteParameter("@character", player.name),
                            new SqliteParameter("@name", spell.name),
                            new SqliteParameter("@castTimeEnd", spell.CastTimeRemaining()),
                            new SqliteParameter("@cooldownEnd", spell.CooldownRemaining()));
        }
    }
    static void SaveBuffs(Player player)
    {
        // buffs: remove old entries first, then add all new ones
        ExecuteNonQuery("DELETE FROM character_buffs WHERE character=@character", new SqliteParameter("@character", player.name));
        foreach (Buff buff in player.buffs)
            // buffTimeEnd is based on NetworkTime.time, which will be different
            // when restarting the server, so let's convert them to the
            // remaining time for easier save & load
            // note: this does NOT work when trying to save character data
            //       shortly before closing the editor or game because
            //       NetworkTime.time is 0 then.
            ExecuteNonQuery("INSERT INTO character_buffs VALUES (@character, @name, @level, @buffTimeEnd)",
                            new SqliteParameter("@character", player.name),
                            new SqliteParameter("@name", buff.name),
                            new SqliteParameter("@level", buff.level),
                            new SqliteParameter("@buffTimeEnd", buff.BuffTimeRemaining()));
    }
    static void SaveQuests(Player player)
    {
        // quests: remove old entries first, then add all new ones
        int charid = player.id;
        ExecuteNonQuery("DELETE FROM character_quests WHERE charid=@charid", new SqliteParameter("@charid", charid));
        foreach (Quest quest in player.quests)
            ExecuteNonQuery("INSERT INTO character_quests VALUES (@charid, @name, @field0, @completed)",
                            new SqliteParameter("@charid", charid),
                            new SqliteParameter("@name", quest.name),
                            new SqliteParameter("@field0", quest.field0),
                            new SqliteParameter("@completed", Convert.ToInt32(quest.completed)));
    }
    // adds character data in the database
    public static int CharacterCreate(Player player)
    {
        ExecuteNonQuery("BEGIN");
        string onlineString = "new";
        string abilityString = player.abilities.CreateString();
        string attributeString = player.attributes.CreateString();
        string apperanceString = player.apperance.CreateString();
        string timeString = DateTime.UtcNow.ToString("s");

        ExecuteNonQuery("INSERT INTO characters (account, class, displayName, x, y, z, health, injury, mana, stamina, skilltime, playtime, online, abilities, attributes, apperance, gmstate, lastplayed, faction) " +
            " VALUES (@account, @class, @displayName, @x, @y, @z, @health, @injury, @mana, @stamina, @skilltime, @playtime, @online, @abilities, @attributes, @apperance, @gmstate, @lastplayed, @faction)",
                        new SqliteParameter("@account", player.account),
                        new SqliteParameter("@class", player.className),
                        new SqliteParameter("@displayName", player.displayName),
                        new SqliteParameter("@x", player.transform.position.x),
                        new SqliteParameter("@y", player.transform.position.y),
                        new SqliteParameter("@z", player.transform.position.z),
                        new SqliteParameter("@health", player.health),
                        new SqliteParameter("@injury", player.injury),
                        new SqliteParameter("@mana", player.mana),
                        new SqliteParameter("@stamina", player.stamina),
                        new SqliteParameter("@skilltime", player.skillTotalTime),
                        new SqliteParameter("@playtime", player.playtime),
                        new SqliteParameter("@online", onlineString),
                        new SqliteParameter("@abilities", abilityString),
                        new SqliteParameter("@attributes", attributeString),
                        new SqliteParameter("@apperance", apperanceString),
                        new SqliteParameter("@gmstate", player.gmState),
                        new SqliteParameter("@lastplayed", timeString),
                        new SqliteParameter("@faction", player.faction));
        ExecuteNonQuery("END");

        // get the new created ID and return
        List<List<object>> table = ExecuteReader("SELECT id FROM characters WHERE account=@account AND online=@online",
            new SqliteParameter("@account", player.account),
            new SqliteParameter("@online", onlineString));
        int charid = 0;
        foreach (List<object> row in table)
            charid = Convert.ToInt32((long)row[0]);
        return charid;
    }
    // adds or overwrites character data in the database
    public static void CharacterSave(Player player, bool online, bool useTransaction = true)
    {
        // only use a transaction if not called within SaveMany transaction
        if (useTransaction) ExecuteNonQuery("BEGIN");
        // online status:
        //   '' if offline (if just logging out etc.)
        //   current time otherwise
        // -> this way it's fault tolerant because external applications can
        //    check if online != '' and if time difference < saveinterval
        // -> online time is useful for network zones (server<->server online
        //    checks), external websites which render dynamic maps, etc.
        // -> it uses the ISO 8601 standard format
        string onlineString = online ? DateTime.UtcNow.ToString("s") : "";
        string abilityString = player.abilities.CreateString();
        string attributeString = player.attributes.CreateString();
        string apperanceString = player.apperance.CreateString();
        string timeString = DateTime.UtcNow.ToString("s");

        ExecuteNonQuery("UPDATE characters" +
            " SET class=@class, displayName=@displayName, x=@x, y=@y, z=@z, health=@health, injury=@injury, mana=@mana, stamina=@stamina," +
            " skilltime=@skilltime, playtime=@playtime, online=@online," +
            " abilities=@abilities, attributes=@attributes, apperance=@apperance, gmstate=@gmstate, lastplayed=@lastplayed, faction=@faction" +
            " WHERE id=@id",
                        new SqliteParameter("@id", player.id),
                        new SqliteParameter("@class", player.className),
                        new SqliteParameter("@displayName", player.displayName),
                        new SqliteParameter("@x", player.transform.position.x),
                        new SqliteParameter("@y", player.transform.position.y),
                        new SqliteParameter("@z", player.transform.position.z),
                        new SqliteParameter("@health", player.health),
                        new SqliteParameter("@injury", player.injury),
                        new SqliteParameter("@mana", player.mana),
                        new SqliteParameter("@stamina", player.stamina),
                        new SqliteParameter("@skilltime", player.skillTotalTime),
                        new SqliteParameter("@playtime", player.playtime),
                        new SqliteParameter("@online", onlineString),
                        new SqliteParameter("@abilities", abilityString),
                        new SqliteParameter("@attributes", attributeString),
                        new SqliteParameter("@apperance", apperanceString),
                        new SqliteParameter("@gmstate", player.gmState),
                        new SqliteParameter("@lastplayed", timeString),
                        new SqliteParameter("@faction", player.faction));
        SaveInventory(player);
        SaveSkills(player);
        SaveSpells(player);
        SaveBuffs(player);
        SaveQuests(player);
        if (useTransaction) ExecuteNonQuery("END");
    }
    // save multiple characters at once (useful for ultra fast transactions)
    public static void CharacterSaveMany(List<Player> players, bool online = true)
    {
        ExecuteNonQuery("BEGIN"); // transaction for performance
        foreach (Player player in players)
            CharacterSave(player, online, false);
        ExecuteNonQuery("END");
    }
    // guilds //////////////////////////////////////////////////////////////////
    public static bool GuildExists(string guild)
    {
        return ((long)ExecuteScalar("SELECT Count(*) FROM guild_info WHERE name=@name", new SqliteParameter("@name", guild))) == 1;
    }
    public static void SaveGuild(string guild, string notice, List<GuildMember> members)
    {
        ExecuteNonQuery("BEGIN"); // transaction for performance
        // guild info
        ExecuteNonQuery("INSERT OR REPLACE INTO guild_info VALUES (@guild, @notice)",
                        new SqliteParameter("@guild", guild),
                        new SqliteParameter("@notice", notice));
        // members list
        ExecuteNonQuery("DELETE FROM character_guild WHERE guild=@guild", new SqliteParameter("@guild", guild));
        foreach (GuildMember member in members)
        {
            ExecuteNonQuery("INSERT INTO character_guild VALUES (@character, @guild, @rank)",
                            new SqliteParameter("@character", member.name),
                            new SqliteParameter("@guild", guild),
                            new SqliteParameter("@rank", member.rank));
        }
        ExecuteNonQuery("END");
    }
    public static void RemoveGuild(string guild)
    {
        ExecuteNonQuery("BEGIN"); // transaction for performance
        ExecuteNonQuery("DELETE FROM guild_info WHERE name=@name", new SqliteParameter("@name", guild));
        ExecuteNonQuery("DELETE FROM character_guild WHERE guild=@guild", new SqliteParameter("@guild", guild));
        ExecuteNonQuery("END");
    }

    // name and states ////////////////////////////////////////////////////////////
    public static void SaveNames(int charid, int namedCharid, string displayName, int state)
    {
        // names: remove old entries first, then add all new ones
        ExecuteNonQuery("DELETE FROM character_names WHERE charid=@charid AND namedcharid=@namedcharid",
                        new SqliteParameter("@charid", charid),
                        new SqliteParameter("@namedcharid", namedCharid));
        ExecuteNonQuery("INSERT INTO character_names VALUES (@charid, @namedcharid, @displayname, @state)",
                        new SqliteParameter("@charid", charid),
                        new SqliteParameter("@namedcharid", namedCharid),
                        new SqliteParameter("@displayname", displayName),
                        new SqliteParameter("@state", state));

    }

    // trade lists ////////////////////////////////////////////////////////////
    public static void FillItemList()
    {
        // load item list first
        List<List<object>> tradeItemList = ExecuteReader("SELECT name FROM  ang_item_info");
        List<string> itemList = new List<string>();
        foreach (List<object> row in tradeItemList)
        {
            string itemName = (string)row[0];
            itemList.Add(itemName);
        }

        // existing items
        foreach (var keyValuePair in ScriptableItem.dict)
        {
            UsableItem item = (UsableItem)keyValuePair.Value;
            // We don't sell and buy items that cannot be used and out int the inventory
            if (item.pickable)
            {
                if (itemList.Contains(item.name))
                {
                    // update base price
                    ExecuteNonQuery("UPDATE ang_item_info SET baseprice=@basePrice WHERE name=@itemName",
                    new SqliteParameter("@itemName", item.name),
                    new SqliteParameter("@basePrice", item.price));
                }
                else
                {
                    ExecuteNonQuery("INSERT INTO ang_item_info VALUES (@itemName, @basePrice)",
                    new SqliteParameter("@itemName", item.name),
                    new SqliteParameter("@basePrice", item.price));
                }
            }
        }

        // warning not more existing items
        int hasWarnings = 0;
        foreach (string itemName in itemList)
        {
            if (!ScriptableItem.dict.Any(tr => tr.Value.name.Equals(itemName)))
            {
                LogFile.WriteLog(LogFile.LogLevel.Warning, String.Format("NPC trading: Table ang_item_info contains unknown item: {0}", itemName));
                hasWarnings++;
            }
        }

        Debug.Log(string.Format("NPC merchants: {0} items updated ({1} warnings in log file)", itemList.Count, hasWarnings));
    }

    public static void FillNpcMerchants(List<Npc> npcMerchants)
    {
        // load npc list first
        List<List<object>> tmpDbList = ExecuteReader("SELECT name FROM  ang_npcmerchant_info");
        List<string> npcMerchantListDb = new List<string>();
        foreach (List<object> row in tmpDbList)
        {
            string npcName = (string)row[0];
            npcMerchantListDb.Add(npcName);
        }

        // existing merchants
        foreach (Npc npcMerchant in npcMerchants)
        {
            if (npcMerchantListDb.Contains(npcMerchant.name))
            {
                // update
                ExecuteNonQuery("UPDATE ang_npcmerchant_info SET buyallsellitems=@buyallsellitems WHERE name=@npcName",
                new SqliteParameter("@npcName", npcMerchant.name),
                new SqliteParameter("@buyallsellitems", Convert.ToInt32(npcMerchant.buyAllSellItems)));
            }
            else
            {
                ExecuteNonQuery("INSERT INTO ang_npcmerchant_info (name, buyallsellitems) VALUES (@npcName, @buyallsellitems)",
                new SqliteParameter("@npcName", npcMerchant.name),
                new SqliteParameter("@buyallsellitems", Convert.ToInt32(npcMerchant.buyAllSellItems)));
            }

        }

        // warning not more existing merchants
        int hasWarnings = 0;
        foreach (string npcName in npcMerchantListDb)
        {
            if (npcMerchants.Any(tr => tr.name.Equals(npcName)))
            {
                LogFile.WriteLog(LogFile.LogLevel.Warning, String.Format("NPC trading: Table ang_npcmerchant_info contains unknown merchant: {0}", npcName));
                hasWarnings++;
            }
        }

        Debug.Log(string.Format("NPC merchants: {0} NPC updated ({1} warnings in log file)", npcMerchants.Count, hasWarnings));
    }

    public static void ApplyNPCTrading(List<Npc> npcMerchants, bool testOnly = false)
    {
        int hasWarnings = 0;
        // load npc list
        List<List<object>> tmpMerchantList = ExecuteReader("SELECT name FROM  ang_npcmerchant_info");
        List<string> npcMerchantListDb = new List<string>();
        foreach (List<object> row in tmpMerchantList)
        {
            string npcName = (string)row[0];
            npcMerchantListDb.Add(npcName);
        }
        // load groups
        List<List<object>> tmpGroupList = ExecuteReader("SELECT id, name, npcmerchant, sort, isbuy, pricelevel FROM  ang_buysellgroup");
        List<NPCTradeGroup> npcGroupListDb = new List<NPCTradeGroup>();
        foreach (List<object> row in tmpGroupList)
        {
            NPCTradeGroup dbGroup = new NPCTradeGroup();
            dbGroup.id = Convert.ToInt32(row[0]);
            dbGroup.name = (string)row[1];
            dbGroup.npcmerchant = (string)row[2];
            dbGroup.sort = Convert.ToInt32(row[3]);
            dbGroup.isbuy = Convert.ToBoolean(row[4]);
            dbGroup.priceLevel = (float)Convert.ToDouble(row[5]);
            npcGroupListDb.Add(dbGroup);
        }
        // load items in groups
        List<List<object>> tmpBuySellItemList = ExecuteReader("SELECT id, buysellgroup, itemname, durability, quality, pricelevel FROM ang_buysellitems");
        List<NPCBuySellItem> npcBuySellItemListDb = new List<NPCBuySellItem>();
        foreach (List<object> row in tmpBuySellItemList)
        {
            NPCBuySellItem dbBuySellItem = new NPCBuySellItem();
            dbBuySellItem.id = Convert.ToInt32(row[0]);
            dbBuySellItem.buySellGroup = Convert.ToInt32(row[1]);
            dbBuySellItem.name = (string)row[2];
            dbBuySellItem.durability = Convert.ToInt32(row[3]);
            dbBuySellItem.quality = Convert.ToInt32(row[4]);
            dbBuySellItem.priceLevel = (float)Convert.ToDouble(row[5]);
            npcBuySellItemListDb.Add(dbBuySellItem);
        }


        // existing merchants
        foreach (Npc npcMerchant in npcMerchants)
        {
            if (npcMerchantListDb.Contains(npcMerchant.name))
            {
                if (!testOnly)
                {
#if UNITY_EDITOR

                    Undo.RecordObject(npcMerchant, "Update trading list");
#endif
                    npcMerchant.buyItems.Clear();
                    npcMerchant.sellItems.Clear();
                }
                foreach (NPCTradeGroup group in npcGroupListDb)
                {
                    if (group.npcmerchant.Equals(npcMerchant.name))
                    {
                        if (group.isbuy)
                        {
                            BuyItems buyItems = new BuyItems();
                            buyItems.headline = group.name;
                            buyItems.priceLevel = group.priceLevel;
                            buyItems.items = new List<ScriptableItem>();
                            foreach (NPCBuySellItem buySellItem in npcBuySellItemListDb.Where(x => x.buySellGroup == group.id))
                            {
                                ScriptableItem itemData;
                                if (ScriptableItem.dict.TryGetValue(buySellItem.name.GetStableHashCode(), out itemData))
                                {
                                    buyItems.items.Add(itemData);
                                }
                                else
                                {
                                    LogFile.WriteLog(LogFile.LogLevel.Warning, String.Format("NPC trading: Item {0} in {1}=>{2} not availabe in game", buySellItem.name, group.name, npcMerchant.name));
                                    hasWarnings++;
                                }
                            }
                            if (!testOnly)
                            {
                                npcMerchant.buyItems.Add(buyItems);
                            }
                        }
                        else
                        {
                            SellItems sellItems = new SellItems();
                            sellItems.headline = group.name;
                            sellItems.priceLevel = group.priceLevel;
                            sellItems.items = new List<SellItem>();
                            foreach (NPCBuySellItem buySellItem in npcBuySellItemListDb.Where(x => x.buySellGroup == group.id))
                            {
                                ScriptableItem itemData;
                                if (ScriptableItem.dict.TryGetValue(buySellItem.name.GetStableHashCode(), out itemData))
                                {
                                    SellItem sellItem = new SellItem();
                                    sellItem.quality = buySellItem.quality;
                                    sellItem.durability = buySellItem.durability;
                                    sellItem.priceLevel = buySellItem.priceLevel;
                                    sellItem.item = itemData;
                                    sellItems.items.Add(sellItem);
                                }
                                else
                                {
                                    LogFile.WriteLog(LogFile.LogLevel.Warning, String.Format("NPC trading: Item {0} in {1}=>{2} not availabe in game (ang_buysellitems id {3})", buySellItem.name, group.name, npcMerchant.name, buySellItem.id));
                                    hasWarnings++;
                                }
                            }
                            if (!testOnly)
                            {
                                npcMerchant.sellItems.Add(sellItems);
                            }
                        }
                    }
                }
            }
            else
            {
                LogFile.WriteLog(LogFile.LogLevel.Warning, String.Format("NPC trading: NPC Merchant not availabe in ang_npcmerchant_info: {0}", npcMerchant.name));
                hasWarnings++;
            }
        }
        // warning not more existing merchants
        foreach (string npcName in npcMerchantListDb)
        {
            if (!npcMerchants.Any(tr => tr.name.Equals(npcName)))
            {
                int iGroups = npcGroupListDb.Count(x => x.npcmerchant.Equals(npcName));
                LogFile.WriteLog(LogFile.LogLevel.Warning, String.Format("NPC trading: Table ang_npcmerchant_info contains unknown merchant: {0}", npcName));
                if (iGroups > 0)
                {
                    LogFile.WriteLog(LogFile.LogLevel.Warning, String.Format("NPC trading: Unknown merchant {0} has {1} assigend groups", npcName, iGroups));
                }
                hasWarnings++;
            }
        }

        // find obsolete items in data base
        List<List<object>> tradeItemList = ExecuteReader("SELECT name FROM  ang_item_info");
        foreach (List<object> row in tradeItemList)
        {
            string itemName = (string)row[0];
            if (!ScriptableItem.dict.Any(tr => tr.Value.name.Equals(itemName)))
            {
                LogFile.WriteLog(LogFile.LogLevel.Warning, String.Format("NPC trading: Table ang_item_info contains unknown item: {0}", itemName));
                hasWarnings++;
            }
        }

        // find orphaned groups
        foreach (NPCTradeGroup group in npcGroupListDb)
        {
            if (!npcMerchants.Any(tr => tr.name.Equals(group.npcmerchant)))
            {
                LogFile.WriteLog(LogFile.LogLevel.Warning, String.Format("NPC trading: Orphaned group {0} (id:{1}) in ang_buysellgroup refers to unknown merchant {2}", group.name, group.id, group.npcmerchant));
                hasWarnings++;
            }
        }

        //find orphaned BuySellItems
        foreach (NPCBuySellItem buySellItem in npcBuySellItemListDb)
        {
            if (!npcGroupListDb.Any(x => x.id == buySellItem.buySellGroup))
            {
                LogFile.WriteLog(LogFile.LogLevel.Warning, String.Format("NPC trading: Orphaned listed item {0} (id:{1}) in ang_buysellitems refers to unknown group ID {2}", buySellItem.name, buySellItem.id, buySellItem.buySellGroup));
                hasWarnings++;
            }
        }

        if (testOnly)
        {
            Debug.Log(string.Format("NPC Trading data verified: {0} warnings in log file", hasWarnings));
        }
        else
        {
            Debug.Log(string.Format("NPC Trading data base applied to game: {0} warnings in log file", hasWarnings));
        }
    }

    private struct NPCTradeGroup
    {
        public int id;
        public string name;
        public string npcmerchant;
        public int sort;
        public bool isbuy;
        public float priceLevel;
    }
    private struct NPCBuySellItem
    {
        public int id;
        public int buySellGroup;
        public string name;
        public int durability;
        public int quality;
        public float priceLevel;
    }
}