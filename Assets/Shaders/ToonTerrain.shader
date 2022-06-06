Shader "Unlit/ToonTerrain"
{
    Properties
    {
		_GrassColorA ("Grass Color A", Color) = (1,1,1,1)
		_GrassColorB ("Grass Color B", Color) = (1,1,1,1)
		_RockColor ("Rock Color", Color) = (1,1,1,1)
		_SandColor ("Sand Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
		_ToonGrad ("Toon Gradient", 2D) = "white" {}
		_MinShadow ("Max Shadow", Range(0,1)) = 0.5
		_ShadowStrength ("Shadow strength", Range(0,1)) = 1
		_SpecDot ("Spec Dot", Range(0,1)) = 0.9
		_SandHeight ("Sand Height", Float) = 6
		_RockDot ("Rock Dot", Range(0,1)) = 0.5
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
				float3 worldPos : WORLD;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed4 _GrassColorA;
			fixed4 _GrassColorB;
			fixed4 _RockColor;
			fixed4 _SandColor;
            sampler2D _ToonGrad;
			fixed _MinShadow;
			fixed _ShadowStrength;
			fixed _SandHeight;
			fixed _RockDot;
			fixed4 _LightColor0;
			fixed _SpecDot;
			fixed4 _PlayerPos;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.norm= UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                UNITY_TRANSFER_FOG(o,o.pos);
				TRANSFER_VERTEX_TO_FRAGMENT(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				//player pos stuff
				fixed2 diff=_PlayerPos.xz-i.worldPos.xz;
				fixed sqrDst=dot(diff,diff);
				fixed closeness=smoothstep(1,0,sqrDst);//*_PlayerPos.w;
				//col.rgb=lerp(col.rgb,fixed3(1,1,1),closeness);

				fixed4 col = fixed4(1,1,1,1);
				fixed xzRandom=tex2D(_MainTex,i.worldPos.xz*(0.05+closeness*0.005)+fixed2(1,1)*sin(_Time.y)*0.002*sin(i.worldPos.x+i.worldPos.z));
				fixed sand=step(i.worldPos.y+xzRandom,_SandHeight);
				col.rgb=lerp(col.rgb,_SandColor.rgb,sand);
				fixed notSand=1-sand;
				fixed rockDot=dot(i.norm,fixed3(0,1,0));
				fixed isRock=step(rockDot+xzRandom*0.2,_RockDot);
				fixed3 grassCol=lerp(_GrassColorA,_GrassColorB,tex2D(_MainTex,i.worldPos.xz*0.005)).rgb;
				//grassCol=lerp(grassCol,_RockColor.rgb,step(0.7,tex2D(_MainTex,i.worldPos.xz*0.2)));
				grassCol=lerp(grassCol,_RockColor.rgb,step(lerp(0.75,0.8,abs(sin(_Time.x*2))),xzRandom));
				fixed3 notSandCol=lerp(grassCol,_RockColor.rgb,isRock);
				col.rgb=lerp(col.rgb,notSandCol,notSand);

				//get shadowmap
				float attenuation = LIGHT_ATTENUATION(i);

				//get shading via dir light
				fixed dt = -dot(-i.norm,_WorldSpaceLightPos0.xyz);
				fixed spec=step(_SpecDot,dt);
				dt=saturate(dt);

				//remap to toon grad
				fixed minShadow=0.25;
				fixed lit=tex2D(_ToonGrad, fixed2(1-dt,0)).x;
				lit=lerp(_MinShadow,1,lit);
				col.rgb*=lit;
				//col.rgb=lerp(col.rgb,_LightColor0.rgb,spec*0.3);
				col.rgb=lerp(col.rgb,col.rgb*col.rgb,spec*0.3);
				
				//receive shadow
				fixed shadow=lerp(minShadow,1,attenuation);
				fixed3 preShadow=col.rgb;
				col.rgb*=shadow;
				fixed ambient=0.25;
				col.rgb=lerp(col.rgb,unity_FogColor.rgb,(1-shadow)*ambient);
				col.rgb=lerp(preShadow,col.rgb,_ShadowStrength);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
