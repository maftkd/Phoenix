Shader "Unlit/Timer"
{
    Properties
    {
		_Color ("On color", Color) = (1,1,1,1)
		_ColorB ("Off color", Color) = (0,0,0,0)
		_FillAmount ("Fill Amount", Range(0,1)) = 0.5
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


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv=v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

			fixed4 _Color;
			fixed4 _ColorB;
			fixed _FillAmount;
			fixed _OutlineThickness;

            fixed4 frag (v2f i) : SV_Target
            {
				fixed r = 0.5;
				fixed2 center = fixed2(0.5,0.5);
				fixed2 diff = i.uv-center;
				fixed mag = sqrt(dot(diff,diff));
				fixed theta = atan2(diff.y,diff.x);
				fixed fAngle=((1-_FillAmount)-0.5)*3.14152*2;
				fixed fLerp = step(theta,fAngle);
				clip(r-mag);
				fixed4 col = lerp(_Color,_ColorB,fLerp);
				fixed outline = step(r-mag,_OutlineThickness);
				col = outline*fixed4(0,0,0,0)+(1-outline)*col;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
