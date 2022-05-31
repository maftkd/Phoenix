Shader "Post/Outlines"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_EdgeStep ("Edge Step", Range(0,1)) = 0.5
		_EdgeColor ("Edge Color", Color) = (0,0,0,1)
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
			sampler2D _CameraDepthNormalsTexture;
			fixed _EdgeStep;
			fixed4 _EdgeColor;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
				//fixed4 dn = tex2D(_CameraDepthNormalsTexture, i.uv);
				fixed2 texelSize=_ScreenParams.zw-fixed2(1,1);
				//col.rgb = dn.rgb;
				fixed x=0;
				fixed y=0;
				x += tex2D(_CameraDepthNormalsTexture, i.uv + float2(-texelSize.x, -texelSize.y)) * -1.0;
				x += tex2D(_CameraDepthNormalsTexture, i.uv + float2(-texelSize.x,            0)) * -2.0;
				x += tex2D(_CameraDepthNormalsTexture, i.uv + float2(-texelSize.x,  texelSize.y)) * -1.0;

				x += tex2D(_CameraDepthNormalsTexture, i.uv + float2( texelSize.x, -texelSize.y)) *  1.0;
				x += tex2D(_CameraDepthNormalsTexture, i.uv + float2( texelSize.x,            0)) *  2.0;
				x += tex2D(_CameraDepthNormalsTexture, i.uv + float2( texelSize.x,  texelSize.y)) *  1.0;

				y += tex2D(_CameraDepthNormalsTexture, i.uv + float2(-texelSize.x, -texelSize.y)) * -1.0;
				y += tex2D(_CameraDepthNormalsTexture, i.uv + float2(           0, -texelSize.y)) * -2.0;
				y += tex2D(_CameraDepthNormalsTexture, i.uv + float2( texelSize.x, -texelSize.y)) * -1.0;

				y += tex2D(_CameraDepthNormalsTexture, i.uv + float2(-texelSize.x,  texelSize.y)) *  1.0;
				y += tex2D(_CameraDepthNormalsTexture, i.uv + float2(           0,  texelSize.y)) *  2.0;
				y += tex2D(_CameraDepthNormalsTexture, i.uv + float2( texelSize.x,  texelSize.y)) *  1.0;
				fixed edge=sqrt(x*x+y*y);
				//edge=step(_EdgeStep,edge);
				//col.rgb=fixed3(1,1,1)*edge;
				col.rgb=lerp(col.rgb,_EdgeColor.rgb,edge);
                return col;
            }
            ENDCG
        }
    }
}
