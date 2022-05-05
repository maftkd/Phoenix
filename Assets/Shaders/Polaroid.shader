Shader "Custom/Polaroid"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_PicParams ("Pic Params (hor margin, bottom margin, aspect, ?)", Vector) = (0.9,0.2,0.75,0)
		_Develop ("Develop", Range(0,1)) = 0
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
		fixed4 _PicParams;
		fixed _Develop;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color

			fixed2 myUv=IN.uv_MainTex;
			/*
			myUv.x=(myUv.x-((1-_PicParams.x)*0.5))/_PicParams.x;
			fixed inZone=step(abs(myUv.x-0.5)*2,1);
			fixed height=_PicParams.x*_PicParams.z;
			myUv.y=(myUv.y-(_PicParams.y))/height;
			inZone*=step(abs(myUv.y-0.5)*2,1);
			*/

			fixed4 picColor=tex2D(_MainTex,myUv)*_Develop;


            //o.Albedo = inZone*picColor+(1-inZone)*fixed3(1,1,1);
            o.Albedo = picColor;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;//*inZone;
            o.Smoothness = _Glossiness;//*(1-inZone);
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
