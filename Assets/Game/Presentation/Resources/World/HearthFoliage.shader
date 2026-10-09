Shader "Howl/Painted foliage"
{
    Properties
    {
        [MainTexture] _BaseMap("Painted leaves",2D)="white"{}
        _Cutoff("Leaf edge",Range(0,1))=.38
        _Wind("Breeze",Float)=.055
        _Desaturate("Blossom pigment",Float)=0
        _MapWidth("Map width",Float)=64
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        Cull Off
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
        CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_ST;
        float _Cutoff, _Wind, _MapWidth, _Desaturate;
        CBUFFER_END
        struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; half4 color:COLOR; };
        struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; float3 normalWS:TEXCOORD1; float3 positionWS:TEXCOORD2; half4 color:COLOR; half fog:TEXCOORD3; };
        float3 WindPosition(Attributes v)
        {
            float3 p=TransformObjectToWorld(v.positionOS.xyz);
            float mx=min(p.x,_MapWidth-p.x);
            float sway=(sin(_Time.y*1.2+mx*.61+p.z*.37)+.35*sin(_Time.y*2.1+p.y))*_Wind*v.color.a;
            p.x+=sway*(p.x>_MapWidth*.5?-1:1); p.z+=sway*.27;
            return p;
        }
        Varyings Vert(Attributes v)
        {
            Varyings o; o.positionWS=WindPosition(v); o.positionCS=TransformWorldToHClip(o.positionWS);
            o.normalWS=TransformObjectToWorldNormal(v.normalOS); o.uv=v.uv; o.color=v.color;
            o.fog=ComputeFogFactor(o.positionCS.z); return o;
        }
        ENDHLSL
        Pass
        {
            Name "Leaves" Tags { "LightMode"="UniversalForward" }
            AlphaToMask On
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            half4 Frag(Varyings i):SV_Target
            {
                half4 paint=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);
                clip(paint.a-_Cutoff);
                paint.rgb=lerp(paint.rgb,saturate(pow(dot(paint.rgb,half3(.25,.6,.15)),.55)*1.3).xxx,_Desaturate);
                half3 n=normalize(i.normalWS);
                Light sun=GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                half diffuse=saturate(dot(n,sun.direction)*.62+.38);
                half3 ambient=SampleSH(n)*.55+half3(.26,.28,.25);
                half shade=lerp(.57,1,sun.shadowAttenuation);
                half3 pigment=i.color.rgb;
                #if !defined(UNITY_COLORSPACE_GAMMA)
                pigment=SRGBToLinear(pigment);
                #endif
                half3 lit=paint.rgb*pigment*(ambient+sun.color*diffuse*.52*shade);
                return half4(MixFog(lit,i.fog),paint.a);
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster" Tags { "LightMode"="ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0
            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            float3 _LightDirection;
            Varyings ShadowVert(Attributes v)
            {
                Varyings o=Vert(v);
                float3 p=ApplyShadowBias(o.positionWS,o.normalWS,_LightDirection);
                o.positionCS=TransformWorldToHClip(p);
                #if UNITY_REVERSED_Z
                o.positionCS.z=min(o.positionCS.z,UNITY_NEAR_CLIP_VALUE);
                #else
                o.positionCS.z=max(o.positionCS.z,UNITY_NEAR_CLIP_VALUE);
                #endif
                return o;
            }
            half4 ShadowFrag(Varyings i):SV_Target { clip(SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).a-_Cutoff);return 0; }
            ENDHLSL
        }
    }
}
