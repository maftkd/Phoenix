// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/Boid"
{
    Properties
    {
		_Color ("Color", Color) = (0,0,0,1)
		_WingParams ("Wing params", Vector) = (1,0,0,0)
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
			fixed4 _WingParams;

            v2f vert (appdata v)
            {
                v2f o;
				o.uv=v.uv;
				float3 worldPos = mul (unity_ObjectToWorld, v.vertex).xyz;
				fixed dist = abs(o.uv.x-0.5)*2;
				v.vertex.y+=dist*_WingParams.x*sin(_Time.w*_WingParams.y+(worldPos.x+worldPos.y+worldPos.z)*_WingParams.z);
				v.vertex.z*=1-dist;
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				fixed4 col = _Color;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
