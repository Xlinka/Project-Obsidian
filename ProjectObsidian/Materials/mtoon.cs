using System;
using Elements.Core;
using FrooxEngine;
using Obsidian.Shaders;

[Category(new string[] { "Assets/Materials/Obsidian/Toon" })]
public class MToonMaterial : SingleShaderMaterialProvider
{
    protected override Uri ShaderURL => ShaderInjection.Mtoon;

    public readonly Sync<float> AlphaCutoff;
    public readonly Sync<colorX> LitColorAlpha;
    public readonly Sync<colorX> ShadeColor;
    public readonly AssetRef<ITexture2D> LitTextureAlpha;
    public readonly AssetRef<ITexture2D> ShadeTexture;
    public readonly Sync<float> NormalScale;
    public readonly AssetRef<ITexture2D> NormalTexture;
    public readonly Sync<float> ReceiveShadow;
    public readonly AssetRef<ITexture2D> ReceiveShadowTexture;
    public readonly Sync<float> ShadingGrade;
    public readonly AssetRef<ITexture2D> ShadingGradeTexture;
    public readonly Sync<float> ShadeShift;
    public readonly Sync<float> ShadeToony;
    public readonly Sync<float> LightColorAttenuation;
    public readonly Sync<float> IndirectLightIntensity;
    public readonly Sync<colorX> RimColor;
    public readonly AssetRef<ITexture2D> RimTexture;
    public readonly Sync<float> RimLightingMix;
    public readonly Sync<float> RimFresnelPower;
    public readonly Sync<float> RimLift;
    public readonly AssetRef<ITexture2D> SphereTextureAdd;
    public readonly Sync<colorX> EmissionColor;
    public readonly AssetRef<ITexture2D> Emission;
    public readonly AssetRef<ITexture2D> OutlineWidthTex;
    public readonly Sync<float> OutlineWidth;
    public readonly Sync<float> OutlineScaledMaxDistance;
    public readonly Sync<colorX> OutlineColor;
    public readonly Sync<float> OutlineLightingMix;
    public readonly AssetRef<ITexture2D> UVAnimationMask;
    public readonly Sync<float> UVAnimationScrollX;
    public readonly Sync<float> UVAnimationScrollY;
    public readonly Sync<float> UVAnimationRotation;

    private static MaterialProperty _AlphaCutoff = new MaterialProperty("_Cutoff");
    private static MaterialProperty _LitColorAlpha = new MaterialProperty("_Color");
    private static MaterialProperty _ShadeColor = new MaterialProperty("_ShadeColor");
    private static MaterialProperty _LitTextureAlpha = new MaterialProperty("_MainTex");
    private static MaterialProperty _ShadeTexture = new MaterialProperty("_ShadeTexture");
    private static MaterialProperty _NormalScale = new MaterialProperty("_BumpScale");
    private static MaterialProperty _NormalTexture = new MaterialProperty("_BumpMap");
    private static MaterialProperty _ReceiveShadow = new MaterialProperty("_ReceiveShadowRate");
    private static MaterialProperty _ReceiveShadowTexture = new MaterialProperty("_ReceiveShadowTexture");
    private static MaterialProperty _ShadingGrade = new MaterialProperty("_ShadingGradeRate");
    private static MaterialProperty _ShadingGradeTexture = new MaterialProperty("_ShadingGradeTexture");
    private static MaterialProperty _ShadeShift = new MaterialProperty("_ShadeShift");
    private static MaterialProperty _ShadeToony = new MaterialProperty("_ShadeToony");
    private static MaterialProperty _LightColorAttenuation = new MaterialProperty("_LightColorAttenuation");
    private static MaterialProperty _IndirectLightIntensity = new MaterialProperty("_IndirectLightIntensity");
    private static MaterialProperty _RimColor = new MaterialProperty("_RimColor");
    private static MaterialProperty _RimTexture = new MaterialProperty("_RimTexture");
    private static MaterialProperty _RimLightingMix = new MaterialProperty("_RimLightingMix");
    private static MaterialProperty _RimFresnelPower = new MaterialProperty("_RimFresnelPower");
    private static MaterialProperty _RimLift = new MaterialProperty("_RimLift");
    private static MaterialProperty _SphereTextureAdd = new MaterialProperty("_SphereAdd");
    private static MaterialProperty _EmissionColor = new MaterialProperty("_EmissionColor");
    private static MaterialProperty _Emission = new MaterialProperty("_EmissionMap");
    private static MaterialProperty _OutlineWidthTex = new MaterialProperty("_OutlineWidthTexture");
    private static MaterialProperty _OutlineWidth = new MaterialProperty("_OutlineWidth");
    private static MaterialProperty _OutlineScaledMaxDistance = new MaterialProperty("_OutlineScaledMaxDistance");
    private static MaterialProperty _OutlineColor = new MaterialProperty("_OutlineColor");
    private static MaterialProperty _OutlineLightingMix = new MaterialProperty("_OutlineLightingMix");
    private static MaterialProperty _UVAnimationMask = new MaterialProperty("_UvAnimMaskTexture");
    private static MaterialProperty _UVAnimationScrollX = new MaterialProperty("_UvAnimScrollX");
    private static MaterialProperty _UVAnimationScrollY = new MaterialProperty("_UvAnimScrollY");
    private static MaterialProperty _UVAnimationRotation = new MaterialProperty("_UvAnimRotation");

    [DefaultValue(-1)]
    public readonly Sync<int> RenderQueue;
    private static PropertyState _propertyInitializationState;

    public override PropertyState PropertyInitializationState
    {
        get => _propertyInitializationState;
        protected set => _propertyInitializationState = value;
    }

    protected override void UpdateMaterial(Material material)
    {
        material.UpdateFloat(_AlphaCutoff, AlphaCutoff);
        material.UpdateColor(_LitColorAlpha, LitColorAlpha);
        material.UpdateColor(_ShadeColor, ShadeColor);
        material.UpdateTexture(_LitTextureAlpha, LitTextureAlpha);
        material.UpdateTexture(_ShadeTexture, ShadeTexture);
        material.UpdateFloat(_NormalScale, NormalScale);
        material.UpdateTexture(_NormalTexture, NormalTexture);
        material.UpdateFloat(_ReceiveShadow, ReceiveShadow);
        material.UpdateTexture(_ReceiveShadowTexture, ReceiveShadowTexture);
        material.UpdateFloat(_ShadingGrade, ShadingGrade);
        material.UpdateTexture(_ShadingGradeTexture, ShadingGradeTexture);
        material.UpdateFloat(_ShadeShift, ShadeShift);
        material.UpdateFloat(_ShadeToony, ShadeToony);
        material.UpdateFloat(_LightColorAttenuation, LightColorAttenuation);
        material.UpdateFloat(_IndirectLightIntensity, IndirectLightIntensity);
        material.UpdateColor(_RimColor, RimColor);
        material.UpdateTexture(_RimTexture, RimTexture);
        material.UpdateFloat(_RimLightingMix, RimLightingMix);
        material.UpdateFloat(_RimFresnelPower, RimFresnelPower);
        material.UpdateFloat(_RimLift, RimLift);
        material.UpdateTexture(_SphereTextureAdd, SphereTextureAdd);
        material.UpdateColor(_EmissionColor, EmissionColor);
        material.UpdateTexture(_Emission, Emission);
        material.UpdateTexture(_OutlineWidthTex, OutlineWidthTex);
        material.UpdateFloat(_OutlineWidth, OutlineWidth);
        material.UpdateFloat(_OutlineScaledMaxDistance, OutlineScaledMaxDistance);
        material.UpdateColor(_OutlineColor, OutlineColor);
        material.UpdateFloat(_OutlineLightingMix, OutlineLightingMix);
        material.UpdateTexture(_UVAnimationMask, UVAnimationMask);
        material.UpdateFloat(_UVAnimationScrollX, UVAnimationScrollX);
        material.UpdateFloat(_UVAnimationScrollY, UVAnimationScrollY);
        material.UpdateFloat(_UVAnimationRotation, UVAnimationRotation);

        if (!RenderQueue.GetWasChangedAndClear()) return;
        var renderQueue = RenderQueue.Value;
        if ((int)RenderQueue == -1) renderQueue = 2000;
        material.SetRenderQueue(renderQueue);
    }

    protected override void UpdateKeywords(ShaderKeywords keywords) { }

    protected override void OnAttach()
    {
        base.OnAttach();
        AlphaCutoff.Value = 0.5f;
        LitColorAlpha.Value = new colorX(1, 1, 1, 1);
        ShadeColor.Value = new colorX(0.97f, 0.81f, 0.86f, 1);
        NormalScale.Value = 1.0f;
        ReceiveShadow.Value = 1;
        ShadingGrade.Value = 1;
        ShadeShift.Value = 0;
        ShadeToony.Value = 0.9f;
        LightColorAttenuation.Value = 0;
        IndirectLightIntensity.Value = 0.1f;
        RimColor.Value = new colorX(0, 0, 0);
        RimLightingMix.Value = 0;
        RimFresnelPower.Value = 1;
        RimLift.Value = 0;
        EmissionColor.Value = new colorX(0, 0, 0);
        OutlineWidth.Value = 0.5f;
        OutlineScaledMaxDistance.Value = 1;
        OutlineColor.Value = new colorX(0, 0, 0, 1);
        OutlineLightingMix.Value = 1;
        UVAnimationScrollX.Value = 0;
        UVAnimationScrollY.Value = 0;
        UVAnimationRotation.Value = 0;
    }
}
