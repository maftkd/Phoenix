Shader "Unlit/LandTarget"
{
    Properties
    {
		_Color ("Color", Color) = (1,0,0,1)
		_Radius ("Radius", Range(0,1))=0.5
		_InnerRadius ("Inner Radius", Range(0,0.5)) = 0.1
		_Freq ("Frequency", Float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 100
			ZTest Off

			Blend SrcAlpha OneMinusSrcAlpha

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
			fixed _Radius;
			fixed _InnerRadius;
			fixed _Freq;

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
				_Radius*=0.5;
				fixed4 col = _Color;
				fixed2 center=fixed2(0.5,0.5);
				fixed2 diff = center-i.uv;
				fixed distSqr=dot(diff,diff);
				fixed drawZone=step(distSqr,_Radius*_Radius);
				clip(saturate(drawZone)-0.1);
				col*=abs(cos(sqrt(distSqr)*_Freq));

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
