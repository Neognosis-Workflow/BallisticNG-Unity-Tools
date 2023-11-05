Shader "BallisticNG/Survival (Double Sided)"
{
    Properties
    {
        _MainTex("Base", 2D) = "white" {}
		_ScreenTex("Screen Texture", 2D) = "black" {}
		_Illum("Illumination", 2D) = "black" {}
		_IllumIntensity("Illumination Intensity", Range(0, 1)) = 1
		_ContourIntensity("Contour Intensity", Range(0, 1)) = 0
		_ContourColorReduction("Contour Color Reduction", Range(0, 1)) = 0
        _Color("Color", Color) = (0.5, 0.5, 0.5, 1)
		_Color2("Color2", Color) = (0.5, 0.5, 0.5, 1)
		_IllumCol("Illumination Color", Color) = (1, 1, 1, 1)
		_ScreenScale("Screen Scale", Float) = 1
		_ScreenIntensity("Screen Intensity", Range(0, 1)) = 0
		_ScreenOffset("Screen Offset", Vector) = (0, 0, 0, 0)
		_ScreenScroll("Screen Scroll", Vector) = (0, 0, 0, 0)
		_Sweep("Sweep Distance", Float) = 30
		_ToonShade("Cubemap", CUBE) = "" { }

		/*---Affine Settings--*/
		[Toggle(_ALLOW_AFFINE_MAPPING)] _AllowAfineMapping("Allow Affine Mapping", Float) = 1
		_AffineBlend("Affine Blend", Range(0, 1)) = 1
    }
    SubShader
    {
		Cull Off
        Pass
        {
            CGPROGRAM

			#include "UnityCG.cginc"
			#include "BallisticNG.cginc"

			#pragma multi_compile __ _ALLOW_AFFINE_MAPPING
			#pragma vertex vert
			#pragma fragment frag

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half2 uv : TEXCOORD;
				float3 cubenormal : TEXCOORD1;
				half distance : TEXCOORD2;
				float4 pos : TEXCOORD3;
				half3 screenPos : TEXCOORD4;

				#if defined(_ALLOW_AFFINE_MAPPING)
					half4 affineuv : TEXCOORD5;
				#endif

				fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
			float4 _Color2;
			float4 _IllumCol;
			sampler2D _Illum;
			sampler2D _ScreenTex;
			float _IllumIntensity;
			samplerCUBE _ToonShade;
			float _Sweep;
			float _ScreenScale;
			float2 _ScreenOffset;
			float2 _ScreenScroll;
			float _ScreenIntensity;
			float _ContourIntensity;
			float _ContourColorReduction;	
			
			/*---Retro Settings---*/
			half _RetroClippingDistance;
			half _AffineTextureMapping;
			half _AffineBlend;
			half _VertexSnapping;

            v2f vert(appdata_full v)
            {
                v2f o;
														
				/*---Vertex Position---*/
				float4 vert = mul(UNITY_MATRIX_MV, v.vertex);

				float4 ps1Vert = TruncateVertex(v.vertex, GEO_RES);
				vert = lerp(vert, ps1Vert, _VertexSnapping);

				float4 outVert = mul(UNITY_MATRIX_P, vert);
				o.vertex = outVert;
								
				/*---Distance To Camera---*/
				o.distance = GetClippingDistance(v.vertex, 0.3, _VertexSnapping);
							
				/*---Uvs---*/
				half2 uv = TRANSFORM_TEX(v.texcoord, _MainTex);

				#if defined(_ALLOW_AFFINE_MAPPING)
					o.uv = uv;
					o.affineuv = CalculateAffineUvs(_AffineTextureMapping, uv, o.vertex);
				#else
					o.uv = uv;
				#endif

				o.pos = mul(unity_ObjectToWorld, v.vertex);
				o.screenPos = mul(UNITY_MATRIX_MV, v.vertex);

				o.cubenormal = mul(UNITY_MATRIX_MV, float4(v.normal, 0));

				float3 camPos = _WorldSpaceCameraPos;
				camPos.y = mul(unity_ObjectToWorld, v.vertex).y;

				float dist = distance(camPos, mul(unity_ObjectToWorld, v.vertex));
				o.color = v.color * lerp(_Color, _Color2, clamp(dist, 0, _Sweep) / clamp(_Sweep, 0, _Sweep));

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
				/*--Uv---*/
				float2 uv;

				#if defined(_ALLOW_AFFINE_MAPPING)
					uv = lerp(i.uv, i.affineuv.xy / i.affineuv.z, i.affineuv.w * _AffineBlend);
				#else
					uv = i.uv.xy;
				#endif

				fixed4 cube = texCUBE(_ToonShade, i.cubenormal);
				fixed4 col = tex2D(_MainTex, uv) * i.color * cube;
				col += tex2D(_Illum, uv) * _IllumIntensity * _IllumCol;

				// isoline colour reduction
				fixed gradient = clamp(lerp(0.2, 0.15, i.screenPos.y - 1.2), 0.0, 0.8);
			    //fixed gradient = 1.0 - (((i.screenPos.x*i.screenPos.x) + (i.screenPos.y*i.screenPos.y)) * 0.005);
				col.rgb *= lerp(1.0, clamp(gradient, 0.0, 1.0) * _ContourColorReduction, _ContourColorReduction);

				// isolines
				float3 k = i.pos.xyz;
				k -= _Time * 1.5;
				float3 f = frac(k * 0.5);
				float3 df = fwidth(k * 0.5);
				float3 g = smoothstep(df * 1.0, df * 2.0, f);
				float c = 1.0 - (g.y);
				col.rgb += (float3(c, c, c) * _Color) * _ContourIntensity;

				// screen scroll
				_ScreenOffset.x = _Time *_ScreenScroll.x;
				_ScreenOffset.y = _Time *_ScreenScroll.y;
				col -= tex2D(_ScreenTex, ((i.screenPos.xy) * _ScreenScale) + _ScreenOffset) * _ScreenIntensity * col.rgba;

				clip(i.distance < _RetroClippingDistance ? 1 : -1);
				return col;
            }

            ENDCG
        }
    }
}
