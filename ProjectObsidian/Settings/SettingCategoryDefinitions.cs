using Elements.Core;
using FrooxEngine;
using Elements.Data;

// NEEDS TO BE IN GLOBAL NAMESPACE!

[DataModelType]
public static class SettingCategoryDefinitions
{
    public static SettingCategoryInfo Obsidian
    {
        get
        {
            return new SettingCategoryInfo(new System.Uri("https://static.wikia.nocookie.net/minecraft_gamepedia/images/9/99/Obsidian_JE3_BE2.png/revision/latest?cb=20200124042057"), 0L);
        }
    }
}