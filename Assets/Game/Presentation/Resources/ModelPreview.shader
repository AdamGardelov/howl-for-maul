Shader "Howl/Model preview"
{
    Properties {
        [MainTexture] _BaseMap("Paint",2D)="white"{}
        [MainColor] _BaseColor("Material",Color)=(1,1,1,1)
        _Tint("Placement tint",Color)=(1,1,1,0)
        _Opacity("Opacity",Range(0,1))=1
        _Unlit("Luminous detail",Float)=0
    }
    SubShader {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        Pass {
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST; half4 _BaseColor,_Tint; half _Opacity,_Unlit;
            CBUFFER_END
            struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; };
            struct V { float4 positionCS:SV_POSITION; float3 normalVS:TEXCOORD0; float2 uv:TEXCOORD1; };
            V Vert(A i) {
                V o; o.positionCS=TransformObjectToHClip(i.positionOS.xyz);
                o.normalVS=mul((float3x3)UNITY_MATRIX_V,TransformObjectToWorldNormal(i.normalOS));
                o.uv=TRANSFORM_TEX(i.uv,_BaseMap); return o;
            }
            half4 Frag(V i):SV_Target {
                half3 n=normalize(i.normalVS);
                // Camera-relative studio light: every roster has the same readable front,
                // independent of the map sun, environment shadows or camera rotation.
                half key=saturate(dot(n,normalize(half3(-.45,.65,1))));
                half fill=saturate(dot(n,normalize(half3(.8,.2,.5))));
                half rim=pow(1-saturate(n.z),3)*saturate(n.y+.35);
                half3 lighting=half3(.42,.46,.49)+key*half3(.66,.61,.51)+fill*half3(.19,.23,.25)+rim*.08;
                half3 paint=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).rgb*_BaseColor.rgb;
                half3 color=paint*lerp(lighting,half3(1,1,1),_Unlit);
                color=lerp(color,_Tint.rgb,_Tint.a);
                return half4(color,_Opacity);
            }
            ENDHLSL
        }
    }
}
