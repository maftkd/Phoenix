Shader "Unlit/Lock"
{
    Properties
    {
		_Color ("Color", Color) = (1,1,1,1)
		_ColorOn ("ColorOn", Color) = (1,1,1,1)
		_Power ("Power", Range(0,1)) = 0
		_RingThickness ("Ring Thicknesss", Range(0,1)) = 0.1
		_Radius ("Radius", Range(0,0.5)) = 0.5
		_RingCenter ("Ring Center", Vector) = (0.5,0.5,0,0)
		_OutlineThickness ("Outline Thickness", Range(0,1)) = 0
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

            fixed4 _Color;
            fixed4 _ColorOn;
			fixed _Power;
			fixed _Radius;
			fixed4 _RingCenter;
			fixed _RingThickness;
			fixed _OutlineThickness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv=v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				//get distance from center
				fixed2 center = fixed2(_RingCenter.x,_RingCenter.y);
				fixed2 diff=i.uv-center;
				fixed dSqr=dot(diff,diff);
				fixed d=sqrt(dSqr);
				fixed r = _Radius;

				//determine if within outer circle
				fixed withinCircle=step(dSqr,r*r);

				//determine ring thickness
				fixed rt = 1-(_RingThickness*0.5+0.5);

				//determine left bar region
				fixed bars = step(rt,abs(i.uv.x-_RingCenter.x));
				fixed isLeft=1-step(0.5,i.uv.x);
				bars*=isLeft;

				//delinate below region
				fixed belowCenter=1-step(_RingCenter.y,i.uv.y);

				//use ring for above center and bars for below
				fixed ring=belowCenter*bars+(1-belowCenter)*step(rt*rt,dSqr)*withinCircle;
				clip(ring-0.5);

				//coloring
				fixed4 col = lerp(_Color,_ColorOn,_Power);

				//get outline for bar
				//left bar outline
				fixed outlineBar = step((1-_OutlineThickness)*0.5,_RingCenter.x-i.uv.x);
				//right bar outline
				outlineBar+=1-step(_OutlineThickness,abs(i.uv.x*2-_RingThickness));
				outlineBar = saturate(outlineBar);
				fixed4 colBelowCenter = outlineBar*fixed4(0,0,0,1)+(1-outlineBar)*col;

				//get outline for ring
				fixed outlineRing = 1-step(_OutlineThickness*0.5,r-d);
				outlineRing+=1-step(_OutlineThickness*0.5,d-(r-_RingThickness*0.5));
				outlineRing=saturate(outlineRing);
				fixed isRight=1-isLeft;
				fixed rightOutline=isRight*step(i.uv.y-_OutlineThickness*0.5,_RingCenter.y);
				fixed4 colAboveCenter=outlineRing*fixed4(0,0,0,1)+(1-outlineRing)*col;
				colAboveCenter=colAboveCenter*(1-rightOutline)+rightOutline*fixed4(0,0,0,1);

				//final color
				col = belowCenter*colBelowCenter+(1-belowCenter)*colAboveCenter;
                return col;
            }
            ENDCG
        }
    }
}
