Shader "Unlit/Hologram"
{
    Properties
    {
		_Color ("Main Color", Color) = (1,1,1,1)
		_Frequency ("Frequency", Float) = 1
		_PhaseMult ("Phase multiplier", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent""Queue"="Transparent" }
        LOD 100
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

			fixed4 _Color;
			fixed _Frequency;
			fixed _PhaseMult;

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
                fixed4 col = _Color;
				fixed wave = 0.5*(sin(i.uv.y*_Frequency+_Time.w*_PhaseMult)+1);
				col.a*=wave;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
