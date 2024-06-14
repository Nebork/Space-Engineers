using Sandbox.Game.Entities;
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
        Nebork's Breaking Distance Script

        This script calculates and shows the required time and distance to come to a full stop with the current ship.
        This is intended to be used for larger space vessels who might carry heavy load, e.g. miners and haulers.

        The script only takes forward thrusters into account, thus you do not need to rotate your ship or anything else to achieve the breaking distance.
        The script also only prints the data, thus you can change the color, padding etc. to your liking!
        */

        readonly string cockpitName = "";  // Name of cockpit, if empty uses the current main cockpit. Remote control is possible, see showOnLcD
        readonly int display = 2;  // Index number of display in cockpit, starting from 0.

        readonly bool showSafeDistance = false;  // true, if you want the safe distance to be shown instead, false otherwise.
        readonly double safetyProportion = 1.1;  // safe distance = distance * safetyProportion. Should be greater than 1!

        // If you want the results shown in an LCD instead of the cockpit.
        readonly bool showOnLcd = false;  // true,  Needed for remote control. If using RC, please enter the name in cockpitName!
        readonly string lcdName = "Distance LCD";  // Name of LCD, needs to be the name of the LCD, can not be left empty!


        // Any changes made below the following line are made on your own risk!
        // --------------------------------------------------------------------------------------------------------


        // Variables
        readonly bool debugMode = false;  // If true outputs the debugText instead of the output

        readonly List<IMyShipController> cockpits = new List<IMyShipController>();
        readonly IMyShipController mainCockpit;

        readonly IMyTextSurface cockpitSurface;

        readonly IMyTextPanel lcdPanel;

        readonly List<IMyThrust> thrusters = new List<IMyThrust>();


        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            Echo("");  // Flushes previous messages

            if (Me.CubeGrid.IsStatic)  // If grid is a station terminate program
            {
                Echo("This grid is a station! \nPlease fix and recompile.");
                Runtime.UpdateFrequency = UpdateFrequency.None;
                return;
            }


            // Finds the given or main cockpit, needed to get speed and mass later on
            if (cockpitName == "")  // If no cockpit name given, search for main cockpit
            {
                GridTerminalSystem.GetBlocksOfType<IMyShipController>(cockpits);
                if (cockpits == null)
                {
                    Echo("There are no connected cockpits! \nPlease fix and recompile.");
                    Runtime.UpdateFrequency = UpdateFrequency.None;
                    return;
                }
                else
                {
                    foreach (IMyShipController cockpit in cockpits)  // Finds main cockpit
                    {
                        if (cockpit.IsMainCockpit)
                        {
                            mainCockpit = cockpit;
                            break;
                        }
                    }
                    if (mainCockpit == null)
                    {
                        Echo("No cockpit has been declared as main cockpit! \nIf remote control is used, enter it's name please. \nPlease fix and recompile.");
                        Runtime.UpdateFrequency = UpdateFrequency.None;
                        return;
                    }
                }
            }
            else  // Retrieve cockpit from given name
            {
                // Checks if the found name is actually a cockpit
                IMyTerminalBlock typeCheckBlock = GridTerminalSystem.GetBlockWithName(cockpitName);
                if (typeCheckBlock == null)
                {
                    Echo($"There is no block with the name \"{cockpitName}\"! \nPlease fix and recompile.");
                    Runtime.UpdateFrequency = UpdateFrequency.None;
                    return;
                }
                if (typeCheckBlock is IMyShipController) { mainCockpit = (IMyShipController)typeCheckBlock; }
                else
                {
                    Echo($"{cockpitName} is not a cockpit! \nPlease fix and recompile.");
                    Runtime.UpdateFrequency = UpdateFrequency.None;
                    return;
                }
            }

            // Retrieve all the correct thrusters, needs main cockpit first
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters, thruster => thruster.Orientation.Forward == mainCockpit.Orientation.Forward);

            if (!showOnLcd)
            {
                if (mainCockpit is IMyCockpit) { cockpitSurface = ((IMyCockpit)mainCockpit).GetSurface(display); }
                else
                {
                    Echo($"\"{mainCockpit.CustomName}\" Is not cockpit with displays!  \nPlease fix and recompile.");
                    Runtime.UpdateFrequency = UpdateFrequency.None;
                    return;
                }
                
                if (cockpitSurface == null)
                {
                    Echo($"There is no display with index {display} in \"{mainCockpit.CustomName}\"! \nPlease fix and recompile.");
                    Runtime.UpdateFrequency = UpdateFrequency.None;
                    return;
                }

                // Settings for cockpit design
                cockpitSurface.ContentType = ContentType.TEXT_AND_IMAGE;
                cockpitSurface.Alignment = TextAlignment.CENTER; 
            }
            else  // If we use LCD
            {
                IMyTerminalBlock typeCheckBlock = GridTerminalSystem.GetBlockWithName(lcdName);
                if (typeCheckBlock == null)
                {
                    Echo($"There is no block with the name \"{lcdName}\"! \nPlease fix and recompile.");
                    Runtime.UpdateFrequency = UpdateFrequency.None;
                    return;
                }
                if (typeCheckBlock is IMyTextPanel) { lcdPanel = (IMyTextPanel)typeCheckBlock;}
                else
                {
                    Echo($"{lcdName} is not an LCD! \nPlease fix and recompile.");
                    Runtime.UpdateFrequency = UpdateFrequency.None;
                    return;
                }

                // Settings for LCD design
                lcdPanel.ContentType = ContentType.TEXT_AND_IMAGE;
                lcdPanel.Alignment = TextAlignment.CENTER;
            }
        }

        public void Main()
        {
            double speed;  // speed in m/s
            double totalMass;  // mass in Kg
            double force = 0;  // force in N
            double accerlation;  // accerlation in m/s^2
            double time;  // time in s
            double distance;  // distance in m
            string output = "";  // output text
            string debugText = "";  // debug text, which is used to show all values for debug


            // All the math stuff!
            // Calculates ship's mass
            MyShipMass shipMass = mainCockpit.CalculateShipMass();
            totalMass = (int)shipMass.TotalMass;  // mass including inventory cargo in kg
            debugText += "Total Ship Weight: " + totalMass + "Kg \n";

            // Calculates ship's speed
            speed = Math.Round(mainCockpit.GetShipSpeed(), 3);
            debugText += "Ship Speed: " + speed + "m/s \n";

            // Calculates ship's force
            foreach (IMyThrust thruster in thrusters)
            {
                force += thruster.MaxThrust;
            }
            force = Math.Round(force, 3);
            debugText += force + "N \n";

            // Calculates ship's accerlation
            accerlation = Math.Round(force / totalMass, 3);
            debugText += accerlation + "m/s² \n";

            // Calculates ship's time to full stop
            time = Math.Round(speed / accerlation, 3);
            output += Math.Round(time, 1) + " seconds to stop \n";
            debugText += Math.Round(time, 1) + " seconds to stop \n";

            // Calculates ship's distance to full stop
            distance = Math.Round((speed * time) / 2);

            // Evaluates if true distance or safe distance should be shown
            if (showSafeDistance) { output += $"{(int) (distance * safetyProportion)} meters for a save stop \n"; }
            else { output += $"{distance} meters for to full stop \n"; }

            debugText += $"{distance} meters for a full stop \n";
            debugText += $"{(int) distance * safetyProportion} meters for a save stop \n";

            // If debug is on
            if (debugMode) { output = debugText; } 

            // Actual output in the cockpit
            if (!showOnLcd) { cockpitSurface.WriteText(output, false); }
            else { lcdPanel.WriteText(output, false); }
        }

        // COPY UNTIL HERE
    }
}
