Shader "Custom/HeightSensor"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_HeightMark ("Height Marker", Range(0,1)) = 0.5
		_MarkColor ("Marker Color", Color) = (1,0,0,1)
		_TargetWidth ("Target width", Range(0,1)) = 0.5
		_TargetColor ("Target color", Color) = (0.5,0.5,0.5,1)
		_LedInZoneColor ("Led Color in Zone", Color) = (0,1,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
		float4 _MainTex_ST;

        struct Input
        {
            float2 custom_uv;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		fixed _HeightMark;
		fixed4 _MarkColor;
		fixed _TargetWidth;
		fixed4 _TargetColor;
		fixed4 _LedInZoneColor;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

		void vert (inout appdata_full v, out Input o)
        {
            // copy the unmodified texture coordinates (aka UVs)
            //o.custom_uv = v.texcoord.xy;
			o.custom_uv = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.custom_uv) * _Color;
			
			//get row 
			fixed row = _MainTex_ST.y*_HeightMark;

			//snap row
			row-=frac(row)-0.5;
			fixed uvDiff=abs(IN.custom_uv.y-row);
			fixed band = step(uvDiff,0.5);

			//color target range
			uvDiff=abs(IN.custom_uv.y-0.5*_MainTex_ST.y);
			fixed inTarget=step(uvDiff,_TargetWidth*_MainTex_ST.y*0.5);
			fixed3 baseColor = _Color.rgb*(1-inTarget)+inTarget*_TargetColor.rgb;
			baseColor*=c.a;

			//color led
			//if( band row is within target range
			//	color green
			uvDiff=abs(row-0.5*_MainTex_ST.y);
			fixed bandWithinTarget=step(uvDiff,_TargetWidth*_MainTex_ST.y*0.5);
			fixed3 ledColor = _MarkColor*(1-bandWithinTarget)+_LedInZoneColor.rgb*bandWithinTarget;

			o.Albedo = baseColor*(1-band)+band*ledColor*c.a;
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
