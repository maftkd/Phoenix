Shader "Skybox/MSky"
{
    Properties
    {
		_GroundColor ("Ground Color", Color) = (0.5,0.5,0.5,1)
		_SkyColor ("Sky Color", Color) = (0,0,1,1)
		_HorizonColor ("Horizon Color", Color) = (0,0,1,1)
		_HorizonColor2 ("Horizon Color 2", Color) = (0,0,1,1)
		_SunColor ("Sun Color", Color) = (1,1,1,1)
		_SunData ("Sun Data (min,max,na,na)", Vector) = (0.1,0.2,0,0)
		_SkyData ("Sky Data (gMin, gMax,hMin,hMax)",Vector) = (-0.05,0.05,0.5,0.75)
		_GroundData ("Ground",Vector) = (-0.05,0.05,0.5,0.75)
    }
    SubShader
    {
		Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
		Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			#include "Lighting.cginc"

			struct appdata_t
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4  pos             : SV_POSITION;
				half3   rayDir          : TEXCOORD0;
				fixed3   bgColor			: TEXCOORD1;
			};

			fixed4 _GroundColor;
			fixed4 _SkyColor;
			fixed4 _HorizonColor;
			fixed4 _HorizonColor2;
			fixed4 _SunColor;
			fixed4 _GroundData;
			fixed4 _SunData;
			fixed4 _SkyData;

            v2f vert (appdata_t v)
            {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);

				// Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
				float3 eyeRay = normalize(mul((float3x3)unity_ObjectToWorld, v.vertex.xyz));

				o.rayDir = fixed3(eyeRay);
				fixed sky = smoothstep(_GroundData.x,_GroundData.y,o.rayDir.y);
				fixed horizon = smoothstep(_SkyData.x,_SkyData.y,o.rayDir.y);
				fixed3 horizonColor = lerp(_HorizonColor,_HorizonColor2,horizon);
				horizon = smoothstep(_SkyData.z,_SkyData.w,o.rayDir.y);
				fixed3 skyColor=lerp(horizonColor,_SkyColor,horizon);
				o.bgColor = lerp(_GroundColor,skyColor,sky);

				return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = fixed4(i.bgColor,1.0);
				fixed3 skyRay = i.rayDir;//normalize(i.rayDir);
				fixed sun =dot(_WorldSpaceLightPos0.xyz,skyRay);
				sun=smoothstep(_SunData.x,_SunData.y,sun);
				col.rgb=lerp(col.rgb,_SunColor.rgb,sun);
				//col.rgb=fixed3(sun,0,0);
				col.a=0.5;
                return col;
            }
            ENDCG
        }
    }
}
