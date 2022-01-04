﻿Shader "Unlit/TransparentGradient"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_ColorBot ("Color Bottom", Color) = (1,1,1,1)
		_ColorTop ("Color Top", Color) = (0,0,0,0)
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
                fixed4 col = lerp(_ColorBot,_ColorTop,i.uv.y);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
