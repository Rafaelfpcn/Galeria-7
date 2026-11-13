// Etapa 144: deixa a imagem com as poucas cores de um jogo de PS1. Cada canal de cor fica com _G7Niveis tons
// (32, como no PS1), e um pontilhado regular disfarça os degraus. Com _G7Niveis em zero, não faz nada.
// É usado por um passe de tela cheia posto no renderizador do URP pelo script Montar.
Shader "Galeria7/CoresDePS1"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off Cull Off ZTest Always

        Pass
        {
            Name "CoresDePS1"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #pragma vertex Vert
            #pragma fragment Frag

            float _G7Niveis;

            static const float Pontilhado[16] = { 0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5 };

            half4 Frag(Varyings entrada) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(entrada);
                half4 cor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, entrada.texcoord);
                if (_G7Niveis < 2.0) return cor;

                // A conta é feita na escala em que o olho enxerga (gama), senão os tons escuros somem.
                float3 gama = pow(saturate(cor.rgb), 1.0 / 2.2);
                uint2 pixel = uint2(entrada.positionCS.xy);
                float limiar = (Pontilhado[(pixel.x & 3) + (pixel.y & 3) * 4] + 0.5) / 16.0 - 0.5;
                gama = saturate(floor(gama * (_G7Niveis - 1.0) + 0.5 + limiar) / (_G7Niveis - 1.0));
                cor.rgb = pow(gama, 2.2);
                return cor;
            }
            ENDHLSL
        }
    }
}
