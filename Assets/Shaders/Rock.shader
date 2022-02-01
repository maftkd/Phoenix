Shader "Custom/Rock"
{
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_AltTex ("Detail Tex", 2D) = "white" {}
		_BumpMap ("Bumpmap", 2D) = "bump" {}
		_Dot ("Dot", Range(0,1)) = 0
	}

	SubShader {
		Tags { "RenderType" = "Opaque" }
		CGPROGRAM
		#pragma surface surf Lambert

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float3 viewDir;
			float3 worldNormal;
			INTERNAL_DATA
		};

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _AltTex;
		fixed _Dot;

		void surf (Input IN, inout SurfaceOutput o) {
			o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
			fixed4 col = tex2D (_MainTex, IN.uv_MainTex);
			fixed4 detail = tex2D (_AltTex, IN.uv_MainTex);
			//float dt = dot(fixed3(0,-1,0),o.Normal);
			//float dt = dot(fixed3(0,-1,0),IN.worldNormal);
			
			float3 worldNormal = WorldNormalVector (IN, o.Normal);
			//o.Albedo=worldNormal;
			fixed dt = dot(fixed3(0,1,0),worldNormal);
			dt=(dt+1)*.5;
			dt*=step(_Dot,dt);
			o.Albedo = lerp(col.rgb,detail.rgb,dt);
		}
		ENDCG
	} 
	Fallback "Diffuse"
}
