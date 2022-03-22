Shader "Unlit/Grass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
		_AlphaDist ("Alpha distance mult", Float) = 0.5
		_Color ("Color mult", Color) = (1,1,1,1)
		_ColorDark ("Color Dark", Color) = (1,1,1,1)
		_Bend ("Bend", Float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "LightMode" = "ForwardBase"}
        LOD 100
		Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
			#pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
			#include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
				float dist : DIST;
				LIGHTING_COORDS(0,1)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed _Cutoff;
			fixed _AlphaDist;
			fixed4 _Color;
			fixed4 _ColorDark;
			fixed _Bend;

            v2f vert (appdata v)
            {
                v2f o;
				fixed4 worldSpace = mul(unity_ObjectToWorld, v.vertex);
				_Bend*= sin(_Time.z+worldSpace.x);
				v.vertex.z+=_Bend*v.vertex.y;
				o.dist=min(1,length(ObjSpaceViewDir(v.vertex))*_AlphaDist);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //UNITY_TRANSFER_FOG(o,o.vertex);
				TRANSFER_VERTEX_TO_FRAGMENT(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
				//col.a*=i.dist;
				clip(col.a-_Cutoff);
				float attenuation = LIGHT_ATTENUATION(i);
				col.rgb*=lerp(_ColorDark,_Color,attenuation);
				//col.a=1;
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
