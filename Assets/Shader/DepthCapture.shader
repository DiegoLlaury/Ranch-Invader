Shader "Hidden/DepthCapture"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        
        Pass
        {
            Name "DepthCapture"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float depth : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                float3 positionVS = TransformWorldToView(TransformObjectToWorld(IN.positionOS.xyz));
                OUT.depth = -positionVS.z;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float normalizedDepth = saturate(IN.depth / 10.0);
                return half4(normalizedDepth, normalizedDepth, normalizedDepth, 1);
            }
            ENDHLSL
        }
    }
}
