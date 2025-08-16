using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using Obsidian.Shaders;
using System.Reflection;
using System.IO;

namespace Obsidian
{
    public class ExecutionHook : IPlatformConnector
    {

#pragma warning disable CS1591
        public PlatformInterface Platform { get; private set; }
        public int Priority => -10;
        public string PlatformName => "Project Obsidian";
        public string Username => null;
        public string PlatformUserId => null;
        public bool IsPlatformNameUnique => false;
        public void SetCurrentStatus(World world, bool isPrivate, int totalWorldCount) { }
        public void ClearCurrentStatus() { }
        public void Update() { }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        public void NotifyOfLocalUser(User user) { }
        public void NotifyOfFile(string file, string name) { }
        public void NotifyOfScreenshot(World world, string file, ScreenshotType type, DateTime timestamp) { }

        public async Task<bool> Initialize(PlatformInterface platformInterface)
        {
            UniLog.Log("Initialize() from platformInterface");
            Platform = platformInterface;
            return true;
        }
#pragma warning restore CS1591

#pragma warning disable CA2255
        [ModuleInitializer]
        public static void Init()
        {
            UniLog.Log("Init() from ModuleInitializer");
        }
#pragma warning restore CA2255
        static ExecutionHook()
        {
            UniLog.Log($"Start of ExecutionHook");
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
    }
}
