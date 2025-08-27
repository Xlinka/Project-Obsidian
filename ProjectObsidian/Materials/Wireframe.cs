using System;
using Elements.Core;
using FrooxEngine;
using Obsidian.Shaders;

[Category(new string[] { "Assets/Materials/Obsidian/Effects" })]
public class WireFrame : SingleShaderMaterialProvider
{
    protected override Uri ShaderURL => ShaderInjection.Wireframe;

    public readonly Sync<colorX> WireframeColor;
    public readonly Sync<colorX> BackgroundColor;

    private static MaterialProperty _WireframeColor = new MaterialProperty("_WireframeColor");
    private static MaterialProperty _BackgroundColor = new MaterialProperty("_BackgroundColor");

    [DefaultValue(-1)]
    public readonly Sync<int> RenderQueue;
    private static bool _propertiesInitialized;

    public override bool PropertiesInitialized
    {
        get => _propertiesInitialized;
        protected set => _propertiesInitialized = value;
    }

    protected override void UpdateMaterial(ref MaterialUpdateWriter material)
    {
        material.UpdateColor(_WireframeColor, WireframeColor);
        material.UpdateColor(_BackgroundColor, BackgroundColor);

        if (!RenderQueue.GetWasChangedAndClear()) return;
        var renderQueue = RenderQueue.Value;
        if ((int)RenderQueue == -1) renderQueue = 3500; // Transparent+1000
        material.SetRenderQueue(renderQueue);
    }

    protected override void UpdateKeywords(ShaderKeywords keywords) { }

    protected override void OnAttach()
    {
        base.OnAttach();
        WireframeColor.Value = new colorX(1, 1, 1, 1); // Default to white
        BackgroundColor.Value = new colorX(0, 0, 0, 1); // Default to black
    }
}
