Shader "Unlit/CircuitLine"
{
    Properties
    {
		_ColorOn ("Color On", Color) = (1,1,1,1)
		_ColorOff ("Color Off", Color) = (0.5,0.5,0.5,0.5)
		_FillAmount ("Fill Amount", Range(0,1)) = 0
		_Outline ("Outline thickness", Range(0,0.5)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
			Cull Front

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
			fixed _FillAmount;
			fixed _Outline;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv=v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed power=1-step(_FillAmount,i.uv.x);
				fixed4 lineCol=power*_ColorOn+(1-power)*_ColorOff;
				fixed outline = step(0.5-_Outline,abs(i.uv.y-0.5));
				fixed4 col = outline*fixed4(0,0,0,1)+(1-outline)*lineCol;
                return col;
            }
            ENDCG
        }
		Pass{
			Cull Back
			Stencil {
				Ref 1
				Comp always
				Pass replace
			}
			CGPROGRAM

			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag

			struct appdata{
				float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
			};

			struct v2f{
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v){
				v2f o;
				o.uv=v.uv;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed _FillAmount;

			fixed4 frag(v2f i) : SV_TARGET{
				fixed power=1-step(_FillAmount,i.uv.x);
				clip(power-0.5);
				return fixed4(0,0,0,0);
			}

			ENDCG
		}
    }
}
