Shader "Unlit/Grass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
		_AlphaDist ("Alpha distance mult", Float) = 0.5
		_Color ("Color mult", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 100
		Cull Off
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
				float dist : DIST;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed _Cutoff;
			fixed _AlphaDist;
			fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
				//fixed3 diff=_WorldSpaceCameraPos-v.vertex;
				//o.dist=min(1,length(diff));
				o.dist=min(1,length(ObjSpaceViewDir(v.vertex))*_AlphaDist);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv)*_Color;
				col.a*=i.dist;
				clip(col.a-_Cutoff);
				//col.a=1;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
