Shader "Custom/Cable"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_BandWidth ("Band Width", Range(0,1)) = 0.5
		_ColorOn ("Color On", Color) = (1,1,1,1)
		_Frequency ("Frequency", Float) = 1
		_PhaseMult ("Phase multiplier", Float) = 1
		_PowerFill ("Power Fill", Range(0,1)) = 0
		_ColorOff ("Color Off", Color) = (0.5,0.5,0.5,1)
		_EmissionMult ("Emission", Float) = 1
		_WavePow ("wave Power", Float) = 40
		_WaveCutoff ("Wave cutoff", Range(0,1)) = 0.8
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		fixed _BandWidth;
		fixed4 _ColorOn;
		fixed _Frequency;
		fixed _PhaseMult;
		fixed _PowerFill;
		fixed4 _ColorOff;
		fixed _EmissionMult;
		fixed _WavePow;
		fixed _WaveCutoff;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
			//fixed4 c = _Color;
			fixed xDiff=abs(IN.uv_MainTex.x-0.5);
			fixed inBand=step(xDiff,_BandWidth*0.5);
			fixed wave = 0.5*(sin(IN.uv_MainTex.y*_Frequency-_Time.w*_PhaseMult)+1);
			//fixed wave= 0.5*(sin(IN.uv_MainTex.y*_Frequency-_Time.w*_PhaseMult)+1);
			wave=pow(wave*step(_WaveCutoff,wave),_WavePow);

			fixed4 bandColor=lerp(_ColorOn,fixed4(1,1,1,1),wave);
			fixed powered = step(IN.uv_MainTex.y,_PowerFill);
			bandColor=bandColor*powered+(1-powered)*_ColorOff;
            o.Albedo = inBand*bandColor+(1-inBand)*_Color;
			o.Emission=inBand*_ColorOn.rgb*powered*_EmissionMult;//*wave;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
