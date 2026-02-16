Shader "Custom/ImpostorBlended"
{
    Properties
    {
        _MainTex ("Texture A", 2D) = "white" {}
        _BlendTex ("Texture B", 2D) = "white" {}
        _BlendAmount ("Blend Amount", Range(0,1)) = 0
        _KeyColor ("Key Color", Color) = (0,0,1,1)
        _Threshold ("Key Threshold", Range(0,2)) = 0.5
        _Softness ("Edge Softness", Range(0,0.5)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BlendTex);
            SAMPLER(sampler_BlendTex);

            float4 _KeyColor;
            float _Threshold;
            float _Softness;
            float _BlendAmount;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // Échantillonner les deux textures
                half4 colA = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half4 colB = SAMPLE_TEXTURE2D(_BlendTex, sampler_BlendTex, IN.uv);

                // Mélanger les deux textures
                half4 col = lerp(colA, colB, _BlendAmount);

                // Appliquer le chroma key
                float chromaDist = distance(col.rgb, _KeyColor.rgb);
                float alpha = smoothstep(_Threshold - _Softness, _Threshold + _Softness, chromaDist);

                col.a *= alpha;
                clip(col.a - 0.01);

                return col;
            }
            ENDHLSL
        }
    }
}
