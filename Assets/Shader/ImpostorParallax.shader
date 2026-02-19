Shader "Custom/ImpostorParallax"
{
    Properties
    {
        _MainTex ("Color Texture", 2D) = "white" {}
        _DepthTex ("Depth Texture", 2D) = "white" {}
        _BlendTex ("Blend Color Texture", 2D) = "white" {}
        _DepthBlendTex ("Blend Depth Texture", 2D) = "white" {}
        _BlendAmount ("Blend Amount", Range(0,1)) = 0
        
        _ParallaxStrength ("Parallax Strength", Range(0, 0.1)) = 0.03
        _ParallaxMinSamples ("Min Samples", Range(4, 32)) = 8
        _ParallaxMaxSamples ("Max Samples", Range(4, 64)) = 32
        
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
            "RenderPipeline"="UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ParallaxImpostor"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewDirTS : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_DepthTex);
            SAMPLER(sampler_DepthTex);
            TEXTURE2D(_BlendTex);
            SAMPLER(sampler_BlendTex);
            TEXTURE2D(_DepthBlendTex);
            SAMPLER(sampler_DepthBlendTex);

            CBUFFER_START(UnityPerMaterial)
            float4 _KeyColor;
            float _Threshold;
            float _Softness;
            float _BlendAmount;
            float _ParallaxStrength;
            float _ParallaxMinSamples;
            float _ParallaxMaxSamples;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;

                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 viewDirWS = normalize(GetCameraPositionWS() - positionWS);

                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
                float3 tangentWS = TransformObjectToWorldDir(IN.tangentOS.xyz);
                float3 bitangentWS = cross(normalWS, tangentWS) * IN.tangentOS.w;

                float3x3 tangentToWorld = float3x3(tangentWS, bitangentWS, normalWS);
                OUT.viewDirTS = mul(tangentToWorld, viewDirWS);

                return OUT;
            }

            float2 ParallaxOcclusionMapping(float2 uv, float3 viewDirTS, TEXTURE2D(depthMap), SAMPLER(samplerDepthMap))
            {
                float numSamples = lerp(_ParallaxMaxSamples, _ParallaxMinSamples, saturate(dot(viewDirTS, float3(0, 0, 1))));
                
                float layerHeight = 1.0 / numSamples;
                float currentLayerHeight = 0.0;
                
                float2 dtex = _ParallaxStrength * viewDirTS.xy / viewDirTS.z / numSamples;
                float2 currentUV = uv;
                
                float currentDepthValue = SAMPLE_TEXTURE2D(depthMap, samplerDepthMap, currentUV).r;
                
                [loop]
                for(int i = 0; i < (int)numSamples && currentLayerHeight < currentDepthValue; i++)
                {
                    currentUV -= dtex;
                    currentDepthValue = SAMPLE_TEXTURE2D(depthMap, samplerDepthMap, currentUV).r;
                    currentLayerHeight += layerHeight;
                }
                
                float2 prevUV = currentUV + dtex;
                float nextDepth = currentDepthValue - currentLayerHeight;
                float prevDepth = SAMPLE_TEXTURE2D(depthMap, samplerDepthMap, prevUV).r - (currentLayerHeight - layerHeight);
                
                float weight = nextDepth / (nextDepth - prevDepth);
                return lerp(currentUV, prevUV, weight);
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float2 uv1 = ParallaxOcclusionMapping(IN.uv, IN.viewDirTS, _DepthTex, sampler_DepthTex);
                half4 col1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv1);

                float chromaDist1 = distance(col1.rgb, _KeyColor.rgb);
                float alpha1 = smoothstep(_Threshold - _Softness, _Threshold + _Softness, chromaDist1);
                col1.a *= alpha1;

                if (_BlendAmount > 0.01)
                {
                    float2 uv2 = ParallaxOcclusionMapping(IN.uv, IN.viewDirTS, _DepthBlendTex, sampler_DepthBlendTex);
                    half4 col2 = SAMPLE_TEXTURE2D(_BlendTex, sampler_BlendTex, uv2);
                    
                    float chromaDist2 = distance(col2.rgb, _KeyColor.rgb);
                    float alpha2 = smoothstep(_Threshold - _Softness, _Threshold + _Softness, chromaDist2);
                    col2.a *= alpha2;
                    
                    col1 = lerp(col1, col2, _BlendAmount);
                }

                clip(col1.a - 0.01);

                return col1;
            }
            ENDHLSL
        }
    }
}
