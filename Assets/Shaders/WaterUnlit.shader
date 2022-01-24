Shader "Unlit/Water"
{
    Properties
    {
        _ColorDeep ("Color Deep", Color) = (1,1,1,1)
		_ColorShallow ("Color Shallow", Color) = (0,0,0,0)
		_MinStep ("Min Step", Float) = 0
		_MaxStep ("Max Step", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
				float4 screenPos : SCREEN;
            };

			sampler2D _CameraDepthTexture;
			fixed4 _ColorDeep;
			fixed4 _ColorShallow;
			fixed _MinStep;
			fixed _MaxStep;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenPos=ComputeScreenPos(o.vertex);
				COMPUTE_EYEDEPTH(o.screenPos.z);
				o.uv=v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));
				float depth = sceneZ - i.screenPos.z;
				fixed s=smoothstep(_MinStep,_MaxStep,depth);
				fixed4 col = lerp(_ColorShallow,_ColorDeep,s);
				col.a=s;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
