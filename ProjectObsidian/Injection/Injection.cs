using System;
using System.Linq;
using System.Reflection;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux.Core;
using Obsidian.Shaders;
using System.Collections.Generic;

namespace Obsidian
{
    [ImplementableClass(true)]
    internal static class ExecutionHook
    {
        // Fields for reflective access
        private static Type? __connectorType;
        private static Type? __connectorTypes;

        private static AssemblyTypeRegistry obsidianRegistry;
        private static bool registered = true;

        // Static constructor for initializing the hook
        static ExecutionHook()
        {
            try
            {
                Engine.Current.OnReady += () =>
                {
                    ShaderInjection.AppendShaders();
                    DevCreateNewForm.AddAction("Plugins", "Register/Unregister Obsidian Assembly", (Slot s) => 
                    { 
                        s.Destroy();
                        var glob = (List<AssemblyTypeRegistry>)typeof(GlobalTypeRegistry).GetField("_coreAssemblies", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                        if (registered)
                        {
                            foreach (var thing in glob.ToList())
                            {
                                if (thing.Assembly == Assembly.GetExecutingAssembly())
                                {
                                    obsidianRegistry = thing;
                                    glob.Remove(thing);
                                }
                            }
                            registered = false;
                        }
                        else
                        {
                            glob.Add(obsidianRegistry);
                            registered = true;
                        }
                    });
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
