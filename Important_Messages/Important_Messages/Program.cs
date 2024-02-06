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
        //how many runs the programm shall do nothing
        int wait;

        //which string is currently used
        int message_number;

        //how many literals are waiting to be shown
        int literals_left;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            string[] storedData = Storage.Split(';');

            switch(storedData.Length)
            {
                case 1:
                    wait = int.Parse(storedData[0]);
                    message_number = 0;
                    literals_left = 0;
                    break;

                case 2:
                    wait = int.Parse(storedData[0]);
                    message_number = int.Parse(storedData[1]);
                    literals_left = 0;
                    break;

                case 3:
                    wait = int.Parse(storedData[0]);
                    message_number = int.Parse(storedData[1]);
                    literals_left = int.Parse(storedData[2]);
                    break;

                default:
                    wait = 0;
                    message_number = 0;
                    literals_left = 0;
                    break;
            }
        }

        public void Save()
        {
            Storage = string.Join(";", wait, message_number, literals_left);
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // All messages you want to be shown on the LCDs.
            // Just add a message, put " around it and add a , after every message.
            // The following messages are just examples, feel free to replace and keep them.
            string[] messages =
            {
                "Here could be your message.",
                "The worlds oldest gold fish was 41 years old and called Fred.",
                "WARNING: All planets stopped moving, the sun suddenly spins around us.",
                "The Council of the Space Engineers finally sets pi to 3!",
            };
        }
    }
}
