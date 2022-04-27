Shader "Unlit/Fade"
{
    Properties
    {
		_Amount("Amount", Range(0,1)) = 0
		_Value ("Value", Range(0,1)) = 0 
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

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
			fixed _Value;

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
				fixed4 col = fixed4(_Value,_Value,_Value,_Amount);
                return col;
            }
            ENDCG
        }
    }
}
