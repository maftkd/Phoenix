Shader "Unlit/Hologram"
{
    Properties
    {
		_Color ("Main Color", Color) = (1,1,1,1)
		_BackColor ("Back Color", Color) = (1,1,1,1)
		_Frequency ("Frequency", Float) = 1
		_PhaseMult ("Phase multiplier", Float) = 1
		_VCut ("V cutoff", Range(0,1)) = 0.75
		_VWidth ("V Width", Range(0,1)) = 0.1
		_MainTex ("Main Tex", 2D) = "white" {}
		_NoiseMult ("Noise Mult", Float) = 0.1
		_AlphaCutoff ("Alpha Cut", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Geometry" }
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
				half3 normal : TEXCOORD2;
				half3 viewDir : TEXCOORD3;
            };

			fixed4 _Color;
			fixed4 _BackColor;
			fixed _Frequency;
			fixed _PhaseMult;
			fixed _VCut;
			fixed _VWidth;
            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed _NoiseMult;
			fixed _AlphaCutoff;

            v2f vert (appdata v, float3 normal :NORMAL)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
				o.normal = UnityObjectToWorldNormal(normal);
				o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                return o;
            }

            fixed4 frag (v2f i,fixed facing : VFACE) : SV_Target
            {
				clip(_VCut-i.uv.y);
				clip(i.uv.y+_VWidth-_VCut);

				//color
				fixed4 col = fixed4(1,1,1,1);
				fixed face = (facing+1)*0.5;
				//col.rgb=lerp(_Color*_BackColor.a,_Color,face);
				col.rgb=lerp(_BackColor.rgb,_Color.rgb,face);

				fixed dt = 1-abs(dot(i.viewDir,i.normal));
				dt = lerp(dt,1,_Color.a);
				//lerp towards white on edge
				col.rgb=lerp(_BackColor.rgb,col.rgb,dt);

				//noise
				fixed yCord=i.uv.y*_Frequency;
				fixed noise = tex2D(_MainTex,i.uv+fixed2(1,0)*_Time.x*0.5).r;
				yCord+=noise*_NoiseMult;

				//wave
				fixed wave = 0.5*(sin(yCord+_Time.y*_PhaseMult)+1);
				col.a*=wave;

				//cutoff
				clip(col.a-_AlphaCutoff);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
