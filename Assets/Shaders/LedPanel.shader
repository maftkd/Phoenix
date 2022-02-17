Shader "Custom/LedPanel"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_LedMap ("Led Map", 2D) = "White" {}
		_Power ("Power", Range(0,1)) = 1
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
		sampler2D _LedMap;

        struct Input
        {
            float2 uv_MainTex;
			float2 uv_LedMap;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
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
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            //o.Albedo = c.rgb;
			float r = 0.5;
			fixed2 diff=frac(IN.uv_MainTex)-fixed2(0.5,0.5);
			fixed dSqr = dot(diff,diff);

			//read led map
			fixed led = tex2D (_LedMap, IN.uv_LedMap).r*_Power;
			o.Albedo=lerp(fixed3(1,1,1),_Color.rgb,pow(dSqr,0.1))*led*c.a;
			o.Emission=o.Albedo;

            // Metallic and smoothness come from slider variables
            o.Metallic = (1-c.a)*_Metallic;
            o.Smoothness = c.a*_Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
