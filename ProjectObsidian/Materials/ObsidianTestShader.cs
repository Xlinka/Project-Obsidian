using System;
using Elements.Core;
using FrooxEngine;
using Obsidian.Shaders;

[Category(new string[] { "Assets/Materials/Obsidian" })]
public class TestMaterial : SingleShaderMaterialProvider
{
    protected override Uri ShaderURL => ShaderInjection.ObsidianTestShader;

    [DefaultValue(-1)]
    public readonly Sync<int> RenderQueue;

    private static PropertyState _propertyInitializationState;

    public override PropertyState PropertyInitializationState
    {
        get => _propertyInitializationState;
        protected set => _propertyInitializationState = value;
    }

    protected override void UpdateKeywords(ShaderKeywords keywords)
    {
        // No keywords to update for this simple shader
    }

    protected override void UpdateMaterial(Material material)
    {
        if (!RenderQueue.GetWasChangedAndClear()) return;

        var renderQueue = RenderQueue.Value;
        if (renderQueue == -1)
        {
            renderQueue = 2600;
        }

        material.SetRenderQueue(renderQueue);
    }
}
