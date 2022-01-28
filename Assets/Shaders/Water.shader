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
		fixed4 _FoamColor;
		sampler2D _NoiseTex;
		fixed4 _WindDir;
		fixed _WindSpeed;

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

			fixed s=smoothstep(_MinStep,_MaxStep,depth);
			fixed4 col = lerp(_ColorShallow,_ColorDeep,s);

			fixed foam = step(depth,_FoamDist);

			fixed2 baseUV = IN.uv_NoiseTex;
			fixed2 scrolledUV = baseUV+_Time.x*_WindDir.xy*_WindSpeed;
			fixed2 offsetScrolledUV = baseUV-_Time.x*_WindDir.zw*_WindSpeed;
			fixed noise = tex2D(_NoiseTex, scrolledUV).r;
			fixed noiseB = tex2D(_NoiseTex, offsetScrolledUV).r;
			fixed noiseDif=1-noise*noiseB;

			o.Albedo=col.rgb*(1-foam)+foam*_FoamColor;
            o.Alpha = lerp(_MinAlpha,1,s)*(1-foam)+foam*_FoamColor.a;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;//*saturate(noiseDif*noiseDif);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
