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

        public Program()
        {
            // The constructor, called only once every session and
            // always before any other method is called. Use it to
            // initialize your script. 
            //     
            // The constructor is optional and can be removed if not
            // needed.
            // 
            // It's recommended to set Runtime.UpdateFrequency 
            // here, which will allow your script to run itself without a 
            // timer block.
        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        List<IMyTerminalBlock> allBlocks = new List<IMyTerminalBlock>();
        public void Main(string argument, UpdateType updateSource)
        {
            Echo("This is working!");
            string Prefix = "(HRP) ";


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
            }
        }
    }
}
