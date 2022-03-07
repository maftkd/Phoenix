Shader "Unlit/Lock"
{
    Properties
    {
		_Color ("Color", Color) = (1,1,1,1)
		_ColorOn ("ColorOn", Color) = (1,1,1,1)
		_Power ("Power", Range(0,1)) = 0
		_OutlineThickness ("Outline", Range(0,1)) = 0.1
		_Radius ("Radius", Range(0,0.5)) = 0.5
		_RingCenter ("Ring Center", Vector) = (0.5,0.5,0,0)
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

            fixed4 _Color;
            fixed4 _ColorOn;
			fixed _Power;
			fixed _Radius;
			fixed4 _RingCenter;
			fixed _OutlineThickness;

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
				fixed2 center = fixed2(_RingCenter.x,_RingCenter.y);
				fixed2 diff=i.uv-center;
				fixed dSqr=dot(diff,diff);
				fixed r = _Radius;
				fixed withinCircle=step(dSqr,r*r);
				fixed4 col = lerp(_Color,_ColorOn,_Power);
				fixed ot = 1-(_OutlineThickness*0.5+0.5);

				fixed belowCenter=1-step(_RingCenter.y,i.uv.y);
				fixed bars = step(ot,abs(i.uv.x-_RingCenter.x));
				fixed isLeft=1-step(0.5,i.uv.x);
				bars*=isLeft;
				fixed outline=belowCenter*bars+(1-belowCenter)*step(ot*ot,dSqr)*withinCircle;
				clip(outline-0.5);
                return col;
            }
            ENDCG
        }
    }
}
