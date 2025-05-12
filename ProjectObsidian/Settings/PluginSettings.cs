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
}