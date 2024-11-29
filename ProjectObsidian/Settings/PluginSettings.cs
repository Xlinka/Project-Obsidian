using System;
using System.Collections.Generic;
using System.Linq;
using FrooxEngine;
using Elements.Core;
using Elements.Assets;
using Commons.Music.Midi;
using System.Reflection;

namespace Obsidian;

[SettingCategory("Obsidian")]
public class PluginSettings : SettingComponent<PluginSettings>
{
    public override bool UserspaceOnly => true;

    [NonPersistent]
    [SettingIndicatorProperty(null, null, null, null, false, 0L)]
    public readonly Sync<bool> CoreAssemblyLoaded;

    private LocaleData _localeData;

    private static AssemblyTypeRegistry obsidianRegistry;

    protected override void OnStart()
    {
        base.OnStart();
        _localeData = new LocaleData();
        _localeData.LocaleCode = "en";
        _localeData.Authors = new List<string>() { "Nytra" };
        _localeData.Messages = new Dictionary<string, string>();
        _localeData.Messages.Add("Settings.Category.Obsidian", "Obsidian");
        _localeData.Messages.Add("Settings.PluginSettings", "Plugin Settings");
        _localeData.Messages.Add("Settings.PluginSettings.CoreAssemblyLoaded", "Core Assembly Loaded");
        _localeData.Messages.Add("Settings.PluginSettings.ToggleCoreAssembly", "Toggle Core Assembly");

        CoreAssemblyLoaded.Value = true;

        // Sometimes the locale is null in here, so wait a bit I guess

        RunInUpdates(15, () =>
        {
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
        this.GetCoreLocale()?.Asset?.Data.LoadDataAdditively(_localeData);
    }

    [SettingProperty(null, null, null, false, 0L, null, null)]
    [SyncMethod(typeof(Action), new string[] { })]
    public void ToggleCoreAssembly()
    {
        UniLog.Log("Toggle pressed");
        var glob = (List<AssemblyTypeRegistry>)typeof(GlobalTypeRegistry).GetField("_coreAssemblies", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        if (CoreAssemblyLoaded)
        {
            foreach (var thing in glob.ToList())
            {
                if (thing.Assembly == Assembly.GetExecutingAssembly())
                {
                    obsidianRegistry = thing;
                    glob.Remove(thing);
                }
            }
            CoreAssemblyLoaded.Value = false;
        }
        else
        {
            glob.Add(obsidianRegistry);
            CoreAssemblyLoaded.Value = true;
        }
    }
}