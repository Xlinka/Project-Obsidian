Shader "ObsidianPlus/ObsidianTestShader"
{
    SubShader
    {
        // This shader renders objects with an opaque render type.
        // It transforms vertex positions to clip space and calculates the view direction.
        // The fragment shader outputs the view direction as the RGB color with alpha set to 1.
        // Created by LeCloutPanda

        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 viewDir : TEXCOORD3;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return float4(i.viewDir, 1);
            }
            ENDCG
        }
    }
}
