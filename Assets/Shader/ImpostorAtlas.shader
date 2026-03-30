Shader "Custom/ImpostorAtlas"
{
    Properties
    {
        _MainTex ("Atlas", 2D) = "white" {}
        _Direction ("Direction", Float) = 0
        _Columns ("Columns", Float) = 4
        _Rows ("Rows", Float) = 2
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

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

            float _Direction;
            float _Columns;
            float _Rows;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                int index = (int)_Direction;

                int col = index % (int)_Columns;
                int row = index / (int)_Columns;

                float2 atlasUV = IN.uv;

                atlasUV.x = (atlasUV.x + col) / _Columns;
                atlasUV.y = (atlasUV.y + (_Rows - 1 - row)) / _Rows;

                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, atlasUV);
            }

            ENDHLSL
        }
    }
}