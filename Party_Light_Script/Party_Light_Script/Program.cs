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


            // Gets a list of the blocks in the group groupName
            groupName = "[CRG] - Party";
            partyGroup = GridTerminalSystem.GetBlockGroupWithName(groupName);

            // Checks if the group is empty, thus no group has been found (empty list if no blocks in group). Stops whole script
            if (partyGroup == null)
            {
                Echo($"Could not find a group with name \"{groupName}\"!");
                Runtime.UpdateFrequency = UpdateFrequency.None;
                return;
            }

            // Fetches all the blocks in partyGroup from class IMyLightingBlock into partyLights  TODO CHECK
            partyLights = new List<IMyLightingBlock>();
            partyGroup.GetBlocksOfType<IMyLightingBlock>(partyLights);

            // Checks if there are any blocks in partyLights, else stop the whole script  TODO CHECK
            if (partyLights == null)
            {
                Echo($"Could not find any lights in the \"{groupName}\" group!");
                Runtime.UpdateFrequency = UpdateFrequency.None;
                return;
            }

            // Give every light a random and valid color. (one RGB value is 0, another one 255, and the last is random from 0-255)
            foreach (var partyLight in partyLights)
            {
                partyLight.Color = RandomValidColor();
            }
        }


        // Returns a color, which is close to the given startColor. This is to ensure a smooth transition between the colors.
        // The color choosen is based on an euler-circle along the edges of a reduced RGB cube.
        // This reduction removed all edges connecting to (0,0,0) and (255,255,255) to ensure colorfulness
        public Color NextColor(Color startColor)
        {
            const int stepsize = 1; // 15 or 17 for UpdateFrequency.Update10

            if (startColor.R > 0 && startColor.G == 0 && startColor.B == 255)
            {
                startColor.R -= stepsize;
                return startColor;
            }
            else if (startColor.R == 0 && startColor.G < 255 && startColor.B == 255)
            {
                startColor.G += stepsize;
                return startColor;
            }
            else if (startColor.R == 0 && startColor.G == 255 && startColor.B > 0)
            {
                startColor.B -= stepsize;
                return startColor;
            }
            else if (startColor.R < 255 && startColor.G == 255 && startColor.B == 0)
            {
                startColor.R += stepsize;
                return startColor;
            }
            else if (startColor.R == 255 && startColor.G > 0 && startColor.B == 0)
            {
                startColor.G -= stepsize;
                return startColor;
            }
            else if (startColor.R == 255 && startColor.G == 0 && startColor.B < 255)
            {
                startColor.B += stepsize;
                return startColor;
            }
            else return startColor;
        }


        // Gives back a random and valid color
        // A valid color in this case is any color, where exactly one value is 0, another one is 255 and the last is random from 0-255
        // This is to ensure the color is on a edge of the RGB cube, which is not connected to black or white ((0,0,0) and (255,255,255)) to ensure colorfullness
        public Color RandomValidColor()
        {
            int randomNmbr = rnd.Next(6);

            byte x = (byte) rnd.Next(256);

            Color rndColor = new Color();

            // TODO get rid of the switch. Use a random permutation of (0,x,255) and give RGB each one of the values.
            switch (randomNmbr)  
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
