Shader "Custom/UI/BeerWave"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Wave Surface)]
        _WaveAmplitude ("Wave Amplitude",  Range(0, 0.12)) = 0.03
        _WaveFrequency ("Wave Frequency",  Range(1, 20))   = 5.0
        _WaveSpeed     ("Wave Speed",      Range(0, 10))   = 2.5
        _WaveSoftness  ("Edge Softness",   Range(0.001, 0.1)) = 0.02

        [Header(Required by Unity UI)]
        _StencilComp      ("Stencil Comparison", Float) = 8
        _Stencil          ("Stencil ID",         Float) = 0
        _StencilOp        ("Stencil Operation",  Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask  ("Stencil Read Mask",  Float) = 255
        _ColorMask        ("Color Mask",         Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref       [_Stencil]
            Comp      [_StencilComp]
            Pass      [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target   2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;
            fixed4    _Color;
            fixed4    _TextureSampleAdd;
            float4    _ClipRect;

            float _WaveAmplitude;
            float _WaveFrequency;
            float _WaveSpeed;
            float _WaveSoftness;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPos = v.vertex;
                OUT.vertex   = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color    = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;

                // --- Surface de vague sur le bord SUPÉRIEUR ---
                // Calcule la hauteur de la surface en UV pour chaque colonne X
                // sin() oscille entre -1 et +1, centré sur le haut de la texture (uv.y = 1)
                float waveSurface = 1.0 - _WaveAmplitude
                                  + sin(uv.x * _WaveFrequency * 6.2831853 + _Time.y * _WaveSpeed)
                                  * _WaveAmplitude;

                // smoothstep : pixels SOUS la surface = opaques, AU-DESSUS = transparents
                // Cela crée une frontière douce et sinusoïdale au sommet de la texture
                float waveMask = smoothstep(waveSurface + _WaveSoftness,
                                            waveSurface - _WaveSoftness,
                                            uv.y);

                // Échantillonne la texture normalement, sans déformer les UVs
                fixed4 color = (tex2D(_MainTex, uv) + _TextureSampleAdd) * IN.color;

                // Applique le masque de vague sur l'alpha
                color.a *= waveMask;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPos.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDHLSL
        }
    }
}
