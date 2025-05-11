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

    [SettingIndicatorProperty(null, null, null, null, false, 0L)]
    public readonly Sync<bool> PluginLoaded;

    private LocaleData _localeData;

    private static AssemblyTypeRegistry obsidianRegistry;

    private static List<AssemblyTypeRegistry> coreAssemblies;

    protected override void OnStart()
    {
        base.OnStart();

        _localeData = new LocaleData();
        _localeData.LocaleCode = "en";
        _localeData.Authors = new List<string>() { "Nytra" };
        _localeData.Messages = new Dictionary<string, string>();
        _localeData.Messages.Add("Settings.Category.Obsidian", "Obsidian");
        _localeData.Messages.Add("Settings.PluginSettings", "Plugin Settings");
        _localeData.Messages.Add("Settings.PluginSettings.PluginLoaded", "Plugin Loaded");
        _localeData.Messages.Add("Settings.PluginSettings.TogglePluginLoaded", "Toggle loading the plugin for new sessions");

        if (PluginLoaded.Value == false && GetObsidianRegistry() is AssemblyTypeRegistry obsidianRegistry && coreAssemblies.Contains(obsidianRegistry))
        {
            TogglePluginLoaded();
        }

        Task.Run(async () => 
        { 
            while (this.GetCoreLocale()?.Asset?.Data is null)
            {
                await default(NextUpdate);
            }
            UpdateLocale();
            Settings.RegisterValueChanges<LocaleSettings>(UpdateLocale);
        });
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        Settings.UnregisterValueChanges<LocaleSettings>(UpdateLocale);
    }

    private void UpdateLocale(LocaleSettings settings = null)
    {
        this.GetCoreLocale()?.Asset?.Data?.LoadDataAdditively(_localeData);
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
        }
    }
}