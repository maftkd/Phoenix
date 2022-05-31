Shader "Post/MapGen"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_MaxDepth ("Max Depth", Float) = 5
		_WaterColor ("Water Color", Color) = (1,1,1,1)
		_LandColor ("Land Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        // No culling or depth
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
			sampler2D _LastCameraDepthTexture;
			fixed _MaxDepth;
			fixed4 _WaterColor;
			fixed4 _LandColor;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
				fixed rawDepth=tex2D(_LastCameraDepthTexture, i.uv).r;
                //fixed depth = LinearEyeDepth(rawDepth);
				//this gets an eye depth for ortho perspective
				float depth = (_ProjectionParams.z-_ProjectionParams.y)*(1-rawDepth)+_ProjectionParams.y;
				fixed height=_MaxDepth-depth+5;
				fixed water=step(height,5);
				col.rgb=fixed3(1,1,1)*(height*0.01);
                return col;
            }
            ENDCG
        }
    }
}
