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
       

        private static readonly List<Uri> Shaders = new()
        {
            
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