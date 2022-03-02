Shader "Custom/River"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_FlowSpeedMult ("Flow Speed", Float)=2
		_FoamDist("Foam Distance", Float)=0.1
		_FoamNoise("Foam Noise", Float) = 0.1
		_StreamFoam ("Stream Foam", Range(0,2)) = 0.5
        _ColorDeep ("Color Deep", Color) = (1,1,1,1)
		_ColorShallow ("Color Shallow", Color) = (0,0,0,0)
		_MinStep ("Min Step", Float) = 0
		_MaxStep ("Max Step", Float) = 1
		_MinAlpha ("Min Alpha", Range(0,1)) = 0.5
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

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
			float4 screenPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		fixed _FlowSpeedMult;
		sampler2D _CameraDepthTexture;
		fixed _FoamDist;
		fixed _FoamNoise;
		fixed _StreamFoam;
		fixed4 _ColorDeep;
		fixed4 _ColorShallow;
		fixed _MinStep;
		fixed _MaxStep;
		fixed _MinAlpha;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
			fixed2 uv = IN.uv_MainTex;
			uv+=fixed2(0,-1)*_Time.x*_FlowSpeedMult;

			float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
			half sceneZ = LinearEyeDepth(tex2D(_CameraDepthTexture, screenUV).r);
			half depth = sceneZ-IN.screenPos.w;

			fixed s=smoothstep(_MinStep,_MaxStep,depth);
			fixed4 col = lerp(_ColorShallow,_ColorDeep,s);
            fixed4 c = tex2D (_MainTex, uv);

			fixed foam = step(depth,_FoamDist+c.r*_FoamNoise);
			fixed c2 = tex2D(_MainTex,uv+fixed2(1,0)*sin(_Time.x)).r;
			foam+=step(_StreamFoam,c2+c.r);
			foam=saturate(foam);

			c=lerp(c,fixed4(1,1,1,1),foam);
            o.Albedo = lerp(col.rgb,fixed3(1,1,1),foam);
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            //o.Alpha = c.a;
            o.Alpha = lerp(_MinAlpha,1,s)*(1-foam)+foam;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
