Shader "Unlit/Target"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_FillTex ("Fill Texture", 2D) = "white" {}
		_Fill ("Fill", Range(0,1)) = 0.5
		_FillColor ("Fill color", Color) = (1,1,1,1)
		_SuccessColor ("Success color", Color) = (1,1,1,1)
		_Success ("Success", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
			ZTest Off
			Cull Front
			Blend SrcAlpha OneMinusSrcAlpha

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _FillTex;
			fixed _Fill;
			fixed4 _FillColor;
			fixed _Success;
			fixed4 _SuccessColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				fixed filledFull=step(0.999,_Fill);
                fixed4 col = tex2D(_MainTex, i.uv);
				//col.rgb=lerp(col.rgb,_FillColor.rgb,filledFull);
				fixed filled=step(1-i.uv.x,_Fill)*(1-filledFull);
				clip(col.a-0.5+filled);
				col.a*=lerp(1,_FillColor.a,filledFull);
				col+=filled*_FillColor*tex2D(_FillTex,i.uv).a;
				col=lerp(col,_SuccessColor,_Success*abs(sin(_Time.w*4)));
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
