Shader "Unlit/AnimScreen"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_AltTex ("Texture B", 2D) = "white" {}
		_Color ("Color A", Color) = (1,1,1,1)
		_ColorB ("Color B", Color) = (0,0,0,0)
		_Dir ("Direction", Range(0,1)) = 0
		_OffsetX ("Offset X", Range(0,1)) = 0
		_OffsetY ("Offset Y", Range(0,1)) = 0
		_FlipX ("Flip x", Float) = 1
		_ColumnOffsetX ("Column Offset X", Range(-1,1)) = 0
		_ColumnOffsetY ("Column Offset Y", Range(-1,1)) = 0
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
				float2 uvb : UVB;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _AltTex;
            float4 _AltTex_ST;
			fixed4 _Color;
			fixed4 _ColorB;
			fixed _Dir;
			fixed _OffsetX;
			fixed _OffsetY;
			fixed _FlipX;
			fixed _ColumnOffsetY;
			fixed _ColumnOffsetX;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				//o.uv=v.uv;
                o.uvb = TRANSFORM_TEX(v.uv, _AltTex);

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

			fixed3 hsv2rgb(fixed3 c)
			{
				fixed4 K = fixed4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
				fixed3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
				return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
			}

            fixed4 frag (v2f i) : SV_Target
            {
				fixed columnA=i.uv.x-frac(i.uv.x);
				fixed hue = columnA/_MainTex_ST.x;
				fixed3 hsv = fixed3(hue,1,1);
				fixed3 rgb = hsv2rgb(hsv);
				fixed4 birdA = tex2D(_MainTex, i.uv+columnA*fixed2(_ColumnOffsetX,_ColumnOffsetY));
				birdA.rgb*=rgb;
				fixed4 birdB = tex2D(_AltTex, i.uvb+fixed2(_OffsetX,_OffsetY)+columnA*fixed2(_ColumnOffsetX,_ColumnOffsetY));
				fixed4 col=birdA*birdA.a;
				fixed4 colB = birdB*birdB.a;
				fixed4 red = fixed4(1,0,0,1);
				fixed4 black = fixed4(0,0,0,1);
				fixed right = step(0.5,frac(i.uv.x));
				fixed4 resampleA = tex2D(_MainTex,i.uv+fixed2(_ColumnOffsetX,_ColumnOffsetY));
				fixed4 resampleB = tex2D(_AltTex,i.uvb+fixed2(_OffsetX,_OffsetY)+fixed2(_ColumnOffsetX,_ColumnOffsetY));
				fixed4 realRight=lerp(resampleB,resampleA,resampleA.a);
				fixed4 sideCol = lerp(black,realRight,right);
				colB = lerp(sideCol,colB,birdB.a);
				col = lerp(colB,col,birdA.a);
				return col;


				/*
				fixed columnA=i.uv.x-frac(i.uv.x);
				fixed4 birdA = tex2D(_MainTex, i.uv+columnA*fixed2(_ColumnOffsetX,_ColumnOffsetY));
				fixed4 birdB = tex2D(_AltTex, i.uvb+fixed2(_OffsetX,_OffsetY));
				birdA = birdA.a*birdA*_Color; 
				birdB = birdB.a*birdB*_ColorB;
				fixed4 col = lerp(birdA,birdB,_Dir);
				return col;
				*/


				/*
				fixed column=i.uv.x-frac(i.uv.x);
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv+fixed2(0,1)*column*_ColumnOffset);
				fixed4 colb = tex2D(_MainTex, fixed2(_FlipX*i.uv.x+_OffsetX,i.uv.y+_OffsetY));

				fixed ab=col.a;
				fixed ba=colb.a;
				col=lerp(_Color,_ColorB,ab);
				colb=lerp(_ColorB,_Color,ba);
				col=lerp(col,colb,_Dir);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
				*/
            }
            ENDCG
        }
    }
}
