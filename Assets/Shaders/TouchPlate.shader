Shader "Custom/TouchPlate"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_Interactable ("Interactable", Range(0,1)) = 0
		_On ("Is on", Range(0,1)) = 0
		_Border ("Border Size", Range(0,0.5)) = 0.05
		_Border2 ("Border 2 Size", Range(0,0.5)) = 0.05
		_BorderColor ("Border Color", Color) = (0,0,0,1)
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
		fixed _Interactable;
		fixed _On;
		fixed _Border;
		fixed _Border2;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            //fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			fixed4 c = _Color*_Color.a;
			fixed xDiff=0.5-abs(IN.uv_MainTex.x-0.5);
			fixed zDiff=0.5-abs(IN.uv_MainTex.y-0.5);
			fixed border=saturate(step(xDiff,_Border)+step(zDiff,_Border))*_Interactable;
			fixed border2=saturate(step(xDiff,_Border2)+step(zDiff,_Border2))*_Interactable;
			fixed3 borderColor=fixed3(1,1,1)*(1-border2)+border2*c.rgb;
            o.Albedo = border*borderColor+(1-border)*c.rgb;

			fixed n = tex2D (_MainTex, IN.uv_MainTex).r;
			n=1-step(_On,n);
			o.Emission=_Color.rgb*n*(1-border);
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
