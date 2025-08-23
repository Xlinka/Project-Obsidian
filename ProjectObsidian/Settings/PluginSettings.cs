using System;
using System.Collections.Generic;
using System.Linq;
using FrooxEngine;
using Elements.Core;
using Elements.Assets;
using System.Reflection;
using System.Threading.Tasks;

namespace Obsidian;

[SettingCategory("Obsidian")]
[AutoRegisterSetting]
public class PluginSettings : SettingComponent<PluginSettings>
{
    public override bool UserspaceOnly => true;

    [DefaultValue(true)]
    [SettingIndicatorProperty(null, null, null, null, false, 0L)]
    public readonly Sync<bool> PluginLoaded;

    private static AssemblyTypeRegistry obsidianRegistry;

    private static List<AssemblyTypeRegistry> coreAssemblies;

    private LocaleData _localeData;

    protected override void OnStart()
    {
        var obsidianRegistry = GetObsidianRegistry();
        if (PluginLoaded.Value == false && obsidianRegistry != null && coreAssemblies.Contains(obsidianRegistry))
        {
            TogglePluginLoaded();
        }
        else if (PluginLoaded.Value == true && obsidianRegistry != null && !coreAssemblies.Contains(obsidianRegistry))
        {
            TogglePluginLoaded();
        }

        _localeData = new LocaleData();
        _localeData.LocaleCode = "en";
        _localeData.Authors = new List<string>() { "Nytra" };
        _localeData.Messages = new Dictionary<string, string>();

        _localeData.Messages.Add("Settings.Category.Obsidian", "Obsidian");

        _localeData.Messages.Add("Settings.PluginSettings", "Plugin Settings");
        _localeData.Messages.Add("Settings.PluginSettings.PluginLoaded", "Plugin Loaded");
        _localeData.Messages.Add("Settings.PluginSettings.TogglePluginLoaded", "Toggle loading the plugin for new sessions");
        _localeData.Messages.Add("Settings.PluginSettings.RefreshLocale", "Refresh Locale");

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

        SettingsLocaleHelper.Update(_localeData);
    }

    private AssemblyTypeRegistry GetObsidianRegistry()
    {
        if (coreAssemblies == null)
        {
            coreAssemblies = (List<AssemblyTypeRegistry>)typeof(GlobalTypeRegistry).GetField("_coreAssemblies", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null);
            if (coreAssemblies == null)
            {
                UniLog.Error("Core assemblies list is null!");
                return null;
            }
        }
        if (obsidianRegistry == null)
        {
            obsidianRegistry = coreAssemblies.FirstOrDefault(assembly => assembly.Assembly == Assembly.GetExecutingAssembly());
            if (obsidianRegistry == null)
            {
                UniLog.Error("Obsidian registry is null!");
            }
        }
        return obsidianRegistry;
    }

    [SettingProperty(null, null, null, false, 0L, null, null)]
    [SyncMethod(typeof(Action), new string[] { })]
    public void TogglePluginLoaded()
    {
        UniLog.Log("Toggling plugin loaded");

        if (GetObsidianRegistry() is AssemblyTypeRegistry obsidianRegistry)
        {
            if (coreAssemblies.Contains(obsidianRegistry))
            {
                UniLog.Log("Removing Obsidian registry");
                coreAssemblies.Remove(obsidianRegistry);
                PluginLoaded.Value = false;
            }
            else if (!coreAssemblies.Contains(obsidianRegistry))
            {
                UniLog.Log("Adding Obsidian registry");
                coreAssemblies.Add(obsidianRegistry);
                PluginLoaded.Value = true;
            }
            try
            {
                var localSettings = Userspace.UserspaceWorld.GetGloballyRegisteredComponent<SettingManagersManager>().LocalSettings.Target;
                var localPluginSettings = localSettings.GetSetting<PluginSettings>();
                localPluginSettings.PluginLoaded.Value = PluginLoaded.Value;
            }
            catch (Exception ex)
            {
                UniLog.Error($"Could not update local plugin settings! {ex}");
            }
        }
    }

    [SettingProperty(null, null, null, false, 0L, null, null)]
    [SyncMethod(typeof(Action), new string[] { })]
    public void RefreshLocale()
    {
        UniLog.Log("Refresh locale pressed");

        SettingsLocaleHelper.Update(_localeData);
    }
}