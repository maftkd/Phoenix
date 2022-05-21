Shader "Custom/Boundary"
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
        _NoiseTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent""Queue"="Transparent" }
        LOD 200
			Cull Off

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
		sampler2D _NoiseTex;

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

			fixed2 baseUV = IN.uv_NoiseTex;
			fixed n = tex2D(_NoiseTex,baseUV+fixed2(1,1)*_Time.x).r;
			//o.Emission=fixed3(1,1,1)*wavePow;
			o.Albedo=col.rgb;
			o.Emission=o.Albedo*n;
            o.Alpha = lerp(1,_MinAlpha,s)*n;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;//*saturate(noiseDif);
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
