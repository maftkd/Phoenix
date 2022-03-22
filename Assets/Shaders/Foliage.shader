Shader "Custom/Foliage"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_Bend ("Bend", Float) = 0.5
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
			float3 worldNormal;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		fixed _Bend;
		fixed _Cutoff;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

		void vert (inout appdata_full v){
			fixed4 worldSpace = mul(unity_ObjectToWorld, v.vertex);
			_Bend*= sin(_Time.z+worldSpace.x);
			v.vertex.z+=_Bend*v.vertex.y;
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			clip(c.a-_Cutoff);
			fixed upness = dot(IN.worldNormal,fixed3(0,1,0));
			upness=step(0,upness);
            o.Albedo = c.rgb;//*IN.facing;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
		Pass{
			Cull Front
			CGPROGRAM

			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag

			struct appdata{
				float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
			};

			struct v2f{
				float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			fixed _Cutoff;


			v2f vert(appdata v){
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
				return o;
			}
			fixed4 frag(v2f i) : SV_TARGET{
				fixed4 c = tex2D (_MainTex, i.uv);
				clip(c.a-_Cutoff);
				c.rgb*=0.5;
				return c;
			}

			ENDCG
		}
    }
    FallBack "Diffuse"
}
