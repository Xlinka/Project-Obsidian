using System;
using System.Collections.Generic;
using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using Obsidian.Shaders;

namespace Obsidian
{
    [ImplementableClass(true)]
    internal static class ExecutionHook
    {
        // Fields for reflective access
        private static Type? __connectorType;
        private static Type? __connectorTypes;

        // Static constructor for initializing the hook
        static ExecutionHook()
        {
            try
            {
                Engine.Current.OnReady += () =>
                {
                    ShaderInjection.AppendShaders();
                    Settings.GetActiveSetting<PluginSettings>();

                    var _localeData = new LocaleData();
                    _localeData.LocaleCode = "en";
                    _localeData.Authors = new List<string>() { "Nytra" };
                    _localeData.Messages = new Dictionary<string, string>();
                    _localeData.Messages.Add("Settings.Category.Obsidian", "Obsidian");
                    _localeData.Messages.Add("Settings.PluginSettings", "Plugin Settings");
                    _localeData.Messages.Add("Settings.PluginSettings.PluginLoaded", "Plugin Loaded");
                    _localeData.Messages.Add("Settings.PluginSettings.TogglePluginLoaded", "Toggle loading the plugin for new sessions");

                    _localeData.Messages.Add("Settings.MIDI_Settings", "MIDI Settings");
                    _localeData.Messages.Add("Settings.MIDI_Settings.RefreshDeviceLists", "Refresh Devices");
                    _localeData.Messages.Add("Settings.MIDI_Settings.InputDevices", "Input Devices");
                    _localeData.Messages.Add("Settings.MIDI_Settings.OutputDevices", "Output Devices");

                    _localeData.Messages.Add("Settings.MIDI_Settings.DeviceName", "Device Name");
                    _localeData.Messages.Add("Settings.MIDI_Settings.OutputDevices.Breadcrumb", "MIDI Output Devices");
                    _localeData.Messages.Add("Settings.MIDI_Settings.InputDevices.Breadcrumb", "MIDI Input Devices");
                    _localeData.Messages.Add("Settings.MIDI_Settings.AllowConnections", "Allow Connections");
                    _localeData.Messages.Add("Settings.MIDI_Settings.DeviceFound", "Device Found");
                    _localeData.Messages.Add("Settings.MIDI_Settings.Remove", "Remove");

                    SettingsLocaleHelper.RegisterData(_localeData);
                    
                };
            }
            catch (Exception e)
            {
                UniLog.Log($"Exception thrown during initialization: {e}");
            }
        }

        // Method to instantiate the connector
        private static DummyConnector InstantiateConnector() => new DummyConnector();

        // Dummy connector class implementing IConnector
        private class DummyConnector : IConnector
        {
            public IImplementable? Owner { get; private set; }

            public void ApplyChanges() { }

            public void AssignOwner(IImplementable owner) => Owner = owner;

            public void Destroy(bool destroyingWorld) { }

            public void Initialize() { }

            public void RemoveOwner() => Owner = null;
        }
    }
}
