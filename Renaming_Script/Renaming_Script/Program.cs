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

        MyIni _ini = new MyIni();

        string _prefix;
        string _postfix;
        bool _workOnSubgrids;  // TODO ADD
        bool _removeDlcNaming;

        string _refineries;
        string _assembler;
        string _h2_O2_Generator;

        List<IMyRefinery> refineries = new List<IMyRefinery>();
        List<IMyAssembler> assembler = new List<IMyAssembler>();
        List<IMyGasGenerator> h2_O2_Generator = new List<IMyGasGenerator>();

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.None;

            // Call the TryParse method on the custom data. This method will
            // return false if the source wasn't compatible with the parser.
            MyIniParseResult result;
            if (!_ini.TryParse(Me.CustomData, out result))
                throw new Exception(result.ToString());

            // Get all variables from the ini/custom data
            _prefix = _ini.Get("Nebork's Renaming Script", "Prefix").ToString();
            _postfix = _ini.Get("Nebork's Renaming Script", "Postfix").ToString();
            _workOnSubgrids = _ini.Get("Nebork's Renaming Script", "WorkOnSubgrids").ToBoolean();
            _removeDlcNaming = _ini.Get("Nebork's Renaming Script", "RemoveDlcNaming").ToBoolean();

            _refineries = _ini.Get("Nebork's Renaming Script", "Refineries").ToString();
            _assembler = _ini.Get("Nebork's Renaming Script", "Assembler").ToString();
            _h2_O2_Generator = _ini.Get("Nebork's Renaming Script", "H2/O2 Generator").ToString();

        }

        List<IMyTerminalBlock> allBlocks = new List<IMyTerminalBlock>();
        public void Main(string argument, UpdateType updateSource)
        {

            GridTerminalSystem.GetBlocks(allBlocks);
            for (int i = 0; i < allBlocks.Count; i++)
            {
                IMyTerminalBlock CurrentBlock = allBlocks[i];
                string targetName = CurrentBlock.DefinitionDisplayNameText;

                if(_removeDlcNaming)
                {
                    targetName = targetName.Replace("Industrial ", "");
                    targetName = targetName.Replace("Sci-Fi ", "");
                    targetName = targetName.Replace("Warfare ", "");
                }

                if(_prefix != "")
                    targetName = _prefix + " " + targetName;
                if (_postfix != "")
                    targetName = targetName + " " + _postfix;

                CurrentBlock.CustomName = targetName;
            }

            GridTerminalSystem.GetBlocksOfType<IMyRefinery>(refineries);
            foreach (IMyRefinery refinery in refineries)
            {
                Echo(refinery.CustomName);
            }

            /* OLD (till 12.2.24) CODE
            
            string Prefix = "(HRP)";

            GridTerminalSystem.GetBlocks(allBlocks);
            for (int i = 0; i < allBlocks.Count; i++)
            {
                IMyTerminalBlock CurrentBlock = allBlocks[i];
                string CurrentBlockName = CurrentBlock.DefinitionDisplayNameText;

                if (CurrentBlockName.Contains("Industrial"))
                {
                    CurrentBlock.CustomName = Prefix + CurrentBlockName.Replace("Industrial ", "");
                }
                else if (CurrentBlockName.Contains("Sci-Fi"))
                {
                    CurrentBlock.CustomName = Prefix + CurrentBlockName.Replace("Sci-Fi ", "");
                }
                else if (CurrentBlockName.Contains("Warfare"))
                {
                    CurrentBlock.CustomName = Prefix + CurrentBlockName.Replace("Warfare ", "");
                }
                else if (CurrentBlockName.Contains("Wheel Suspension"))
                {
                    // SPECIAL CASE!!! Keeps the last name part "Right" and "Left"
                    string[] parts = CurrentBlockName.Split(' ');
                    CurrentBlock.CustomName = Prefix + "Wheel - " + parts[parts.Length - 1];
                }
                else
                {
                    CurrentBlock.CustomName = Prefix + CurrentBlockName;
                }

                if (CurrentBlockName.Contains("Large"))
                {
                    CurrentBlock.CustomName = CurrentBlock.CustomName.Replace("Large ", "") + " (Large)";
                }
                else if (CurrentBlockName.Contains("Small"))
                {
                    CurrentBlock.CustomName = CurrentBlock.CustomName.Replace("Small ", "") + " (Small)";
                }
            }*/

        }
    }
}
