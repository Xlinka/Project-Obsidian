using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elements.Core;
using FrooxEngine;


namespace Obsidian.Hardware
{
    internal class HardwareInjector
    {
        private static readonly List<Type> HardwareClasses = new()
        {
            //typeof(XboxOneController),
        };

        private static async Task RegisterHardwareClass(Type hardwareClass)
        {
            UniLog.Log($"Initializing hardware class: {hardwareClass.Name}");
        }

        public static void InitializeHardwareClasses() => Task.WaitAll(HardwareClasses.Select(hardwareClass => RegisterHardwareClass(hardwareClass)).ToArray());
    }
}