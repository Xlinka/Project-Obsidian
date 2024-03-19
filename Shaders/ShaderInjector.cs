using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrooxEngine;

namespace Obsidian.Shaders
{
    internal class ShaderInjection
    {
      

        private static readonly List<Uri> Shaders = new()
        {
            
        };

        private static async Task RegisterShader(Uri uri)
        {
            var signature = AssetUtil.ExtractSignature(uri);
            var shaderExists = await Engine.Current.LocalDB.ReadVariableAsync(signature, false);
            if (!shaderExists) await Engine.Current.LocalDB.WriteVariableAsync(signature, true);
        }

        public static void AppendShaders() => Task.WaitAll(Shaders.Select(shader => RegisterShader(shader)).ToArray());
    }
}