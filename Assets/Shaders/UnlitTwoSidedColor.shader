Shader "Unlit/UnlitTwoSidedColor"
{
    Properties
    {
		_Color ("Color", Color) = (1,1,1,1)
		_ColorB ("ColorB", Color) = (1,1,1,1)
		_Lerp ("Lerp", Range(0,1)) = 0
		_OutlineThickness ("Outline Thickness", Range(0,0.5)) = 0
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
            };

            fixed4 _Color;
            fixed4 _ColorB;
			fixed _Lerp;
			fixed _OutlineThickness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = lerp(_Color,_ColorB,_Lerp);
				fixed outline = step(0.5-_OutlineThickness,abs(i.uv.y-0.5));
				outline += step(0.5-_OutlineThickness,abs(i.uv.x-0.5));
				outline = saturate(outline);
				col =col*(1-outline)+outline*fixed4(0,0,0,1);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
