using System.Threading.Tasks;
using Elements.Assets;
using FrooxEngine;

namespace Obsidian;

public static class SettingsLocaleHelper
{
    public static void RegisterData(LocaleData _localeData)
    {
        Task.Run(async () => 
        {
            while (Userspace.UserspaceWorld?.GetCoreLocale()?.Asset?.Data is null)
            {
                await default(NextUpdate);
            }
            UpdateLocale(_localeData);
            Settings.RegisterValueChanges<LocaleSettings>(localeSettings => UpdateLocale(_localeData));
        });
    }
    private static void UpdateLocale(LocaleData _localeData, LocaleSettings settings = null)
    {
        Userspace.UserspaceWorld?.GetCoreLocale()?.Asset?.Data?.LoadDataAdditively(_localeData);
    }
}