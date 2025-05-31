using System;
using System.Collections.Generic;
using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using Obsidian.Shaders;

namespace Obsidian
{
    [ImplementableClass(true)]
    internal static class ExecutionHook
    {
        // Fields for reflective access
        private static Type? __connectorType;
        private static Type? __connectorTypes;

        // Static constructor for initializing the hook
        static ExecutionHook()
        {
            try
            {
                Engine.Current.OnReady += () =>
                {
                    ShaderInjection.AppendShaders();
                };
            }
            catch (Exception e)
            {
                UniLog.Log($"Exception thrown during initialization: {e}");
            }
        }

        // Method to instantiate the connector
        private static DummyConnector InstantiateConnector() => new DummyConnector();

        // Dummy connector class implementing IConnector
        private class DummyConnector : IConnector
        {
            public IImplementable? Owner { get; private set; }

            public void ApplyChanges() { }

            public void AssignOwner(IImplementable owner) => Owner = owner;

            public void Destroy(bool destroyingWorld) { }

            public void Initialize() { }

            public void RemoveOwner() => Owner = null;
        }
    }
}
