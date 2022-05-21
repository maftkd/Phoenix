Shader "Unlit/ForceField"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_DistParams ("Distance Params", Vector) = (0,1,0,0)

    }
    SubShader
    {
        Tags { "RenderType"="Transparent""Queue"="Transparent+1" }
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
				float dist : DIST;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed4 _DistParams;

            v2f vert (appdata v)
            {
                v2f o;
				o.dist=min(1,length(ObjSpaceViewDir(v.vertex)));
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				fixed2 coords=i.uv;
				coords+=_DistParams.zw*_Time.y;
                fixed4 col = tex2D(_MainTex, coords);
				//col.a=col.r;
				fixed dist = 1-smoothstep(_DistParams.x,_DistParams.y,i.dist);
				col.a*=dist;
				col.rgb=fixed3(1,1,1)*step(col.r,dist);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
