using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FrooxEngine;
using SkyFrost.Base;

namespace Obsidian.Shaders
{
    internal class ShaderInjection
    {
      

        private static readonly List<Uri> Shaders = new()
        {
            
        };

        private static async Task RegisterShader(Uri uri)
        {
            var signature = ExtractSignature(uri);
            var shaderExists = await Engine.Current.LocalDB.ReadVariableAsync(signature, false);
            if (!shaderExists) await Engine.Current.LocalDB.WriteVariableAsync(signature, true);
        }

        private static string ExtractSignature(Uri uri)
        {
            string extension;
            return ExtractSignature(uri, out extension);
        }
        public static string ExtractSignature(Uri uri, out string extension)
        {
            if (uri.Scheme != "resdb")
            {
                throw new ArgumentException("Not a Resonite DB URI");
            }
            string path = uri.Segments[1];
            extension = Path.GetExtension(path);
            return Path.GetFileNameWithoutExtension(path);
        }

        public static void AppendShaders() => Task.WaitAll(Shaders.Select(shader => RegisterShader(shader)).ToArray());
    }

}