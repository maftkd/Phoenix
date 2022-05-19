Shader "Unlit/ToonLeaf"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_AClip ("Alpha clip", Range(0,1)) = 0.1
		_OutlineThickness ("Outline Thickness", Range(0,.1)) = 0.1
		_ColorGrad ("Foliage Color Map", 2D) = "white" {}
		_HeightVec ("Height Vec (min,max)", Vector) = (10,50,0,0)
		_ToonGrad ("Toon Gradient", 2D) = "white" {}
		_MinShadow ("Max Shadow", Range(0,1)) = 0.5
		_AlphaDist ("Alpha distance", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        LOD 100
			Cull Off
			//Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

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
				float dist : DIST;
				fixed3 world : WORLD;
                UNITY_FOG_COORDS(1)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed _AClip;
			fixed _OutlineThickness;
			sampler2D _ColorGrad;
			fixed4 _HeightVec;
			sampler2D _ToonGrad;
			fixed _MinShadow;
			fixed _AlphaDist;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.norm= UnityObjectToWorldNormal(v.normal);
				o.dist=min(1,length(ObjSpaceViewDir(v.vertex))*_AlphaDist);
				o.world = mul(unity_ObjectToWorld, v.vertex).xyz;

                UNITY_TRANSFER_FOG(o,o.pos);
				TRANSFER_VERTEX_TO_FRAGMENT(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
				fixed a=col.r;
				col.a=a;
				fixed close=1-i.dist;
				clip(a-_AClip-close);

				fixed outline=1-step(0,a-_AClip-_OutlineThickness);
				col.a=1;
				//col.a*=i.dist;

				//get shading via dir light
				fixed dt = -dot(-i.norm,_WorldSpaceLightPos0.xyz);
				dt=saturate(dt);

				//remap to toon grad
				fixed minShadow=0.25;
				fixed lit=tex2D(_ToonGrad, fixed2(1-dt,0)).x;
				lit=lerp(_MinShadow,1,lit);
				fixed world01 = (i.world.y-_HeightVec.x)/(_HeightVec.y-_HeightVec.x);
				fixed worldXZ = 1+sin((i.world.x+i.world.z)*_HeightVec.z)*_HeightVec.w;
				fixed3 color = tex2D(_ColorGrad, fixed2(world01,0)).rgb;
				color.rgb*=worldXZ;
				col.rgb=lerp(color*lit,fixed3(0,0,0),outline);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }

		//*************************************************************
		Pass {
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }

			Fog {Mode Off}
			ZWrite On ZTest LEqual Cull Off
			Offset 1, 1

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			struct v2f {
				V2F_SHADOW_CASTER;
				fixed2 uv : TEXCOORD3;
				fixed dist : DIST;
			};

            sampler2D _MainTex;
			fixed _AClip;
			fixed _AlphaDist;

			v2f vert(appdata_base v) {
				v2f o;
				o.uv=v.texcoord;
				o.dist=min(1,length(ObjSpaceViewDir(v.vertex))*_AlphaDist);
				TRANSFER_SHADOW_CASTER(o)
				return o;
			}

			fixed4 frag(v2f i) : COLOR {
                fixed4 col = tex2D(_MainTex, i.uv);
				fixed a=col.r;
				fixed close=1-i.dist;
				clip(a-_AClip-close);
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
    }
	Fallback "Transparent/Cutout/Diffuse"
}
