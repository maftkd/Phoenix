Shader "Custom/Egg"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_Crack ("Crack", Range(0,1)) = 0
		_CrackWidth ("Crack Width", Range(0,1)) = 0.02
		_CrackHeight ("Crack Height", Range(0,1)) = 0.5
		_NoiseAmount ("Noise Amount", Float) = 1
		_NoiseScale ("Noise Scale", Float) = 1
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

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		fixed _CrackWidth;
		fixed _Crack;
		fixed _CrackHeight;
		fixed _NoiseAmount;
		fixed _NoiseScale;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed n = tex2D (_MainTex, IN.uv_MainTex*_NoiseScale).r;
			fixed h=_CrackHeight+(n-0.5)*2*_NoiseAmount;
			fixed center = abs(IN.uv_MainTex.y-h)*2;
			fixed crack = step(center,_CrackWidth*_Crack);
			fixed4 c = (1-crack)*_Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
