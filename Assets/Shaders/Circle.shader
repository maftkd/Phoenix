Shader "Unlit/Circle"
{
    Properties
    {
		_Color ("Color", Color) = (1,1,1,1)
		_ColorB ("ColorB", Color) = (1,1,1,1)
		_Powered ("Powered", Range(0,1)) = 0

		_OutlineThickness ("Outline", Range(0,1)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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

			fixed _OutlineThickness;
			fixed4 _Color;
			fixed4 _ColorB;
			fixed _Powered;

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
                // sample the texture
				fixed2 center = fixed2(0.5,0.5);
				fixed2 diff=i.uv-center;
				fixed dSqr=dot(diff,diff);
				fixed r = 0.5;
				fixed inCircle=step(dSqr,r*r);
				fixed ot = 1-(_OutlineThickness*0.5+0.5);
				fixed outline=step(ot*ot,dSqr);
				fixed4 col = lerp(lerp(_ColorB,_Color,outline),_ColorB,1-_Powered);
				outline+=_Powered;
				//fixed4 col = lerp(_Color,_ColorB,1-_Powered);
				clip(inCircle*outline-0.5);

                return col;
            }
            ENDCG
        }
    }
}
