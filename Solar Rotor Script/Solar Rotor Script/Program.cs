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
        public void Main(string argument, UpdateType updateSource)
        {
            // This script rotates panels connected to a rotor.
            // The script allows up to nine solar arrays with a rotor each, to use multiple just adjust the size parameter.
            // Every rotor has a designated solar panel it uses as a reference.This panel should be named differently than the other solar panels on one array.
            // A rotor and its reference solar panel must have the same number, so rotor 1 uses solar panel 1 as a reference etc.

            // Amount of rotors and reference panels
            int size = 3;

            // Critical power value, below which the rotor shall rotate. Value in kW
            int critPower = 140;


            // The prefixes of a rotor and its designated panel
            // With this example the first rotor should be named "Station Script Rotor 1",
            // while the according solar panel should be named "Station Script Panel 1".

            string rotorPrefix = "Station Script Rotor";
            string panelPrefix = "Station Script Panel";



            // ==========================================================================
            // Scripts Workspace


            // Iteration for every pair of rotors and solar panels
            for (int i = 1; i <= size; i++)
            {
                IMyMotorStator rotor = GridTerminalSystem.GetBlockWithName(rotorPrefix + " " + i.ToString()) as IMyMotorStator;
                IMySolarPanel panel = GridTerminalSystem.GetBlockWithName(panelPrefix + " " + i.ToString()) as IMySolarPanel;


                // If power output is below critPower in kW, rotate the solar panel. Else stop it.
                rotor.TargetVelocityRPM = panel.CurrentOutput < (critPower / 1000) ? 0.5F : 0F;
            }
        }
    }
}
