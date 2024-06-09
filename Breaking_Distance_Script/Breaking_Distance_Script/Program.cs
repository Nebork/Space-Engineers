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

        /*
        Nebork's Breaking_Distance_Script

        This script calculates and shows the required time and distance to come to a full stop with the current ship.
        This is intended to be used for larger space vessels who might carry heavy load, e.g. miners and haulers.

        The scipt only takes forward thrusters into account, thus you do not need to rotate your ship or anything else to achieve the breaking distance.
        */

        const bool show_in_cockpit = true;  // true, if you want the results shown in your cockpit  TODO add LCD support
        const string cockpitName = "Miner Cockpit";  // Name of cockpit
        const int display = 2;  // Display number in cockpit

        readonly bool show_safeDistance = false;  // true, if you want the safe distance to be shown, false otherwise
        double safetyProportion = 1.1;  // safe distance = distance * safetyProportion. Should be greater than 1!


        // Any changes made below the following line are made on your own risk!
        // --------------------------------------------------------------------------------------------------------


        // Variables
        readonly bool debugMode = false;  // If true outputs the debugText instead of the output
        readonly double inaccuracyProportion = 1.00;  // additional distance to stop  TODO really needed?

        double speed;  // speed in m/s
        double totalMass;  // mass in Kg
        double force;  // force in N
        double accerlation;  // accerlation in m/s^2
        double time;  // time in s
        double distance;  // distance in m
        double safeDistance;  // distance * safetyProportion in m
        string output;  // output text
        string debugText;  // debug text, which is used to show all values for debug

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            // surface.ContentType = ContentType.TEXT_AND_IMAGE;  //TODO put all the objects up here
            // surface.Alignment = TextAlignment.CENTER;

            if (Me.CubeGrid.IsStatic)
            {
                Echo("This grid is a station! \n");
                Runtime.UpdateFrequency = UpdateFrequency.None;
                return;
            }
        }

        public void Main(/*string argument, UpdateType updateSource*/)
        {

            // Initializes lists of all cockpits and thrusters
            List<IMyCockpit> cockpits = new List<IMyCockpit>();
            GridTerminalSystem.GetBlocksOfType<IMyCockpit>(cockpits);
            IMyCockpit cockpit = null;

            if (cockpits.Count == 0)
            {
                Echo("There are no cockpits on this ship! \n");
                Runtime.UpdateFrequency = UpdateFrequency.None;
                return;
            }

            for (int i = 0; i < cockpits.Count; i++)  // TODO simplify, used in light script a way to find block of name, look up in MDK
            {
                if (cockpits[i].CustomName == cockpitName)
                {
                    cockpit = cockpits[i];
                }
            }
            if (cockpit == null)
            {
                Echo("There is no cockpit named " + cockpitName + " on this ship! \n");
                Runtime.UpdateFrequency = UpdateFrequency.None;
                return;
            }
            List<IMyThrust> thrusters = new List<IMyThrust>();
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters);


            // Calculates ship's mass
            MyShipMass shipMass = cockpit.CalculateShipMass();
            totalMass = (int)shipMass.TotalMass; // mass including inventory cargo
                                                 // text = text + "Total Ship Weight: " + totalMass + "Kg \n";

            // Calculates ship's speed
            speed = Math.Round(cockpit.GetShipSpeed(), 3);
            debugText += "Ship Speed: " + speed + "m/s \n";

            // Calculates ship's force
            for (int i = 0; i < thrusters.Count; i++)
            {
                IMyThrust thruster = thrusters[i];
                if (thruster.Orientation.ToString().Split(',')[0] == cockpit.Orientation.ToString().Split(',')[0])
                {
                    force += thruster.MaxThrust;
                }
            }
            force = Math.Round(force, 3);
            debugText += force + "N \n";

            // Calculates ship's accerlation
            accerlation = Math.Round(force / totalMass, 3);
            debugText += accerlation + "m/s² \n";

            // Calculates ship's time to full stop
            time = Math.Round(speed / accerlation, 3);
            output += time + " seconds to stop \n";
            debugText += time + " seconds to stop \n";

            // Calculates ship's distance to full stop
            distance = Math.Round((((speed * time) / 2) * inaccuracyProportion), 1);
            output += distance + " meters to full stop \n";
            debugText += distance + " meters to full stop \n";

            // If show_safeDistance is true calculates and shows safe distance to full stop
            if (show_safeDistance)
            {
                if (safetyProportion < 1) { safetyProportion = 1; }
                safeDistance = Math.Round(distance * safetyProportion, 1);
                output += safeDistance + " for a save stop \n";
                debugText += safeDistance + " for a save stop \n";
            }

            // If debug is on
            if (debugMode) { Echo(debugText); }

            // Prints everything on the cockpits panel
            if (show_in_cockpit)
            {
                IMyTextSurface surface = null;
                try
                {
                    surface = cockpit.GetSurface(display);
                }
                catch (Exception e)
                {
                    Echo("there is no display with number " + display + "\n");
                    Echo(e.ToString());
                }

                surface.ContentType = ContentType.TEXT_AND_IMAGE;  //TODO put all the objects up here
                surface.Alignment = TextAlignment.CENTER;
                surface.WriteText(output, false);
            }
        }
    }
}
