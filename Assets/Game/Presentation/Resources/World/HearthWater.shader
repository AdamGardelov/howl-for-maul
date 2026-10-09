Shader "Howl/Sheltered water"
{
    Properties {
        [MainTexture] _BaseMap("Pool pigment",2D)="white"{}
        _MapWidth("Map width",Float)=64
        _Ice("Glacial water",Float)=0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST; float _MapWidth,_Ice;
            CBUFFER_END
            struct A { float4 positionOS:POSITION;float2 uv:TEXCOORD0; };
            struct V { float4 positionCS:SV_POSITION;float3 positionWS:TEXCOORD0;float2 uv:TEXCOORD1;half fog:TEXCOORD2; };
            V Vert(A v){V o;o.positionWS=TransformObjectToWorld(v.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);o.uv=v.uv;o.fog=ComputeFogFactor(o.positionCS.z);return o;}
            half4 Frag(V i):SV_Target
            {
                float x=min(i.positionWS.x,_MapWidth-i.positionWS.x),z=i.positionWS.z;
                float time=_Time.y*lerp(.22,.10,_Ice);
                float a=sin(x*1.5+z*.66+sin(z*.35+time)*.8+time);
                float b=sin(z*1.35-x*.47+sin(x*.5-time)*.6-time);
                half shimmer=pow(saturate(a*b),12);
                half3 baseColor=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).rgb;
                half3 sky=lerp(half3(.19,.34,.32),half3(.19,.36,.43),_Ice);
                half3 color=baseColor*(.92+.055*a)+sky*(.14+shimmer*.19);
                return half4(MixFog(color,i.fog),1);
            }
            ENDHLSL
        }
    }
}
