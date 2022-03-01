Shader "Unlit/FakeShadow"
{
    Properties
    {
		_Strength ("Shadow Strength", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
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


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv=v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

			fixed _Strength;

            fixed4 frag (v2f i) : SV_Target
            {
				fixed2 diff = fixed2(0.5-i.uv.x,0.5-i.uv.y);
				fixed dSqr=dot(diff,diff);
				//fixed d01=dSqr*4;
				fixed d01=1-smoothstep(0,0.25,dSqr);
				fixed4 col = fixed4(0,0,0,d01*_Strength);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
