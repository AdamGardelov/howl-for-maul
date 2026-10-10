Shader "Howl/Painted ridge"
{
    Properties
    {
        [MainTexture] _BaseMap("Existing shelf paint",2D)="white"{}
        _RockMap("Exposed stone",2D)="white"{}
        _RockTint("Stone pigment",Color)=(.77,.85,.82,1)
        _MapHeight("Map height",Float)=64
        _Cutoff("Surface cutoff",Range(0,1))=.38
        _Wind("Static surface",Float)=0
        _Desaturate("Surface pigment",Float)=0
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
        TEXTURE2D(_RockMap); SAMPLER(sampler_RockMap);
        CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_ST;
        float4 _RockTint;
        float _Cutoff, _Wind, _MapWidth, _Desaturate, _MapHeight;
        CBUFFER_END
        struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; half4 color:COLOR; };
        struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; float3 normalWS:TEXCOORD1; float3 positionWS:TEXCOORD2; half4 color:COLOR; half fog:TEXCOORD3; };
        Varyings Vert(Attributes v)
        {
            Varyings o; o.positionWS=TransformObjectToWorld(v.positionOS.xyz); o.positionCS=TransformWorldToHClip(o.positionWS);
            o.normalWS=TransformObjectToWorldNormal(v.normalOS); o.uv=v.uv; o.color=v.color;
            o.fog=ComputeFogFactor(o.positionCS.z); return o;
        }
        ENDHLSL
        Pass
        {
            Name "Ground" Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fog
            half4 Frag(Varyings i):SV_Target
            {
                half4 paint=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);
                float3 weights=pow(abs(normalize(i.normalWS)),4); weights/=max(.001,weights.x+weights.y+weights.z);
                float mx=min(i.positionWS.x,_MapWidth-i.positionWS.x);
                half3 rock=SAMPLE_TEXTURE2D(_RockMap,sampler_RockMap,float2(i.positionWS.z,i.positionWS.y)/3).rgb*weights.x
                    +SAMPLE_TEXTURE2D(_RockMap,sampler_RockMap,float2(mx,i.positionWS.z)/3).rgb*weights.y
                    +SAMPLE_TEXTURE2D(_RockMap,sampler_RockMap,float2(mx,i.positionWS.y)/3).rgb*weights.z;
                rock=lerp(rock,dot(rock,half3(.25,.6,.15)).xxx,.3)*_RockTint.rgb;
                paint.rgb=lerp(paint.rgb,rock,saturate(i.color.a));
                clip(paint.a-_Cutoff);
                half3 n=normalize(i.normalWS);
                // Match the underlying URP/Lit shelf, including its roughness and ambient light.
                // A foliage-specific lighting model produces a visible grid at the tapered foot.
                SurfaceData surface=(SurfaceData)0;
                surface.albedo=paint.rgb;surface.smoothness=.12;surface.normalTS=half3(0,0,1);
                surface.occlusion=1;surface.alpha=1;
                InputData data=(InputData)0;
                data.positionWS=i.positionWS;data.normalWS=n;
                data.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.positionWS);
                data.shadowCoord=TransformWorldToShadowCoord(i.positionWS);
                data.bakedGI=SampleSH(n);data.vertexLighting=VertexLighting(i.positionWS,n);
                data.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.positionCS);
                data.shadowMask=half4(1,1,1,1);
                half4 lit=UniversalFragmentPBR(data,surface);
                return half4(MixFog(lit.rgb,i.fog),1);
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
