Shader "Unlit/Vignette"
{
    Properties
    {
		_Amount("Amount", Range(0,1)) = 0
		_Params ("Params (min,max,?,?)", Vector) = (0.25,0.4,0,0)
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
			fixed4 _Params;

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
				fixed2 diff = fixed2(0.5,0.5)-i.uv;
				fixed dstSqr=dot(diff,diff);
				fixed ds01=smoothstep(_Params.x,_Params.y,dstSqr);
				fixed a = ds01*_Amount;
				fixed4 col = fixed4(0,0,0,a);
                return col;
            }
            ENDCG
        }
    }
}
