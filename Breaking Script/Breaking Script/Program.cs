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
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // true, if you want the results shown in your cockpit
            bool show_in_cockpit = true;

            // Name of cockpit
            string cockpitName = "Miner Cockpit";

            // Display number in cockpit
            int display = 2;

            // true, if you want the safe distance to be shown, false otherwise
            bool show_safeDistance = true;
            // safe distance = distance * safetyCoefficient. Should be greater than 1!
            double safetyCoefficient = 1.1;

            // true, if the script's name should be displayed on the programmable block
            bool show_name_on_programmable_block = true;



            // SHIPS MAGIC
            double speed = 0;       // speed in m/s
            double totalMass = 0;   // mass in Kg
            double force = 0;       // force in N
            double accerlation = 0; // accerlation in m/s^2
            double time = 0;        // time in s
            double distance = 0;    // distance in m
            double safeDistance = 0;// distance * safetyCoefficient in m
            string text = "";       // output text

            double inaccuracyCoefficient = 1.00; // additional distance to stop, 1 for the original distance.


            // Initializes lists of all cockpits and thrusters
            List<IMyCockpit> cockpits = new List<IMyCockpit>();
            GridTerminalSystem.GetBlocksOfType<IMyCockpit>(cockpits);
            IMyCockpit cockpit = null;

            if (cockpits.Count == 0)
            {
                Echo("There are no cockpits on this ship! \n");
                return;
            }

            for (int i = 0; i < cockpits.Count; i++)
            {
                if (cockpits[i].CustomName == cockpitName)
                {
                    cockpit = cockpits[i];
                }
            }
            if (cockpit == null)
            {
                Echo("There is no cockpit named " + cockpitName + " on this ship! \n");
                return;
            }
            List<IMyThrust> thrusters = new List<IMyThrust>();
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters);


            // Calculates ship's mass
            MyShipMass shipMass = cockpit.CalculateShipMass();
            totalMass = (int)shipMass.TotalMass; // mass including inventory cargo
            //text = text + "Total Ship Weight: " + totalMass + "Kg \n";

            // Calculates ship's speed
            speed = Math.Round(cockpit.GetShipSpeed(), 3);
            //text += "Ship Speed: " + speed + "m/s \n";

            // Calculates ship's breaking force
            for (int i = 0; i < thrusters.Count; i++)
            {
                IMyThrust thruster = thrusters[i];
                if (thruster.Orientation.Forward == cockpit.Orientation.Forward)
                {
                    force += thruster.MaxThrust;
                }
            }
            force = Math.Round(force, 3);
            //text = text + force + "N \n";

            // Calculates ship's accerlation
            accerlation = Math.Round(force / totalMass, 3);
            //text = text + accerlation + "m/s² \n";

            // Calculates ship's time to full stop
            time = Math.Round(speed / accerlation, 3);
            text = text + time + " seconds to stop \n";

            // Calculates ship's distance to full stop
            distance = Math.Round(speed * time / 2 * inaccuracyCoefficient, 1);
            text = text + distance + " meters to full stop \n";

            // If show_safeDistance is true calculates and shows safe distance to full stop
            if (show_safeDistance)
            {
                if (safetyCoefficient < 1) { safetyCoefficient = 1; }
                safeDistance = Math.Round(distance * safetyCoefficient, 1);
                text = text + safeDistance + " for a save stop \n";
            }


            // Prints everything on the cockpits panel
            if (show_in_cockpit)
            {
                IMyTextSurface surface = null;
                try
                {
                    surface = cockpit.GetSurface(display);
                }
                catch (Exception)
                {
                    Echo("there is no display with number " + display);
                }

                if (surface.FontSize == 1 && surface.TextPadding == 2 && surface.Alignment == TextAlignment.LEFT)
                {
                    surface.Font = "DEBUG";
                    surface.FontSize = 1.5F;
                    surface.Alignment = TextAlignment.CENTER;
                    surface.TextPadding = 35;
                }

                surface.ContentType = ContentType.TEXT_AND_IMAGE;
                surface.WriteText(text, false);
            }

            // Prints the script's name on the programmable block.
            if (show_name_on_programmable_block)
            {
                IMyTextSurface surface = Me.GetSurface(0);

                surface.ContentType = ContentType.TEXT_AND_IMAGE;
                surface.Font = "DEBUG";
                surface.FontSize = 1.75F;
                surface.Alignment = TextAlignment.CENTER;
                surface.TextPadding = 40;

                surface.WriteText("Nebork's Breaking Script", false);
            }
        }
    }
}
