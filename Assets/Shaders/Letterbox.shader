Shader "Unlit/Letterbox"
{
    Properties
    {
		_Amount("Amount", Range(-1,1)) = 0
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

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

			fixed _Amount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv=v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				//if amount is basically 0, add a fudge value to make sure all pixels are clipped
				fixed fudge=step(abs(_Amount),0.0001)*0.001;
				fixed uvHeight=1-abs(i.uv.y-0.5)*2;
				fixed uvWidth=1-abs(i.uv.x-0.5)*2;
				fixed useHeight=step(0,_Amount);
				fixed uv=useHeight*uvHeight+(1-useHeight)*uvWidth;
				clip(abs(_Amount)-uv-fudge);
				fixed4 col = fixed4(0,0,0,1);
                return col;
            }
            ENDCG
        }
    }
}
