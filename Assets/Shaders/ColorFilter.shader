Shader "Post/ColorFilter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_MaxHue ("Max Hue", Range(-0.1,1.1)) = 0.05
		_MinHue ("Min Hue", Range(-0.1,1.1)) = 0.95
		_SubMask ("Sub Mask", Color) = (0,0,0,0)
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

			fixed3 rgb2hsv(fixed3 c)
			{
				fixed4 K = fixed4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				fixed4 p = lerp(fixed4(c.bg, K.wz), fixed4(c.gb, K.xy), step(c.b, c.g));
				fixed4 q = lerp(fixed4(p.xyw, c.r), fixed4(c.r, p.yzx), step(p.x, c.r));

				float d = q.x - min(q.w, q.y);
				float e = 1.0e-10;
				return fixed3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
			fixed _MaxHue;
			fixed _MinHue;
			fixed3 _SubMask;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
				fixed3 hsv = rgb2hsv(col.rgb);
                // just invert the colors
                //col.rgb = 1 - col.rgb;
				fixed3 bw = fixed3(1,1,1)*hsv.z;
				fixed inColor = step(hsv.x,_MaxHue)+step(_MinHue,hsv.x);
				inColor = saturate(inColor);
				//col.rgb=lerp(bw,col.rgb,inColor);
				col.r=lerp(bw.r,col.r,inColor*_SubMask.r);
				col.g=lerp(bw.g,col.g,inColor*_SubMask.g);
				col.b=lerp(bw.b,col.b,inColor*_SubMask.b);
                return col;
            }
            ENDCG
        }
    }
}
