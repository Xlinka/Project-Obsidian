using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrooxEngine;
using SkyFrost.Base;
using FrooxEngine.Store;
using System.IO;
namespace Obsidian.Shaders
{
    internal class ShaderInjection
    {

        public static readonly Uri ObsidianTestShader = new("resdb:///65178a8353f8c164cd2c6dc5d07ad3f978157b023f52331b4a0ca2e34781162e.unityshader");
        public static readonly Uri ParallaxOcclusion = new("resdb:///93ae78de262e31f8299661725899aac61d96071ac2ebea8b76a5c56febfc3feb.unityshader");

        private static readonly List<Uri> Shaders = new()
        {
            ObsidianTestShader,
            ParallaxOcclusion
        };
        public static string ExtractSignature(Uri uri)
        {
            if (uri.Scheme != "resdb")
            {
                throw new ArgumentException("Not a resdb URI");
            }
            string path = uri.Segments[1];
            return Path.GetFileNameWithoutExtension(path);
        }

        private static async Task RegisterShader(Uri uri)
        {
            var signature = ExtractSignature(uri);
            var shaderExists = await Engine.Current.LocalDB.ReadVariableAsync(signature, false);
            if (!shaderExists) await Engine.Current.LocalDB.WriteVariableAsync(signature, true);
        }


        public static void AppendShaders() => Task.WaitAll(Shaders.Select(shader => RegisterShader(shader)).ToArray());
    }
}