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

        // COPY FROM HERE

        Color color = new Color();
        Random rnd = new Random();
        IMyLightingBlock light;
        string name;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            name = "[CRG] Rotating Light - Party Nebork";
            light = GridTerminalSystem.GetBlockWithName(name) as IMyLightingBlock;

            if (light == null)
            {
                Echo($"Could not find light with name {name}");
                Runtime.UpdateFrequency = UpdateFrequency.None;
                return;
            }

            color.R = 255;
            color.G = 0;
            color.B = 255;
            light.Color = color;
        }

        public Color NextColor(Color startColor)
        {
            const int step = 1; // 15 or 17 for UpdateFrequency.Update10

            if (startColor.R > 0 && startColor.G == 0 && startColor.B == 255)
            {
                startColor.R -= step;
                return startColor;
            }
            else if (startColor.R == 0 && startColor.G < 255 && startColor.B == 255)
            {
                startColor.G += step;
                return startColor;
            }
            else if (startColor.R == 0 && startColor.G == 255 && startColor.B > 0)
            {
                startColor.B -= step;
                return startColor;
            }
            else if (startColor.R < 255 && startColor.G == 255 && startColor.B == 0)
            {
                startColor.R += step;
                return startColor;
            }
            else if (startColor.R == 255 && startColor.G > 0 && startColor.B == 0)
            {
                startColor.G -= step;
                return startColor;
            }
            else if (startColor.R == 255 && startColor.G == 0 && startColor.B < 255)
            {
                startColor.B += step;
                return startColor;
            }
            else return startColor;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // The main entry point of the script, invoked every time
            // one of the programmable block's Run actions are invoked,
            // or the script updates itself. The updateSource argument
            // describes where the update came from. Be aware that the
            // updateSource is a  bitfield  and might contain more than 
            // one update type.
            // 
            // The method itself is required, but the arguments above
            // can be removed if not needed.

            // For random
            // color.R = (byte)rnd.Next(256);
            // color.G = (byte)rnd.Next(256);
            // color.B = (byte)rnd.Next(256);

            color = light.Color;
            color = NextColor(color);
            light.Color = color;
        }
        // COPY UNTIL HERE
    }
}
