Shader "NeoCyber/SolidLightHologram"
{
    // =========================================================================
    // Autora / Author: Raquel D. Torres (El taller de la gatita tech)
    // Meta Global AI Developer Hackathon 2026 // Unity 6 URP (Quest 3S / Quest 3)
    // =========================================================================
    // [ES] Shader holografico para armas de luz solida (blasters, katanas, ballestas)
    // y nucleos enemigos. Unlit aditivo de un solo paso, 0 MB de texturas: ultra ligero
    // en Snapdragon XR2 Gen 2. El brillo perimetral se genera con termino Fresnel dependiente
    // del angulo de vision en vez de una textura pesada.
    // ---
    // [EN] Holographic shader for solid-light weaponry (energy blasters, katanas, crossbows)
    // and enemy cores. Unlit additive single-pass with zero texture VRAM cost: highly optimized
    // for Snapdragon XR2 Gen 2. Rim glow is dynamically computed via view-dependent Fresnel.
    // =========================================================================

    Properties
    {
        _BaseColor ("Core Color", Color) = (0, 1, 1, 1)
        _RimColor ("Rim Color", Color) = (1, 0, 1, 1)
        _RimPower ("Rim Power", Range(0.1, 8)) = 2.5
        _Intensity ("Intensity", Range(0, 10)) = 3
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 2
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.25
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Blend One One
        ZWrite Off
        ZTest LEqual
        Cull Back

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS    : TEXCOORD0;
                float3 viewDirWS   : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _RimColor;
                half _RimPower;
                half _Intensity;
                half _PulseSpeed;
                half _PulseAmount;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = posInputs.positionCS;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS = GetWorldSpaceViewDir(posInputs.positionWS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 normalWS = normalize(IN.normalWS);
                float3 viewDirWS = normalize(IN.viewDirWS);

                // Fresnel: 0 facing the camera dead-on, 1 at the silhouette edge.
                half fresnel = pow(saturate(1.0 - saturate(dot(normalWS, viewDirWS))), _RimPower);

                // Slow brightness pulse so held weapons and boss cores read as "alive".
                half pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseAmount;

                half3 coreColor = _BaseColor.rgb * (1.0 - fresnel);
                half3 rimColor = _RimColor.rgb * fresnel;
                half3 finalColor = (coreColor + rimColor) * _Intensity * pulse;

                // Additive blend (Blend One One) means alpha only affects how much
                // this pass adds; 1.0 keeps the full computed brightness.
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
