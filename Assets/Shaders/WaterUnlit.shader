Shader "Unlit/WaterUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorDeep ("Color Deep", Color) = (1,1,1,1)
		_ColorShallow ("Color Shallow", Color) = (0,0,0,0)
		_MinStep ("Min Step", Float) = 0
		_MaxStep ("Max Step", Float) = 1
		_ReflectPow ("Reflection Power", Float) = 5
		_ReflectDamp ("Reflection Dampen", Range(0,1)) = 0.1
		_WaveCutoff ("Wave Cutoff", Range(0,1)) = 0.5
		_WaveCutoffMax ("Wave CutoffMax", Range(0,1)) = 0.5
		_WaveSpeed ("Wave Speed", Range(0,.3)) = 0.5
		_WavePower ("Wave Power", Range(0.01,5)) = 1
		_WaveScale ("Wave Scale", Range(0,.1)) = 0.1
		_WavePeak ("Wave Peak", Range(0,1)) = 0.9
		_NoiseScale ("Noise Scale", Float) = 4
		_NoiseSpeed ("Noise Speed", Range(0,1)) = 0.1
		_NoiseIntensity ("Noise intensity", Range(0,3)) = 0.1
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
				float4 scrPos : SCREEN_POS;
				float3 worldPos : WORLD;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			sampler2D _CameraDepthTexture;
			fixed4 _ColorDeep;
			fixed4 _ColorShallow;
			fixed _MinStep;
			fixed _MaxStep;
			fixed _ReflectPow;
			fixed _ReflectDamp;
			fixed4 _LightColor0;
			fixed _WaveCutoff;
			fixed _WaveCutoffMax;
			fixed _WaveSpeed;
			fixed _WavePower;
			fixed _WaveScale;
			fixed _WavePeak;
			fixed _NoiseScale;
			fixed _NoiseSpeed;
			fixed _NoiseIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.scrPos = ComputeScreenPos(o.vertex);

				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				//sample depth
				float2 screenUV = i.scrPos.xy / i.scrPos.w;
				half sceneZ = LinearEyeDepth(tex2D(_CameraDepthTexture, screenUV).r);
				half depth = sceneZ-i.scrPos.w;

				//water color
				fixed s=smoothstep(_MinStep,_MaxStep,depth);
				fixed4 col = lerp(_ColorShallow,_ColorDeep,s);

				//NOISE NOISE NOISE
                fixed noise = tex2D(_MainTex, i.uv).r;
                fixed noise2 = tex2D(_MainTex, (i.uv+fixed2(0.9,1)*_Time.y*_WaveSpeed)*_WaveScale).r;
                fixed noise3 = tex2D(_MainTex, (i.uv+fixed2(1,1)*_Time.y*_NoiseSpeed)*_NoiseScale).r;
				noise+=step(noise,noise3)*_NoiseIntensity;

				//get reflectance
				fixed3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
				fixed3 reflectView=viewDir;
				reflectView.y*=-1;
				fixed dt = -dot(reflectView,_WorldSpaceLightPos0.xyz);
				dt=(dt+1)*0.5;
				fixed rawDt=dt;
				dt=pow(dt,_ReflectPow);

				//sun reflection ripples
				fixed reflectNoise=step(noise3,dt-_ReflectDamp);
				//fixed reflectNoise=step(noise,dt);
				//fixed3 reflectColor=lerp(fixed3(1,1,1),_LightColor0.rgb,dt);

				//wave ripples v2
				fixed cut = lerp(_WaveCutoff,_WaveCutoffMax,noise2);
				fixed wave = step(cut,noise);
				fixed waveHeight=saturate(pow((noise-cut)/(1-cut),_WavePower));
				fixed3 waveColor=lerp(col.rgb,_ColorShallow.rgb,smoothstep(0,_WavePeak,waveHeight));
				//fixed wavePeak=smoothstep(_WavePeak,1,waveHeight)*noise2;//*sin(waveHeight*_Time.y);
				//waveColor=lerp(waveColor,fixed3(1,1,1),reflectNoise);
				col.rgb=lerp(col.rgb,waveColor,wave);

				col.rgb=lerp(col.rgb,_LightColor0.rgb,reflectNoise);


                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
