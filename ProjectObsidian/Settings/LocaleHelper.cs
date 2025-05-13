using Elements.Assets;
using Elements.Core;
using FrooxEngine;

namespace Obsidian;

public static class SettingsLocaleHelper
{
    private static StaticLocaleProvider localeProvider;
    public static void Update(LocaleData _localeData)
    {
        UpdateDelayed(_localeData);
        Settings.RegisterValueChanges<LocaleSettings>(localeSettings => UpdateDelayed(_localeData));
        
    }
    private static void UpdateDelayed(LocaleData _localeData)
    {
        Userspace.UserspaceWorld.RunInUpdates(15, () => UpdateIntern(_localeData));
    }
    private static void UpdateIntern(LocaleData _localeData)
    {
        localeProvider = Userspace.UserspaceWorld.GetCoreLocale();
        if (localeProvider?.Asset?.Data is null)
            Userspace.UserspaceWorld.RunSynchronously(() => UpdateIntern(_localeData));
        else
        {
            UpdateLocale(_localeData);
        }
    }
    private static void UpdateLocale(LocaleData _localeData)
    {
        UniLog.Log("Updating Obsidian locale!");
        //foreach (var kVP in _localeData.Messages)
        //{
            //UniLog.Log($"{kVP.Key} -> {kVP.Value}");
        //}
        if (localeProvider?.Asset?.Data != null)
        {
            localeProvider.Asset.Data.LoadDataAdditively(_localeData);

            // force asset update for locale provider
            localeProvider.OverrideLocale.Value = "owo";
            Userspace.UserspaceWorld.RunInUpdates(1, () => 
            {
                localeProvider.OverrideLocale.Value = null;
            });
        }
        else
            UniLog.Error("Locale data is null when it shouldn't be!");
    }
}