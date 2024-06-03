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
        // COPY FROM HERE

        Random rnd = new Random();
        string groupName;
        IMyBlockGroup partyGroup;
        List<IMyLightingBlock> partyLights;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;


            // Takes all the lights from the group and puts them into the list "partyLights"
            groupName = "[CRG] - Party";
            partyGroup = GridTerminalSystem.GetBlockGroupWithName(groupName);
            partyLights = new List<IMyLightingBlock>();
            partyGroup.GetBlocksOfType<IMyLightingBlock>(partyLights);

            foreach (var partyLight in partyLights)
            {
                partyLight.Color = RandomValidColor();
            }


            // CHANGE TO EMPTY GROUP AND NO LIGHTS IN GROUP
            /*if (light == null)
            {
                Echo($"Could not find light with name {name}");
                Runtime.UpdateFrequency = UpdateFrequency.None;
                return;
            }*/
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

        public Color RandomValidColor()
        {
            int randomNmbr = rnd.Next(6);

            byte x = (byte) rnd.Next(256);

            Color rndColor = new Color();

            switch(randomNmbr)
            {
                case 0:
                    rndColor.R = x;
                    rndColor.G = 0;
                    rndColor.B = 255;
                    break;
                case 1:
                    rndColor.R = 0;
                    rndColor.G = x; ;
                    rndColor.B = 255;
                    break;
                case 2:
                    rndColor.R = 0;
                    rndColor.G = 255;
                    rndColor.B = x; ;
                    break;
                case 3:
                    rndColor.R = x; ;
                    rndColor.G = 255;
                    rndColor.B = 0;
                    break;
                case 4:
                    rndColor.R = 255;
                    rndColor.G = x;
                    rndColor.B = 0;
                    break;
                case 5:
                    rndColor.R = 255;
                    rndColor.G = 0;
                    rndColor.B = x;
                    break;
            }

            return rndColor;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            foreach (var partyLight in partyLights)
            {
                partyLight.Color = NextColor(partyLight.Color);
            }
        }

        // COPY UNTIL HERE
    }
}
