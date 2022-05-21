Shader "Unlit/ImageBorder"
{
    Properties
    {
		_Color ("Color", Color) = (1,1,1,1)
		_XBorder ("X border", Range(0,0.5)) = 0.1
		_XBorderWidth ("X border width", Range(0,0.05)) = 0.01
		_YBorder ("Y border", Range(0,0.5)) = 0.1
		_YBorderWidth ("Y border width", Range(0,0.05)) = 0.01

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

            #include "UnityCG.cginc"

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

            float4 _Color;
			fixed _XBorder;
			fixed _XBorderWidth;
			fixed _YBorder;
			fixed _YBorderWidth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				//x border
				fixed xBorder = abs(0.5-i.uv.x);
				xBorder=abs(xBorder-_XBorder);
				xBorder=1-step(_XBorderWidth,xBorder);

				//y border
				fixed yBorder = abs(0.5-i.uv.y);
				yBorder=abs(yBorder-_YBorder);
				yBorder=1-step(_YBorderWidth,yBorder);


				clip(xBorder+yBorder-0.1);
				fixed4 col = _Color;
                return col;
            }
            ENDCG
        }
    }
}
