using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;
using static IngameScript.Program;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // COPY FROM HERE

        /*
        Nebork's Renaming Script

        This script is used to rename every block uniformly in the control panel.
        This is extremely useful, if you care about proper naming and organizing blocks WITHOUT having to rename 100+ blocks manually.
        */


        // Any changes made below the following line are made on your own risk!
        // --------------------------------------------------------------------------------------------------------


        // Global Setting Variables

        static string _prefix;
        static string _postfix;

        static bool _leadingZeros;


        // Used to go over every group
        readonly static List<BlockGroup> blockGroups = new List<BlockGroup>();

        // Stores all given data and handles renaming
        public class BlockGroup
        {
            public string GroupName { get; set; } = string.Empty;  // Used in the custom data and to find the right group
            public string Command { get; set; } = string.Empty;

            public List<IMyTerminalBlock> groupMembers;


            // Settings processed from Command, see Process()
            private bool _numbering = false;  // -N
            private bool _showInTerminal = false;  // -T
            private bool _showInInventory = false;  // -I
            private bool _showOnHud = false;  // -H
            private bool _showInToolbarConfig = false;  // -B

            private bool _rename = false;  // -R "x"
            private string _replacementName;  // "x"

            // Constructor, adding it to the global variables
            public BlockGroup(string groupname = "")
            {
                this.GroupName = groupname;
                blockGroups.Add(this);

                groupMembers = new List<IMyTerminalBlock>() { };  // Initialises the list and puts in first block.
            }


            // Main function. Renames every block in the group with the given settings.
            public int Rename()
            {
                if (this.Command.Contains("-S")) { return 0; }  // If skip is used
                if (this.Process() == -1) { return -1; }  // Returns -1 if process fails

                for (int i = 0; i < groupMembers.Count; i++)
                {
                    // Renaming
                    string futureName = "";

                    // creating futureName
                    futureName += $"{_prefix}";
                    if (_rename) { futureName += $" {_replacementName}"; }
                    else { futureName += $" {groupMembers[i].DefinitionDisplayNameText}"; }
                    if (_numbering)
                    {
                        if (!_leadingZeros) { futureName += $" {i + 1}"; }
                        else
                        {
                            int numberOfZeros = (int)Math.Log10(groupMembers.Count) - (int)Math.Log10(i + 1);
                            futureName += $" {String.Concat(Enumerable.Repeat("0", numberOfZeros))}{i + 1}";
                        }
                    }
                    futureName += $" {_postfix}";

                    groupMembers[i].CustomName = futureName;


                    // Settings
                    groupMembers[i].ShowInInventory = this._showInInventory;
                    groupMembers[i].ShowOnHUD = this._showOnHud;
                    groupMembers[i].ShowInTerminal = this._showInTerminal;
                    groupMembers[i].ShowInToolbarConfig = this._showInToolbarConfig;
                }
                return 0;
            }


            private int Process()
            {
                this.Command = this.Command.Replace(" ", "");
                // Checks if input is not valid, good luck with the Regex :D
                if (this.Command != "" && !System.Text.RegularExpressions.Regex.IsMatch(this.Command, "(-[NTIHBS]|(-R\"[a-zA-Z0-9]+\"))+"))
                {
                    return -1;
                }

                string[] commands = this.Command.Split('-');

                for (int i = 0; i < commands.Length; i++)
                {
                    if (commands[i] == "N") this._numbering = true;
                    else if (commands[i] == "I") this._showInInventory = true;
                    else if (commands[i] == "H") this._showOnHud = true;
                    else if (commands[i] == "T") this._showInTerminal = true;
                    else if (commands[i] == "B") this._showInToolbarConfig = true;
                    else if (System.Text.RegularExpressions.Regex.IsMatch(commands[i], "R\"[a-zA-Z0-9]+\""))
                    {
                        this._rename = true;
                        this._replacementName = commands[i].Substring(2, commands[i].Length - 3);  // Everything but the first 2 and the last char
                    }
                }
                return 0;
            }
        }

        // Creates the blockGroups of the current state  TODO is dependent on workOnSubgrids. Just add parameter and filter allBlocks dependently
        public void CreateBlockGroups()
        {

            // Get all blocks and prepare for assignment to groups.
            List<IMyTerminalBlock> allBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocks(allBlocks);

            // Loop to assign every block to a group or create a new one.
            foreach (IMyTerminalBlock terminalBlock in allBlocks)
            {
                string easyBlockType = terminalBlock.GetType().ToString().Split('.').Last().Replace("My", "");  // Regex looks easier

                int index = -1;

                for (int i = 0; i < blockGroups.Count; i++)
                {
                    if (easyBlockType == blockGroups[i].GroupName)
                    {
                        index = i; break;
                    }
                }

                if (index == -1)  // No group with this name found
                {
                    new BlockGroup(easyBlockType);
                    blockGroups.Last().groupMembers.Add(terminalBlock);
                }
                else  // There already is a group with this name, just add the block
                {
                    blockGroups[index].groupMembers.Add(terminalBlock);
                }

                blockGroups.Sort((x, y) => x.GroupName.CompareTo(y.GroupName));  // Sort the list
            }
        }


        // Generates the Custom Data based on the current blockGroups
        public void AddBlockGroupsToCd()
        {
            // For just adding the block groups to the current custom data
            string addition = "\n";

            foreach (BlockGroup blockGroup in blockGroups)
            {
                addition += $"\n{blockGroup.GroupName} = {blockGroup.Command}";
            }

            addition += "\n";

            Me.CustomData += addition;
        }

        // Loads the given block group settings and removes them from the custom data.
        public void LoadBlockGroups()
        {
            string cd = Me.CustomData;
            string[] cdLines = cd.Split('\n');

            bool reachedBlockSettings = false;
            bool changed = false;

            for (int i = 0; i < cdLines.Length; i++)
            {
                if (cdLines[i] == "[Block Settings]")
                {
                    reachedBlockSettings = true;
                }
                else if (reachedBlockSettings && !cdLines[i].StartsWith(";") && cdLines[i] != "")
                {
                    if (changed == false)
                    {
                        changed = true;
                        Me.CustomData = string.Join("\n", cdLines.Take(i - 1));  // Only the first i-1 elements, so without settings
                    }
                    string[] words = cdLines[i].Replace(" ", "").Split(new char[] { '=' });
                    new BlockGroup(words[0]) { Command = words[1] };
                }
            }
        }


        // Runs at the start of the game or every time it is recompiled (edit code or press recompile)
        // Shall check and generate the custom data
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.None;

            // Clears list of any old, unused members
            blockGroups.Clear();

            LoadBlockGroups();
            CreateBlockGroups();
            AddBlockGroupsToCd();
        }


        // Runs every time someone presses run. Shall fetch all the blocks and put them into the groups
        // Processes the command string and uses it to rename all the blocks of a group
        // TODO maybe use update source to differentiate from run from program (fresh start) or run from command
        public void Main(/*string argument, UpdateType updateSource*/)
        {
            // Clears list of any old, unused members
            blockGroups.Clear();

            CreateBlockGroups();

            MyIni _ini = new MyIni();

            // Try to parse the ini
            MyIniParseResult result;
            if (!_ini.TryParse(Me.CustomData, out result))
            {
                Echo("Could not parse the Custom Data, syntax error!");
                throw new Exception(result.ToString());
            }

            // Read all global settings
            _prefix = _ini.Get("Global Settings", "Prefix").ToString();
            _postfix = _ini.Get("Global Settings", "Postfix").ToString();

            _leadingZeros = _ini.Get("Global Settings", "LeadingZeros").ToBoolean();

            // Loads the input for every block group
            foreach (BlockGroup blockGroup in blockGroups)
            {
                blockGroup.Command = _ini.Get("Block Settings", blockGroup.GroupName).ToString();
            }


            // Main loop, iterates every block group and renames it according to their settings.
            foreach (BlockGroup blockGroup in blockGroups)
            {
                if (blockGroup.Rename() == -1)
                {
                    Echo($"An error occured in group {blockGroup.GroupName} with its command {blockGroup.Command}!\n");
                    break;
                }
            }
        }

        // COPY UNTIL HERE
    }
}
