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

        (GROUND) BREAKING DISTANCE SCRIPT

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Main(string argument, UpdateType updateSource)
        {

            // INITIALIZING
            bool show_in_cockpit = true;  // true, if you want the results shown in your cockpit
            string cockpitName = "Miner Cockpit";  // Name of cockpit
            int display = 2;  // Display number in cockpit

            bool show_safeDistance = false;  // true, if you want the safe distance to be shown, false otherwise
            double safetyProportion = 1.1;  // safe distance = distance * safetyProportion. Should be greater than 1!



            // DON'T TOUCH!!!
            double speed = 0;  // speed in m/s
            double totalMass = 0;  // mass in Kg
            double force = 0;  // force in N
            double accerlation = 0;  // accerlation in m/s^2
            double time = 0;  // time in s
            double distance = 0;  // distance in m
            double safeDistance = 0;  // distance * safetyProportion in m
            string text = "";  // output text

            double inaccuracyProportion = 1.00;  // additional distance to stop


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
            }
            List<IMyThrust> thrusters = new List<IMyThrust>();
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters);


            // Calculates ship's mass
            MyShipMass shipMass = cockpit.CalculateShipMass();
            totalMass = (int)shipMass.TotalMass; // mass including inventory cargo
                                                 // text = text + "Total Ship Weight: " + totalMass + "Kg \n";

            // Calculates ship's speed
            speed = Math.Round(cockpit.GetShipSpeed(), 3);
            // text += "Ship Speed: " + speed + "m/s \n";

            // Calculates ship's force
            for (int i = 0; i < thrusters.Count; i++)
            {
                IMyThrust thruster = thrusters[i];
                if (thruster.Orientation.ToString().Split(',')[0] == cockpit.Orientation.ToString().Split(',')[0])
                {
                    force = force + thruster.MaxThrust;
                }
            }
            force = Math.Round(force, 3);
            // text = text + force + "N \n";

            // Calculates ship's accerlation
            accerlation = Math.Round(force / totalMass, 3);
            // text = text + accerlation + "m/s² \n";

            // Calculates ship's time to full stop
            time = Math.Round(speed / accerlation, 3);
            text = text + time + " seconds to stop \n";

            // Calculates ship's distance to full stop
            distance = Math.Round((((speed * time) / 2) * inaccuracyProportion), 1);
            text = text + distance + " meters to full stop \n";

            // If show_safeDistance is true calculates and shows safe distance to full stop
            if (show_safeDistance)
            {
                if (safetyProportion < 1) { safetyProportion = 1; }
                safeDistance = Math.Round(distance * safetyProportion, 1);
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
                catch (Exception e)
                {
                    Echo("there is no display with number " + display);
                }
                surface.ContentType = ContentType.TEXT_AND_IMAGE;
                surface.WriteText(text, false);
            }
        }
    }
}
