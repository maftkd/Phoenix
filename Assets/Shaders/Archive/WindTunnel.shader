Shader "Unlit/WindTunnel"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Fade ("Edge Fade", Range(0,1)) = 0.8
		_ScrollSpeed ("Scroll Speed", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 100
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
			fixed _Fade;
			fixed _ScrollSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv=v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				fixed2 coord = i.uv*_MainTex_ST.xy;
				coord.y+=_ScrollSpeed*_Time.y;
                fixed4 col = tex2D(_MainTex, coord);
				fixed diff=abs(i.uv.y-0.5)*2;
				fixed aMult=smoothstep(1,_Fade,diff);
				col.a*=aMult;


                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
