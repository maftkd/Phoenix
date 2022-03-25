Shader "Unlit/UnlitTwoSidedColor"
{
    Properties
    {
		_ColorOn ("Color On", Color) = (1,1,1,1)
		_ColorOff ("Color Off", Color) = (1,1,1,1)
		_OutlineColor ("Outline color", Color) = (0,0,0,1)
		_Lerp ("Lerp", Range(0,1)) = 0
		_OutlineThickness ("Outline Thickness", Range(0,0.5)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
			Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};


            fixed4 _ColorOn;
            fixed4 _ColorOff;
            fixed4 _OutlineColor;
			fixed _Lerp;
			fixed _OutlineThickness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = lerp(_ColorOff,_ColorOn,_Lerp);
				fixed outline = step(0.5-_OutlineThickness,abs(i.uv.y-0.5));
				outline += step(0.5-_OutlineThickness,abs(i.uv.x-0.5));
				outline = saturate(outline);
				col =col*(1-outline)+outline*_OutlineColor;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
		Pass{
			Cull Off
			Stencil {
				Ref 1
				Comp always
				Pass replace
			}
			CGPROGRAM

			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v){
				v2f o;
				o.uv=v.uv;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed _Lerp;
			fixed4 _ColorOn;

			fixed4 frag(v2f i) : SV_TARGET{
				clip(_Lerp-0.5);
				return _ColorOn;
			}

			ENDCG
		}
    }
}
