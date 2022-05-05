Shader "Custom/Water"
{
    Properties
    {
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _ColorDeep ("Color Deep", Color) = (1,1,1,1)
		_ColorShallow ("Color Shallow", Color) = (0,0,0,0)
		_MinStep ("Min Step", Float) = 0
		_MaxStep ("Max Step", Float) = 1
		_MinAlpha ("Min Alpha", Range(0,1)) = 0.5
		_FoamDist ("Foam Distance", Float) = 0.1
		_FoamColor ("Foam Color", Color) = (1,1,1,1)
		_FoamFreq ("Foam Frequency", Float) = 1
        _NoiseTex ("Albedo (RGB)", 2D) = "white" {}
		_WindDir ("Wind direction", Vector) = (1,0,1,0)
		_WindSpeed ("Wind multiplier", Float) = 0.25
		_SineMult ("Sine Multiplier", Float) = 0.1
		//testing
		_WaveCutoff ("Wave Cutoff", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent""Queue"="Transparent" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha:blend

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0


        struct Input
        {
			fixed4 screenPos;
            float2 uv_NoiseTex;
        };

        half _Glossiness;
        half _Metallic;
		sampler2D _CameraDepthTexture;
		fixed4 _ColorDeep;
		fixed4 _ColorShallow;
		fixed _MinStep;
		fixed _MaxStep;
		fixed _MinAlpha;
		fixed _FoamDist;
		fixed _FoamFreq;
		fixed4 _FoamColor;
		sampler2D _NoiseTex;
		fixed4 _WindDir;
		fixed _WindSpeed;
		fixed _SineMult;
		//testing
		fixed _WaveCutoff;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
			half sceneZ = LinearEyeDepth(tex2D(_CameraDepthTexture, screenUV).r);
			half depth = sceneZ-IN.screenPos.w;

			//water color
			fixed s=smoothstep(_MinStep,_MaxStep,depth);
			fixed4 col = lerp(_ColorShallow,_ColorDeep,s);

			//foam
			fixed2 baseUV = IN.uv_NoiseTex;
			fixed sn = sin(_Time.x);
			fixed2 scrollOffset=_Time.x*_WindDir.xy*_WindSpeed-sn*_WindDir.xy*_SineMult+fixed2(0.25,0);
			fixed foamNoise=tex2D(_NoiseTex,(baseUV*_FoamFreq)+scrollOffset);
			fixed foam = step(depth,_FoamDist+(foamNoise-0.5)*_FoamDist*2);
			//fixed foam = step(depth,_FoamDist);
			foam*=step(IN.screenPos.w,60);

			//waves
			fixed waveNoise=tex2D(_NoiseTex,baseUV);
			waveNoise=step(_WaveCutoff,waveNoise);
			col.rgb=lerp(col.rgb,fixed3(1,1,1),waveNoise);

			o.Albedo=col.rgb*(1-foam)+foam*_FoamColor;
            o.Alpha = lerp(_MinAlpha,1,s)*(1-foam)+foam*_FoamColor.a;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
