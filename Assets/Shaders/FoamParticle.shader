Shader "Unlit/FoamParticle"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_NoiseAmp ("Noise amp", Range(0,1)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "PreviewType"="Plane" "Queue"="Transparent" }
		Blend One OneMinusSrcAlpha
		Cull Off Lighting Off ZWrite Off
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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed _NoiseAmp;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed noise = tex2D(_MainTex, i.uv).r;
				fixed4 col=fixed4(1,1,1,1);
				fixed2 diff=i.uv-fixed2(0.5,0.5);
				fixed sqrD=dot(diff,diff);
				//noise=(noise*2)-1;
				clip(0.25-sqrD-noise*_NoiseAmp);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
