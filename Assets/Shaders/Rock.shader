﻿Shader "Custom/Rock"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _DetailTex ("Detail", 2D) = "white" {}
		_MinDot ("Min", Range(0,1)) = 0.5
		_MaxDot ("Max", Range(0,1)) = 0.5
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
		sampler2D _DetailTex;

        struct Input
        {
            float2 uv_MainTex;
			fixed3 worldNormal;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		fixed _MinDot;
		fixed _MaxDot;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			fixed4 d = tex2D (_DetailTex, IN.uv_MainTex);
			fixed dt = dot(IN.worldNormal,fixed3(0,1,0));
			dt=(dt+1)*0.5;
			o.Albedo=lerp(c.rgb,d.rgb,smoothstep(_MinDot,_MaxDot,dt));

            //o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}