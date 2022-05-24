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
		_ReflectPow2 ("Reflection Power 2", Float) = 5
		_ReflectDamp ("Reflection Dampen", Range(0,1)) = 0.1
		_WaveSpeed ("Wave Speed", Range(0,.3)) = 0.5
		_WaveScale ("Wave Scale", Range(0,.1)) = 0.1
        _GradTex ("Grad Tex", 2D) = "white" {}
		_MinFresnel ("Min Fresnel", Range(0,1)) = 0.5
		_MaxPlayerDist ("Min player dist", Range(0,8)) = 5
		_PlayerPos ("Player Pos", Vector) = (0,0,0,0)
		_MaxNoiseBoost ("Max Noise Boost", Range(0.5,1.5)) = 1
		_RippleNoiseReduce ("Ripple Noise Reduce", Range(0,1)) = 0.1
		_Foam ("Foam", Range(0,.1)) = 0.01
		_Darkness ("Darkness", Range(0,1)) = 0
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
			fixed _ReflectPow2;
			fixed _ReflectDamp;
			fixed4 _LightColor0;
			fixed _WaveSpeed;
			fixed _WaveScale;
			sampler2D _GradTex;
			fixed _MinFresnel;
			fixed4 _PlayerPos;
			fixed _MaxPlayerDist;
			fixed _MaxNoiseBoost;
			fixed _RippleNoiseReduce;
			fixed _Foam;
			fixed _Darkness;

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

				//player position
				fixed3 diff=i.worldPos-_PlayerPos.xyz;
				fixed dSqr=dot(diff,diff);
				fixed playerNearWater=1-smoothstep(0,_MaxPlayerDist*_MaxPlayerDist,dSqr);
				playerNearWater*=lerp(0.75,1,_PlayerPos.w);

				//NOISE NOISE NOISE
                fixed noise = tex2D(_MainTex, i.uv).r;

				fixed noiseBoostMult=lerp(1,_MaxNoiseBoost,saturate(playerNearWater-noise*_RippleNoiseReduce));
                fixed noise2 = tex2D(_MainTex, (i.uv+fixed2(0.9,1)*_Time.y*_WaveSpeed)*_WaveScale*noiseBoostMult).r;
				//noise2+=step(noise2,noise3);

				//get reflectance
				fixed3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
				fixed3 reflectView=viewDir;
				reflectView.y*=-1;
				fixed dt = -dot(reflectView,_WorldSpaceLightPos0.xyz);
				dt=(dt+1)*0.5;
				fixed rawDt=dt;
				dt=pow(dt,_ReflectPow);
				fixed dt2=pow(rawDt,_ReflectPow2);
				//fixed reflectNoise=step(noise,dt2-_ReflectDamp);
				fixed reflectNoise=step(noise,dt2);

				//wave stuff
				fixed wave=noise;
				wave*=dt;
				wave+=noise2;
				wave=saturate(wave);
				col.rgb=lerp(col.rgb,_ColorShallow.rgb,tex2D(_GradTex,fixed2(wave,0)));

				//fresnel
				fixed fresnel=1-abs(dot(viewDir,fixed3(0,-1,0)));
				fresnel=smoothstep(_MinFresnel,1,fresnel);
				col.rgb=lerp(col.rgb,unity_FogColor.rgb,fresnel);

				//apply sun reflect
				col.rgb=lerp(col.rgb,_LightColor0.rgb,reflectNoise);

				//foam
				fixed foam = step(s,_Foam);
				col.rgb=lerp(col.rgb,fixed3(1,1,1),foam);
				fixed superFoam = step(s+0.007,_Foam);
				col.rgb=lerp(col.rgb,fixed3(0,0,0),superFoam);
				
				col.rgb*=_Darkness;


                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
