using System;
using Elements.Core;
using FrooxEngine;
using Obsidian.Shaders;

[Category(new string[] { "Assets/Materials/Obsidian/Effects" })]
public class CrystalShader : SingleShaderMaterialProvider
{
    protected override Uri ShaderURL => ShaderInjection.Crystal;

    public readonly AssetRef<ITexture2D> BaseColortex;
    public readonly Sync<colorX> BaseColor;
    public readonly AssetRef<ITexture2D> SurfaceColorTex;
    public readonly Sync<colorX> SurfaceColor;
    public readonly AssetRef<ITexture2D> Normal;
    public readonly AssetRef<ITexture2D> Alpha;
    [Range(0f, 1f, "0.00")]
    public readonly Sync<float> Metallic;
    [Range(0f, 1f, "0.00")]
    public readonly Sync<float> Gloss;
    [Range(1f, 20f, "0.00")]
    public readonly Sync<float> Repetition;
    [Range(1f, 5f, "0.00")]
    public readonly Sync<float> ColorLoop;
    [Range(0f, 1f, "0.00")]
    public readonly Sync<float> Width;
    [Range(0f, 1f, "0.00")]
    public readonly Sync<float> ColorLevel;
    [Range(0f, 1f, "0.00")]
    public readonly Sync<float> PastelColor;
    [Range(0f, 10f, "0.00")]
    public readonly Sync<float> Distortion;
    [Range(-1f, 1f, "0.00")]
    public readonly Sync<float> ChromaticAberration;
    [Range(-1f, 1f, "0.00")]
    public readonly Sync<float> LightCompletion;

    private static MaterialProperty _BaseColortex = new MaterialProperty("_BaseColortex");
    private static MaterialProperty _BaseColor = new MaterialProperty("_BaseColor");
    private static MaterialProperty _SurfaceColorTex = new MaterialProperty("_SurfaceColorTex");
    private static MaterialProperty _SurfaceColor = new MaterialProperty("_SurfaceColor");
    private static MaterialProperty _Normal = new MaterialProperty("_Normal");
    private static MaterialProperty _Alpha = new MaterialProperty("_alpha");
    private static MaterialProperty _Metallic = new MaterialProperty("_Metallic");
    private static MaterialProperty _Gloss = new MaterialProperty("_Gloss");
    private static MaterialProperty _Repetition = new MaterialProperty("_Repetition");
    private static MaterialProperty _ColorLoop = new MaterialProperty("_ColorLoop");
    private static MaterialProperty _Width = new MaterialProperty("_Width");
    private static MaterialProperty _ColorLevel = new MaterialProperty("_ColorLevel");
    private static MaterialProperty _PastelColor = new MaterialProperty("_PastelColor");
    private static MaterialProperty _Distortion = new MaterialProperty("_Distortion");
    private static MaterialProperty _ChromaticAberration = new MaterialProperty("_ChromaticAberration");
    private static MaterialProperty _LightCompletion = new MaterialProperty("_LightCompletion");

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
        material.UpdateTexture(_BaseColortex, BaseColortex);
        material.UpdateColor(_BaseColor, BaseColor);
        material.UpdateTexture(_SurfaceColorTex, SurfaceColorTex);
        material.UpdateColor(_SurfaceColor, SurfaceColor);
        material.UpdateTexture(_Normal, Normal);
        material.UpdateTexture(_Alpha, Alpha);
        material.UpdateFloat(_Metallic, Metallic);
        material.UpdateFloat(_Gloss, Gloss);
        material.UpdateFloat(_Repetition, Repetition);
        material.UpdateFloat(_ColorLoop, ColorLoop);
        material.UpdateFloat(_Width, Width);
        material.UpdateFloat(_ColorLevel, ColorLevel);
        material.UpdateFloat(_PastelColor, PastelColor);
        material.UpdateFloat(_Distortion, Distortion);
        material.UpdateFloat(_ChromaticAberration, ChromaticAberration);
        material.UpdateFloat(_LightCompletion, LightCompletion);

        if (!RenderQueue.GetWasChangedAndClear()) return;
        var renderQueue = RenderQueue.Value;
        if ((int)RenderQueue == -1) renderQueue = 3000; // Transparent render queue
        material.SetRenderQueue(renderQueue);
    }

    protected override void UpdateKeywords(ShaderKeywords keywords) { }

    protected override void OnAttach()
    {
        base.OnAttach();
        BaseColor.Value = new colorX(1, 1, 1, 1); // Default to white
        SurfaceColor.Value = new colorX(0, 0, 0, 1); // Default to black
        Metallic.Value = 0.5f;
        Gloss.Value = 0.9f;
        Repetition.Value = 10;
        ColorLoop.Value = 1;
        Width.Value = 0.5f;
        ColorLevel.Value = 0.5f;
        PastelColor.Value = 0.3f;
        Distortion.Value = 5f;
        ChromaticAberration.Value = 0f;
        LightCompletion.Value = 1f;
    }
}
