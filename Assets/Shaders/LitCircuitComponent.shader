Shader "Custom/LitCircuitComponent"
{
    Properties
    {
        _ColorOn ("Color On", Color) = (1,1,1,1)
        _ColorOff ("Color Off", Color) = (1,1,1,1)
		_OutlineColor ("Outline color", Color) = (0,0,0,1)
		_OutlineThickness ("Outline thickness", Range(0,1)) = 0.1
		_Power ("Power", Range(0,1)) = 0
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_MainTex ("Main Tex", 2D) = "white" {}
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
        fixed4 _ColorOn;
        fixed4 _ColorOff;
        fixed4 _OutlineColor;
		fixed _OutlineThickness;
		fixed _Power;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color

			fixed outlineX=1-step(_OutlineThickness,1-abs(IN.uv_MainTex.x-0.5)*2);
			fixed outlineY=1-step(_OutlineThickness,1-abs(IN.uv_MainTex.y-0.5)*2);
			fixed ol=saturate(outlineX+outlineY);
            o.Albedo = ol*_OutlineColor+(1-ol)*lerp(_ColorOff.rgb,_ColorOn.rgb,_Power);
			//o.Emission=(1-ol)*_Power*_ColorOn.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
