Shader "Custom/PuzzleBox"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_Outline ("Outline thickness", Range(0,1)) = 0.1
		_TexScale ("Texture Scale", Float) = 2
		_PowerSilhouette ("Power Silhouette Color", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+1" }
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
		fixed _Outline;
		fixed4 _OutlineColor;
		fixed _TexScale;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex*_TexScale) * _Color;
			fixed outlineX=1-step(_Outline,1-abs(IN.uv_MainTex.x-0.5)*2);
			fixed outlineY=1-step(_Outline,1-abs(IN.uv_MainTex.y-0.5)*2);
			fixed ol=saturate(outlineX+outlineY);
            o.Albedo = ol*_OutlineColor.rgb+(1-ol)*c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic*c.a;
            o.Smoothness = _Glossiness*c.a;
            o.Alpha = 1;
        }
        ENDCG

		Pass{
			Stencil {
				Ref 1
				Comp equal
			}
			CGPROGRAM

			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag

			struct appdata{
				float4 vertex : POSITION;
			};

			struct v2f{
                float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v){
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 _PowerSilhouette;

			fixed4 frag(v2f i) : SV_TARGET{
				return _PowerSilhouette;
			}

			ENDCG
		}
    }
    FallBack "Diffuse"
}
