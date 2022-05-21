Shader "Unlit/Buoy"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color ("Main color", Color) = (1,1,1,1)
		_Stripe ("Stripe color", Color) = (1,1,1,1)
		_Band ("Band Width", Range(0,1)) = 0.5
		_Dirt ("Dirt", Range(0,1)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry"}
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
				half3 worldNormal : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed4 _Color;
			fixed4 _Stripe;
			fixed _Band;
			fixed _Dirt;

            v2f vert (appdata v, float3 normal : NORMAL)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldNormal = UnityObjectToWorldNormal(normal);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                //fixed4 col = tex2D(_MainTex, i.uv);

				fixed band=step(abs(0.7-i.uv.y)*2,_Band);
				fixed4 col=band*_Stripe+(1-band)*_Color;
				col.rgb-=tex2D(_MainTex,i.uv).r*_Dirt;

				fixed dt = dot(_WorldSpaceLightPos0.xyz,i.worldNormal);
				dt=(dt+1)*0.5;
				col.rgb*=dt;


                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
