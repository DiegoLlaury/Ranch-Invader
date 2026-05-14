Shader "Custom/BlobShadow"
{
    Properties
    {
        _Opacity  ("Opacity",  Range(0, 1))    = 0.5
        _Softness ("Softness", Range(0.01, 1)) = 0.35
        _Color    ("Shadow Color", Color)      = (0, 0, 0, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "Queue"          = "Transparent-1"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest LEqual
        Cull Off
        Offset -1, -1  // Évite le zfighting avec le sol

        Pass
        {
            Name "BlobShadow"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            float  _Opacity;
            float  _Softness;
            float4 _Color;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Distance depuis le centre du quad (normalisée 0->1 au bord)
                float dist = length(IN.uv - 0.5) * 2.0;

                // Dégradé doux : plein au centre, transparent au bord
                float shadow = 1.0 - smoothstep(1.0 - _Softness, 1.0, dist);

                clip(shadow - 0.001);

                return half4(_Color.rgb, shadow * _Opacity);
            }
            ENDHLSL
        }
    }
}
