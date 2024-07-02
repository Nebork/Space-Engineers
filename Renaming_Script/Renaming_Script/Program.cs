﻿using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
// #01
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
        // #02
        // Any changes made below the following line are made on your own risk!
        // Global Setting Variables
        static string _prefix;
        static string _postfix;

        static bool _leadingZeros;


        // #03
        readonly static List<BlockGroup> blockGroups = new List<BlockGroup>();
        readonly static List<string> blockGroupNames = new List<string>();

        // #04
        public class BlockGroup
        {
            public string GroupName { get; set; } = string.Empty;  // #05
            public string Command { get; set; } = string.Empty;

            // TODO #06
            public List<IMyTerminalBlock> groupMembers;


            // #07
            private bool _numbering;  // -N
            private bool _rename;  // -R "x"
            private string _replacementName;  // "x"
            private bool _showInTerminal;  // -T
            private bool _showInInventory;  // -I
            private bool _showOnHud;  // -H
            private bool _useConveyor;  // -C


            // #08
            public BlockGroup(IMyTerminalBlock firstBlock, string groupname = "")
            {
                this.GroupName = groupname;
                blockGroups.Add(this);
                blockGroupNames.Add(groupname);

                groupMembers = new List<IMyTerminalBlock>() { firstBlock };  // #09
            }


            // #10
            public int Rename()
            {
                if(this.Process() == -1) { return -1; }
                for(int i = 0; i < groupMembers.Count; i++)
                {
                    string futureName = "";

                    // #11
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
                }
                return 0;
            }

            // #12
            private int Process()
            {
                this.Command = this.Command.Replace(" ", "");
                // #13
                if(!System.Text.RegularExpressions.Regex.IsMatch(this.Command, "(-[HTICNR]|(-R\"[a-zA-Z0-9]+\"))*"))
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
                    else if (commands[i] == "C") this._useConveyor = true;
                    else if (System.Text.RegularExpressions.Regex.IsMatch(commands[i], "R\"[a-zA-Z0-9]+\""))
                    {
                        this._rename = true;
                        this._replacementName = commands[i].Substring(2, commands[i].Length - 3);  // #14
                    }
                }
                return 0;
            }
        }

        // #15
        public void CreateBlockGroups()
        {
            // #16
            blockGroups.Clear();
            blockGroupNames.Clear();

            // #17
            List<IMyTerminalBlock> allBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocks(allBlocks);

            // #18
            foreach (IMyTerminalBlock terminalBlock in allBlocks)
            {
                string easyBlockType = terminalBlock.GetType().ToString().Split('.').Last().Replace("My", "");  // Regex looks easier

                int i = blockGroupNames.IndexOf(easyBlockType);
                if (i == -1)  // #19
                {
                    new BlockGroup(terminalBlock, easyBlockType);
                }
                else  // #20
                {
                    blockGroups[i].groupMembers.Add(terminalBlock);
                }
            }
        }


        // #21
        public void CreateCustomData()
        {
            // #22
            string addition = "";

            // TODO #23
            foreach (BlockGroup blockGroup in blockGroups)
            {
                addition += $"{blockGroup.GroupName} = \n";
            }

            Me.CustomData += addition;
        }


        // #24
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.None;
            CreateBlockGroups();
            CreateCustomData();
        }


        // #25
        // TODO #26
        public void Main(/*string argument, UpdateType updateSource*/)
        {
            string debug = "";

            CreateBlockGroups();


            MyIni _ini = new MyIni();  // TODO #27

            // #28
            MyIniParseResult result;
            if (!_ini.TryParse(Me.CustomData, out result))
            {
                Echo("Could not parse the Custom Data, syntax error!");
                throw new Exception(result.ToString());
            }

            // #29
            _prefix = _ini.Get("Nebork's Renaming Script", "Prefix").ToString();
            _postfix = _ini.Get("Nebork's Renaming Script", "Postfix").ToString();

            _leadingZeros = _ini.Get("Nebork's Renaming Script", "LeadingZeros").ToBoolean();

            // #30
            foreach (BlockGroup blockGroup in blockGroups)
            {
                blockGroup.Command = _ini.Get("Nebork's Renaming Script", blockGroup.GroupName).ToString();
            }


            // #31
            foreach (BlockGroup blockGroup in blockGroups)
            {
                if(blockGroup.Rename() == -1)
                {
                    Echo($"An error occured in group {blockGroup.GroupName} with its command {blockGroup.Command}!\n");
                    break;
                }
            }


            Echo(debug);
        }

        // COPY UNTIL HERE



        /* MyIni _ini = new MyIni();

        // Define the variables used for the custom data.
        string _gridName;
        string _prefix;
        string _postfix;
        bool _workOnSubgrids;
        bool _removeDlcNaming;
        bool _leadingZeros;
        bool _romanNumerals;

        string _refineries;
        string _assembler;
        string _h2_O2_Generator;

        string _misc;

        // "stolen"/found, try to understand what's happening here. From the DC, is in MMI.
        bool AddIfType<T>(IMyTerminalBlock block, List<T> list)
        where T : class, IMyTerminalBlock
        {
            var cast = block as T;
            bool isType = cast != null;
            if (isType)
            {
                list.Add(cast);
            }
            return isType;
        }

        // Converts an int to a roman number as string
        public static string IntToRoman(int number)
        {
            string output = "";
            int[] numberLimits = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
            string[] romanLimits = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };

            for (int i = 0; i < numberLimits.Length; i++)
            {
                while (number >= numberLimits[i])
                {
                    number -= numberLimits[i];
                    output += romanLimits[i];
                }
            }
            return output;
        }

        public static string AddLeadingZeroes(int number, int maxNumber)
        {
            int numberOfZeros = (int)Math.Log10(maxNumber) - (int)Math.Log10(number);
            return String.Concat(Enumerable.Repeat("0", numberOfZeros)) + number;
        }

        public class BlockGroup
        {
            public string Name;
            public string Operations;

            public List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();

            
            public BlockGroup(string groupName, string wantedOperations, List<IMyTerminalBlock> containedBlocks) : this(groupName, wantedOperations)
            {
                Blocks = containedBlocks;
            }
            public BlockGroup(string groupName, string wantedOperations)
            {
                Name = groupName;
                Operations = wantedOperations;
            }

            public void Rename(bool removeDlcNaming, bool leadingZeros, bool romanNumerals, string prefix, string postfix)
            {
                for(int i = 0; i < Blocks.Count; i++)
                {
                    IMyTerminalBlock block = Blocks[i];
                    string targetName = block.DefinitionDisplayNameText;

                    if (removeDlcNaming)
                    {
                        targetName = targetName.Replace("Industrial ", "");
                        targetName = targetName.Replace("Sci-Fi ", "");
                        targetName = targetName.Replace("Warfare ", "");
                    }

                    if (Operations.Contains("N"))
                    {
                        targetName += AddLeadingZeroes(i + 1, Blocks.Count);
                    }

                    if (prefix != "")
                        targetName = prefix + " " + targetName;
                    if (postfix != "")
                        targetName = targetName + " " + postfix;

                    block.CustomName = targetName;
                }
            }
        }

        public void Rename<T>(List<T> blocks, string operations)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                IMyTerminalBlock currentBlock = (IMyTerminalBlock) blocks[i];
                string targetName = currentBlock.DefinitionDisplayNameText;



                if (_removeDlcNaming)
                {
                    targetName = targetName.Replace("Industrial ", "");
                    targetName = targetName.Replace("Sci-Fi ", "");
                    targetName = targetName.Replace("Warfare ", "");
                }

                if (operations.Contains("N"))
                {
                    targetName = AddNumbering(targetName, i + 1, blocks.Count);
                }

                if (_prefix != "")
                    targetName = _prefix + " " + targetName;
                if (_postfix != "")
                    targetName = targetName + " " + _postfix;

                currentBlock.CustomName = targetName;
            }
        }

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.None;

            // Call the TryParse method on the custom data. This method will
            // return false if the source wasn't compatible with the parser.
            MyIniParseResult result;
            if (!_ini.TryParse(Me.CustomData, out result))
            {
                throw new Exception(result.ToString());
            }

            // Get all variables from the ini/custom data
            _gridName = _ini.Get("Nebork's Renaming Script", "GridName").ToString();
            _workOnSubgrids = _ini.Get("Nebork's Renaming Script", "WorkOnSubgrids").ToBoolean();
            _prefix = _ini.Get("Nebork's Renaming Script", "Prefix").ToString();
            _postfix = _ini.Get("Nebork's Renaming Script", "Postfix").ToString();
            _removeDlcNaming = _ini.Get("Nebork's Renaming Script", "RemoveDlcNaming").ToBoolean();
            _leadingZeros = _ini.Get("Nebork's Renaming Script", "LeadingZeros").ToBoolean();
            _romanNumerals = _ini.Get("Nebork's Renaming Script", "RomanNumerals").ToBoolean();

            _refineries = _ini.Get("Nebork's Renaming Script", "Refineries").ToString();
            _assembler = _ini.Get("Nebork's Renaming Script", "Assembler").ToString();
            _h2_O2_Generator = _ini.Get("Nebork's Renaming Script", "H2/O2 Generator").ToString();

            _misc = _ini.Get("Nebork's Renaming Script", "Misc").ToString();
            // TODO ADD ALL

        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (_gridName == "")
            {
                _gridName = Me.CubeGrid.CustomName;
            }

            List<IMyTerminalBlock> allBlocks = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> allGridBlocks = new List<IMyTerminalBlock>();

            List<IMyRefinery> refineries = new List<IMyRefinery>();
            List<IMyAssembler> assembler = new List<IMyAssembler>();
            List<IMyGasGenerator> h2_O2_Generator = new List<IMyGasGenerator>();

            List<IMyTerminalBlock> misc = new List<IMyTerminalBlock>();


            GridTerminalSystem.GetBlocks(allBlocks);
            foreach (IMyTerminalBlock block in allBlocks)
            {
                if (_workOnSubgrids || block.CubeGrid.CustomName == _gridName)  // Only adds blocks of the wanted grids
                {
                    if (
                    AddIfType(block, refineries)
                    || AddIfType(block, assembler)
                    || AddIfType(block, h2_O2_Generator))
                    {
                        // IS already added by the AddIfType,
                        allGridBlocks.Add(block);
                    }
                    else
                    {
                        misc.Add(block);
                        allGridBlocks.Add(block);
                    }
                }
            }

            if (allGridBlocks.Count == 0)
            {
                // TODO add a warning, "There are no blocks on the given grid", add gridname.
                return;
            }

            // Rename(refineries, _refineries);
            // Rename(assembler, _assembler);
            // Rename(h2_O2_Generator, _h2_O2_Generator);

            // Rename(misc, _misc);
        }*/
    }
}
