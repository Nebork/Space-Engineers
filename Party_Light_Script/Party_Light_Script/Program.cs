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

        /*
        Nebork's Party Light Script

        This script can smoothly change the color of all lights in the given group. First they are randomized and then change.
        If new lights are added to the group, or the group was renamed, please recompile the programmable block
        */

        readonly string groupName = "[PARTY] Party";  // Rename this to the EXACT name of the group containing the lights.
        const int stepsize = 10;  // The speed of the color change. The higher, the faster the color change. Values over 10 might not be smooth.


        // Any changes made below the following line are made on your own risk!
        // --------------------------------------------------------------------------------------------------------


        // Global Definitions
        IMyBlockGroup partyGroup;
        List<IMyLightingBlock> partyLights;

        Random rnd = new Random();

        // Main function that is run, every time the block is recompiled
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            // Gets a list of the blocks in the group groupName
            partyGroup = GridTerminalSystem.GetBlockGroupWithName(groupName);

            // Checks if the group is empty, thus no group has been found (empty list if no blocks in group). Stops whole script
            if (partyGroup == null)
            {
                Echo($"Could not find a group with name \"{groupName}\"!");
                Runtime.UpdateFrequency = UpdateFrequency.None;
                return;
            }

            // Fetches all the blocks in partyGroup from class IMyLightingBlock into partyLights
            partyLights = new List<IMyLightingBlock>();
            partyGroup.GetBlocksOfType<IMyLightingBlock>(partyLights);

            // Checks if there are any blocks in partyLights, else stop the whole script
            if (partyLights.Count == 0)
            {
                Echo($"Could not find any lights in the \"{groupName}\" group!");
                Runtime.UpdateFrequency = UpdateFrequency.None;
                return;
            }

            // Give every light a random and valid color. (one RGB value is 0, another one 255, and the last is random from 0-255)
            foreach (var partyLight in partyLights)
            {
                partyLight.Color = RandomValidColor(partyLight.Color);
            }

            Echo("");  //flushes previous messages. If this is executed program should work properly
        }


        // Returns a color, which is close to the given startColor. This is to ensure a smooth transition between the colors.
        // The color choosen is based on an euler-circle along the edges of a reduced RGB cube.
        // This reduction removed all edges connecting to (0,0,0) and (255,255,255) to ensure colorfulness
        public Color NextColor(Color startColor)
        {
            if (startColor.R > 0 && startColor.G == 0 && startColor.B == 255)
            {
                startColor.R -= (byte)(startColor.R % stepsize == 0 ? stepsize : startColor.R % stepsize);  // ensures that after the FIRST change, 0 is reached in x steps
                return startColor;
            }
            else if (startColor.R == 0 && startColor.G < 255 && startColor.B == 255)
            {
                startColor.G += (byte)((255 - startColor.G) % stepsize == 0 ? stepsize : (255 - startColor.G) % stepsize);  // same as above with 255 in x steps
                return startColor;
            }
            else if (startColor.R == 0 && startColor.G == 255 && startColor.B > 0)
            {
                startColor.B -= (byte)(startColor.B % stepsize == 0 ? stepsize : startColor.B % stepsize);
                return startColor;
            }
            else if (startColor.R < 255 && startColor.G == 255 && startColor.B == 0)
            {
                startColor.R += (byte)((255 - startColor.R) % stepsize == 0 ? stepsize : (255 - startColor.R) % stepsize);
                return startColor;
            }
            else if (startColor.R == 255 && startColor.G > 0 && startColor.B == 0)
            {
                startColor.G -= (byte)(startColor.G % stepsize == 0 ? stepsize : startColor.G % stepsize);
                return startColor;
            }
            else if (startColor.R == 255 && startColor.G == 0 && startColor.B < 255)
            {
                startColor.B += (byte)((255 - startColor.B) % stepsize == 0 ? stepsize : (255 - startColor.B) % stepsize);
                return startColor;
            }
            else
            {
                Echo("Could not change Color!");
                return startColor;
            }
        }


        // Gives back a random and valid color
        // A valid color in this case is any color, where exactly one value is 0, another one is 255 and the last is random from 0-255
        // This is to ensure the color is on a edge of the RGB cube, which is not connected to black or white ((0,0,0) and (255,255,255)) to ensure colorfullness
        public Color RandomValidColor(Color previousColor)
        {

            int randomNmbr = rnd.Next(6);

            byte x = (byte) rnd.Next(256);

            switch (randomNmbr)
            {
                case 0:
                    previousColor.R = x;
                    previousColor.G = 0;
                    previousColor.B = 255;
                    break;
                case 1:
                    previousColor.R = 0;
                    previousColor.G = x;
                    previousColor.B = 255;
                    break;
                case 2:
                    previousColor.R = 0;
                    previousColor.G = 255;
                    previousColor.B = x;
                    break;
                case 3:
                    previousColor.R = x;
                    previousColor.G = 255;
                    previousColor.B = 0;
                    break;
                case 4:
                    previousColor.R = 255;
                    previousColor.G = x;
                    previousColor.B = 0;
                    break;
                case 5:
                    previousColor.R = 255;
                    previousColor.G = 0;
                    previousColor.B = x;
                    break;
            }

            return previousColor;
        }

        // Main functions that runs every time the script is either run by hand, any block action OR via the UpdateFrequency property
        public void Main(/*string argument, UpdateType updateSource*/)
        {
            foreach (var partyLight in partyLights)
            {
                partyLight.Color = NextColor(partyLight.Color);
            }
        }

        // COPY UNTIL HERE
    }
}
