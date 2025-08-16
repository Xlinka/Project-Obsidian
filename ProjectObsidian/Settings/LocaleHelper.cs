using System.Reflection;
using Elements.Assets;
using Elements.Core;
using FrooxEngine;

namespace Obsidian;

public static class SettingsLocaleHelper
{
    private static StaticLocaleProvider localeProvider;
    private static string lastOverrideLocale;
    private const string overrideLocaleString = "somethingRandomJustToMakeItChange";
    //private static MethodInfo _forceAssetUpdateMethod = typeof(Asset<>).GetMethod("ForceAssetUpdate", BindingFlags.NonPublic | BindingFlags.Instance);
    public static void Update(LocaleData _localeData)
    {
        UpdateDelayed(_localeData);
        Settings.RegisterValueChanges<LocaleSettings>(localeSettings => UpdateDelayed(_localeData));
        
    }
    private static void UpdateDelayed(LocaleData _localeData)
    {
        // I hate having to do an arbitrary delay, but it doesn't work otherwise
        Userspace.UserspaceWorld.RunInUpdates(7, () => UpdateIntern(_localeData));
        //UpdateIntern(_localeData);
    }
    private static void UpdateIntern(LocaleData _localeData)
    {
        localeProvider = Userspace.UserspaceWorld.GetCoreLocale();
        if (localeProvider?.Asset?.Data is null)
            Userspace.UserspaceWorld.RunSynchronously(() => UpdateIntern(_localeData));
        else
        {
            //Userspace.UserspaceWorld.RunSynchronously(() => UpdateLocale(_localeData));
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

            //_forceAssetUpdateMethod.Invoke(localeProvider.Asset, null);

            // force asset update for locale provider
            if (localeProvider.OverrideLocale.Value != null && localeProvider.OverrideLocale.Value != overrideLocaleString)
                lastOverrideLocale = localeProvider.OverrideLocale.Value;
            localeProvider.OverrideLocale.Value = overrideLocaleString;
            Userspace.UserspaceWorld.RunInUpdates(1, () =>
            {
                localeProvider.OverrideLocale.Value = lastOverrideLocale;
            });
        }
        else
            UniLog.Error("Locale data is null when it shouldn't be!");
    }
}