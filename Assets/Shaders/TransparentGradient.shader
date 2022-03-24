Shader "Unlit/TransparentGradient"
{
    Properties
    {
		_ColorBot ("Color Bottom", Color) = (1,1,1,1)
		_ColorTop ("Color Top", Color) = (0,0,0,0)
		_Center ("Center", Range(0,1)) = 0
        _MainTex ("Texture", 2D) = "white" {}
		_NoiseLerp ("Noise Lerp", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent""Queue"="Transparent" }
        LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
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

			fixed4 _ColorBot;
			fixed4 _ColorTop;
			fixed _Center;
            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed _NoiseLerp;

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
                fixed n = tex2D(_MainTex, fixed2(i.uv.x*_MainTex_ST.x,i.uv.y*_MainTex_ST.y)+fixed2(1,1)*_Time.x).r;
				fixed top=saturate(abs(i.uv.y-_Center)*2);
				top=abs(i.uv.y-0.5)*2;
                fixed4 col = lerp(_ColorBot,_ColorTop,top);
				n=lerp(n,1,_NoiseLerp);
				col.a*=n;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
