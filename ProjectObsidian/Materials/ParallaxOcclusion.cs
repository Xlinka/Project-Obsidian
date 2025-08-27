using System;
using Elements.Core;
using FrooxEngine;
using Obsidian.Shaders;

[Category(new string[] { "Assets/Materials/Obsidian/Effects" })]
public class ParallaxOcclusion : SingleShaderMaterialProvider
{
    protected override Uri ShaderURL => ShaderInjection.ParallaxOcclusion;

    public readonly Sync<colorX> Color;
    public readonly AssetRef<ITexture2D> MainTex;
    public readonly Sync<float> TextureScale;
    public readonly AssetRef<ITexture2D> NormalMap;
    [Range(0f, 1f, "0.00")]
    public readonly Sync<float> NormalScale;
    public readonly AssetRef<ITexture2D> ParallaxMap;
    [Range(0f, 1f, "0.00")]
    public readonly Sync<float> Parallax;
    [Range(0f, 1f, "0.00")]
    public readonly Sync<float> Glossiness;
    [Range(0f, 1f, "0.00")]
    public readonly Sync<float> Metallic;
    [Range(2f, 100f, "0")]
    public readonly Sync<float> ParallaxMinSamples;
    [Range(2f, 100f, "0")]
    public readonly Sync<float> ParallaxMaxSamples;
    [Range(0f, 1f, "0.00")]
    public readonly Sync<float> AlphaCutoff;

    private static MaterialProperty _Color = new MaterialProperty("_Color");
    private static MaterialProperty _MainTex = new MaterialProperty("_MainTex");
    private static MaterialProperty _TextureScale = new MaterialProperty("_TextureScale");
    private static MaterialProperty _NormalMap = new MaterialProperty("_BumpMap");
    private static MaterialProperty _NormalScale = new MaterialProperty("_BumpScale");
    private static MaterialProperty _ParallaxMap = new MaterialProperty("_ParallaxMap");
    private static MaterialProperty _Parallax = new MaterialProperty("_Parallax");
    private static MaterialProperty _Glossiness = new MaterialProperty("_Glossiness");
    private static MaterialProperty _Metallic = new MaterialProperty("_Metallic");
    private static MaterialProperty _ParallaxMinSamples = new MaterialProperty("_ParallaxMinSamples");
    private static MaterialProperty _ParallaxMaxSamples = new MaterialProperty("_ParallaxMaxSamples");
    private static MaterialProperty _AlphaCutoff = new MaterialProperty("_AlphaCutoff");

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
        material.UpdateColor(_Color, Color);
        material.UpdateTexture(_MainTex, MainTex);
        material.UpdateFloat(_TextureScale, TextureScale);
        material.UpdateTexture(_NormalMap, NormalMap);
        material.UpdateFloat(_NormalScale, NormalScale);
        material.UpdateTexture(_ParallaxMap, ParallaxMap);
        material.UpdateFloat(_Parallax, Parallax);
        material.UpdateFloat(_Glossiness, Glossiness);
        material.UpdateFloat(_Metallic, Metallic);
        material.UpdateFloat(_ParallaxMinSamples, ParallaxMinSamples);
        material.UpdateFloat(_ParallaxMaxSamples, ParallaxMaxSamples);
        material.UpdateFloat(_AlphaCutoff, AlphaCutoff);

        if (!RenderQueue.GetWasChangedAndClear()) return;
        var renderQueue = RenderQueue.Value;
        if ((int)RenderQueue == -1) renderQueue = 2000;
        material.SetRenderQueue(renderQueue);
    }

    protected override void UpdateKeywords(ShaderKeywords keywords) { }

    protected override void OnAttach()
    {
        base.OnAttach();
        Color.Value = colorX.White;
        TextureScale.Value = 1f;
        NormalScale.Value = 1f;
        Parallax.Value = 0.05f;
        Glossiness.Value = 0.5f;
        Metallic.Value = 0f;
        ParallaxMinSamples.Value = 4;
        ParallaxMaxSamples.Value = 20;
        AlphaCutoff.Value = 0.5f;
    }
}
