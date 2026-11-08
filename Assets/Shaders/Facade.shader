// One material for all roadside scenery (urban-spec 1.4): facade tiles from a Texture2DArray (slice in vertex colour alpha / 32),
// vertex colour as tint (solid colours use the plain slice 7), main light + ambient, fog. Opaque, no shadows cast or received.
Shader "Geisterfahrer/Facade"
{
    Properties { _Slices("Facade slices", 2DArray) = "" {} }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D_ARRAY(_Slices);
            SAMPLER(sampler_Slices);
            CBUFFER_START(UnityPerMaterial)
                float4 _Slices_ST;
            CBUFFER_END

            struct Attributes { float4 pos : POSITION; float3 nrm : NORMAL; float2 uv : TEXCOORD0; half4 col : COLOR; };
            struct Varyings { float4 pos : SV_POSITION; float3 nrm : TEXCOORD0; float3 uvs : TEXCOORD1; half3 tint : TEXCOORD2; half fog : TEXCOORD3; };

            Varyings vert(Attributes a)
            {
                Varyings o;
                o.pos = TransformObjectToHClip(a.pos.xyz);
                o.nrm = TransformObjectToWorldNormal(a.nrm);
                o.uvs = float3(a.uv, round(a.col.a * 255.0 / 32.0));
                o.tint = a.col.rgb;
                o.fog = ComputeFogFactor(o.pos.z);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 t = SAMPLE_TEXTURE2D_ARRAY(_Slices, sampler_Slices, i.uvs.xy, i.uvs.z);
                float3 n = normalize(i.nrm);
                Light l = GetMainLight();
                half3 lit = l.color * saturate(dot(n, l.direction)) + SampleSH(n);
                return half4(MixFog(t.rgb * i.tint * lit, i.fog), 1);
            }
            ENDHLSL
        }
    }
}
