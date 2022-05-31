Shader "Unlit/Wipe"
{
    Properties
    {
		_Amount("Amount", Range(0,1)) = 0
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
				fixed fudge=step(_Amount,0.0001)*0.001;
				clip(_Amount-i.uv.y-fudge);
				fixed4 col = fixed4(0,0,0,1);
                return col;
            }
            ENDCG
        }
    }
}
