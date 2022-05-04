// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/Toon"
{
    Properties
    {
		_Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
		_OutlineThickness ("Outline Thickness", Range(0,0.01)) = 0.001
		_ToonGrad ("Toon Gradient", 2D) = "white" {}
		_MinShadow ("Max Shadow", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase"}
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
			#pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
			#include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
				float3 norm : REFLECT;
				LIGHTING_COORDS(0,1)
                UNITY_FOG_COORDS(2)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			sampler2D _ToonGrad;
			fixed4 _Color;
			fixed _MinShadow;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                // world space normal
                o.norm= UnityObjectToWorldNormal(v.normal);

                UNITY_TRANSFER_FOG(o,o.pos);
				TRANSFER_VERTEX_TO_FRAGMENT(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv)*_Color;
				
				//get shadowmap
				float attenuation = LIGHT_ATTENUATION(i);

				//get shading via dir light
				fixed dt = -dot(-i.norm,_WorldSpaceLightPos0.xyz);
				dt=saturate(dt);

				//remap to toon grad
				fixed minShadow=0.25;
				fixed lit=tex2D(_ToonGrad, fixed2(1-dt,0)).x;
				lit=lerp(_MinShadow,1,lit);
				col.rgb*=lit;
				
				//receive shadow
				fixed shadow=lerp(minShadow,1,attenuation);
				col.rgb*=shadow;
				fixed ambient=0.25;
				col.rgb=lerp(col.rgb,unity_FogColor.rgb,(1-shadow)*ambient);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
		Pass{

			Cull Front
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM

			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag

			struct appdata{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			fixed _OutlineThickness;
			fixed4 _RimColor;
			fixed _Highlight;

			v2f vert(appdata v){
				v2f o;
				float3 normal = normalize(v.normal);
				float3 outlineOffset = normal*_OutlineThickness;
				float3 position = v.vertex+outlineOffset;
				o.position = UnityObjectToClipPos(position);
				o.uv=v.uv;
				return o;
			}
			fixed4 frag(v2f i) : SV_TARGET{
				clip(_OutlineThickness-0.0001);
				fixed4 col = fixed4(0,0,0,1);
				return col;
			}

			ENDCG
		}
    }
    FallBack "Diffuse"
}
