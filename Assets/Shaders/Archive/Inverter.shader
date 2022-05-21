Shader "Unlit/Inverter"
{
    Properties
    {
		_ColorOff ("Off color", Color) = (1,1,1,1)
		_ColorOn ("On color", Color) = (1,1,1,1)
		_ColorB ("Border color", Color) = (0,0,0,1)
		_Border ("Border thickness", Range(0,1)) = 0.2
		_Invert ("Invert", Range(0,1)) = 0
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

			fixed4 _ColorOn;
			fixed4 _ColorOff;
			fixed4 _ColorB;
			fixed _Border;
			fixed _Invert;

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
				fixed xDist = abs(i.uv.x-0.5)*2;
				fixed yDist = abs(i.uv.y-0.5)*2;
				fixed border = step(1-_Border,xDist);
				border+=step(1-_Border,yDist);
				border = saturate(border);
				fixed invert = step(0.5,_Invert);
				fixed top = step(0.5,i.uv.y);
				top=lerp(top,1-top,invert);
				fixed4 mainCol = top*_ColorOn+(1-top)*_ColorOff;
				fixed4 col = border*_ColorB+(1-border)*mainCol;
                return col;
            }
            ENDCG
        }
    }
}
