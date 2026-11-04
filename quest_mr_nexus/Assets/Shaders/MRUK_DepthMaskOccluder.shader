Shader "NeoCyber/MRUK_DepthMaskOccluder"
{
    // =========================================================================
    // Autora / Author: Raquel D. Torres (El taller de la gatita tech)
    // Meta Global AI Developer Hackathon 2026 // Unity 6 URP (Quest 3S / Quest 3)
    // =========================================================================
    // [ES] Oclusor invisible para la malla de habitacion escaneada por MRUK (EffectMesh).
    // Escribe unicamente en el buffer de profundidad (ColorMask 0) y se dibuja antes de
    // la geometria normal (Queue "Geometry-1"), permitiendo que el sofa, TV o pared real
    // oculten fisicamente cualquier elemento virtual renderizado detras en el mismo frame.
    // ---
    // [EN] Invisible depth occluder for MRUK's scanned room mesh (EffectMesh).
    // Writes depth only (ColorMask 0) and renders before normal geometry (Queue "Geometry-1"),
    // ensuring real physical objects (sofas, TVs, walls) occlude virtual content behind them.
    // =========================================================================

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry-1" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "DepthOnly"
            ColorMask 0
            ZWrite On
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Never actually seen: ColorMask 0 above discards the color write.
                return half4(0, 0, 0, 0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
