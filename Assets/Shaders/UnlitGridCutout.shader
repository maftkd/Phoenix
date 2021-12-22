// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/UnlitGridCutout"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_GridScale ("Grid Scale", Float) = 1
		_LineThickness ("Line Thickness", Range(0,1)) = 0.1
		_Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
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
				float3 worldPos : WORLD;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed _GridScale;
			fixed _LineThickness;
			fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
				o.worldPos = mul (unity_ObjectToWorld, v.vertex);
				o.vertex = UnityObjectToClipPos(v.vertex);
                //o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv)*_Color;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

				fixed xGrid = step(1-_LineThickness,frac(i.worldPos.x*_GridScale));
				fixed yGrid = step(1-_LineThickness,frac(i.worldPos.y*_GridScale));
				//fixed zGrid = step(1-_LineThickness,frac(i.worldPos.z*_GridScale));
				//fixed grid=saturate(xGrid+yGrid+zGrid);
				fixed grid=saturate(xGrid+yGrid);
				clip(grid-0.9);
                return col;
            }
            ENDCG
        }
    }
}
