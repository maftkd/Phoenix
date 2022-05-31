Shader "Unlit/Map"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColTex ("Color Texture", 2D) = "white" {}
        _NoiseTex ("Color Texture", 2D) = "white" {}
		_WaterColor ("Water color", Color) = (1,1,1,1)
		_WaterColorB ("Water color B", Color) = (1,1,1,1)
		_LineSpacing ("Line Spacing", Range(0,2)) = 1
		_LineWidth ("Line Width", Range(0,1)) = 0.1
		_MaxHeight ("max Height", Float) = 10
		_GridSize ("Grid Size", Range(0,1)) = 0.1
		_GridWidth ("Grid Width", Range(0,1)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
            };

            sampler2D _MainTex;
            sampler2D _ColTex;
            sampler2D _NoiseTex;
            float4 _MainTex_ST;
			fixed4 _WaterColor;
			fixed4 _WaterColorB;
			fixed _LineSpacing;
			fixed _LineWidth;
			fixed _MaxHeight;
			fixed _GridSize;
			fixed _GridWidth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				fixed4 col = tex2D(_ColTex, i.uv);
                fixed4 d = tex2D(_MainTex, i.uv).r;
				fixed height=d*100;
				fixed water=step(height,4.9);
				fixed n = tex2D(_NoiseTex, i.uv+fixed2(1,1)*_Time.x*0.5).r;
				fixed thresh=lerp(0.15,0.3,abs(sin(_Time.y*0.5)));
				n=step(thresh,n);
				fixed3 waterCol=lerp(_WaterColor.rgb,_WaterColorB.rgb,n);
				col.rgb=lerp(col.rgb,waterCol,water);
				fixed h=height%_LineSpacing;
				fixed l = step(h,_LineWidth*_LineSpacing)*(1-water);
				l*=step(height,_MaxHeight);
				col.rgb=lerp(col.rgb,fixed3(0,0,0),l);
				fixed grid=step(i.uv.x%_GridSize,_GridSize*_GridWidth);
				grid+=step(i.uv.y%_GridSize,_GridSize*_GridWidth);
				grid=saturate(grid);
				col.rgb=lerp(col.rgb,fixed3(1,1,1),grid);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
