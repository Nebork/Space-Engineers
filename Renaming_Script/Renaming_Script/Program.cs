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

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // In order to add a new utility class, right-click on your project, 
        // select 'New' then 'Add Item...'. Now find the 'Space Engineers'
        // category under 'Visual C# Items' on the left hand side, and select
        // 'Utility Class' in the main area. Name it in the box below, and
        // press OK. This utility class will be merged in with your code when
        // deploying your final script.
        //
        // You can also simply create a new utility class manually, you don't
        // have to use the template if you don't want to. Just do so the first
        // time to see what a utility class looks like.
        // 
        // Go to:
        // https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts
        //
        // to learn more about ingame scripts.

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
