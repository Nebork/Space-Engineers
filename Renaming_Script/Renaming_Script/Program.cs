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
// using System.Text.RegularExpressions;  // can be implemented, but the whole namespace is needed
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

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
        readonly static List<string> blockGroupNames = new List<string>();

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
            public BlockGroup(IMyTerminalBlock firstBlock, string groupname = "")
            {
                this.GroupName = groupname;
                blockGroups.Add(this);
                blockGroupNames.Add(groupname);

                groupMembers = new List<IMyTerminalBlock>() { firstBlock };  // Initialises the list and puts in first block.
            }


            // Main function. Renames every block in the group with the given settings.
            public int Rename()
            {
                if(this.Process() == -1) { return -1; }  // returns -1 if process fails
                if(this.Command.Contains("-S")) { return 0; }  // if skip is used

                for(int i = 0; i < groupMembers.Count; i++)
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
                // Checks if input is not valid, good luck with Regex :D
                if(this.Command != "" && !System.Text.RegularExpressions.Regex.IsMatch(this.Command, "(-[NTIHBS]|(-R\"[a-zA-Z0-9]+\"))+"))
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
                        this._replacementName = commands[i].Substring(2, commands[i].Length - 3);  // everything but the first 2 and the last char
                    }
                }
                return 0;
            }
        }

        // Creates the blockGroups of the current state  TODO is dependent on workOnSubgrids. Just add parameter and filter allBlocks dependently
        public void CreateBlockGroups()
        {
            // Clears both list of any old, unused members
            blockGroups.Clear();
            blockGroupNames.Clear();

            // Get all blocks and prepare for assignment to groups.
            List<IMyTerminalBlock> allBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocks(allBlocks);

            // Loop to assign every block to a group or create a new one.
            foreach (IMyTerminalBlock terminalBlock in allBlocks)
            {
                string easyBlockType = terminalBlock.GetType().ToString().Split('.').Last().Replace("My", "");  // Regex looks easier

                int i = blockGroupNames.IndexOf(easyBlockType);
                if (i == -1)  // no group with this name found
                {
                    new BlockGroup(terminalBlock, easyBlockType);
                }
                else  // there already is a group with this name, add the block
                {
                    blockGroups[i].groupMembers.Add(terminalBlock);
                }
            }
        }


        // Generates the Custom Data based on the blockGroups
        public void CreateCustomData()
        {
            // For just adding the block groups to the current custom data
            string addition = "\n";

            // TODO add the check, that only NEW groups are added
            foreach (BlockGroup blockGroup in blockGroups)
            {
                addition += $"{blockGroup.GroupName} = \n";
            }

            Me.CustomData += addition;
        }


        // Runs at the start of the game or every time it is recompiled (edit code or press recompile)
        // Shall check and generate the custom data
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.None;
            CreateBlockGroups();
            CreateCustomData();
        }


        // Runs every time someone presses run. Shall fetch all the blocks and put them into the groups
        // processes the command string and uses it to rename all the blocks of a group
        // TODO maybe use update source to differentiate from run from program (fresh start) or run from command
        public void Main(/*string argument, UpdateType updateSource*/)
        {
            CreateBlockGroups();

            MyIni _ini = new MyIni();  // TODO _ini needed global?

            // Try to parse the ini
            MyIniParseResult result;
            if (!_ini.TryParse(Me.CustomData, out result))
            {
                Echo("Could not parse the Custom Data, syntax error!");
                throw new Exception(result.ToString());
            }

            // Read all global settings
            _prefix = _ini.Get("Nebork's Renaming Script", "Prefix").ToString();
            _postfix = _ini.Get("Nebork's Renaming Script", "Postfix").ToString();

            _leadingZeros = _ini.Get("Nebork's Renaming Script", "LeadingZeros").ToBoolean();

            // Loads the input for every block group
            foreach (BlockGroup blockGroup in blockGroups)
            {
                blockGroup.Command = _ini.Get("Nebork's Renaming Script", blockGroup.GroupName).ToString();
            }


            // Main loop, iterates every block group and renames it according to their settings.
            foreach (BlockGroup blockGroup in blockGroups)
            {
                if(blockGroup.Rename() == -1)
                {
                    Echo($"An error occured in group {blockGroup.GroupName} with its command {blockGroup.Command}!\n");
                    break;
                }
            }
        }

        // COPY UNTIL HERE
    }
}
