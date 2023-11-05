// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "BallisticNG/CustomShipCull"
{
    Properties
    {
        _MainTex("Base", 2D) = "white" {}
        _ReflectionMask("Reeflection", 2D) = "white" {}
		_Illum("Illumination", 2D) = "black" {}
		_IllumIntensity("Illumination Intensity", Range(0, 1)) = 1
		_IllumCol("Illumination Color", Color) = (1.0, 1.0, 1.0, 1)
        _Color("Color", Color) = (0.5, 0.5, 0.5, 1)

		/*---Affine Settings--*/
		[Toggle(_ALLOW_AFFINE_MAPPING)] _AllowAfineMapping("Allow Affine Mapping", Float) = 1
		_AffineBlend("Affine Blend", Range(0, 1)) = 1
    }
		Category{
			SubShader
			{
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
						half2 uv : TEXCOORD0;
						half distance : TEXCOORD1;
						float3 cubenormal : TEXCOORD2;

						float3 vertPos : TEXCOORD3;
						#if defined(_ALLOW_AFFINE_MAPPING)
							half4 affineuv : TEXCOORD4;
						#endif

						fixed4 color : COLOR;
					};

					sampler2D _MainTex;
					sampler2D _ReflectionMask;
					float4 _MainTex_ST;
					sampler2D _Illum;
					float _IllumIntensity;
					float4 _IllumCol;
					float4 _Color;
					float3 pos;	
					
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
						o.vertPos = mul(unity_ObjectToWorld, v.vertex);

						/*---Distance To Camera---*/
						o.distance = GetClippingDistance(v.vertex, 0.3, _VertexSnapping);

						/*---Color---*/
						fixed4 color = v.color;
						color.a = 1.0f;
						o.color = color * _Color;
						
						/*---Uvs---*/
						half2 uv = TRANSFORM_TEX(v.texcoord, _MainTex);

						#if defined(_ALLOW_AFFINE_MAPPING)
							o.uv = uv;
							o.affineuv = CalculateAffineUvs(_AffineTextureMapping, uv, o.vertex);
						#else
							o.uv = uv;
						#endif

						/*---Cube Normal---*/
						o.cubenormal = mul(unity_ObjectToWorld, v.normal);
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

						float4 finalCol = i.color;
						finalCol.a = 1.0f;
						fixed4 col = (tex2D(_MainTex, uv) * finalCol);
						fixed4 refCol = tex2D(_ReflectionMask, uv);

						float tempAlpha = col.a;

						float3 reflectDir = -reflect(_WorldSpaceCameraPos.xyz - i.vertPos, normalize(i.cubenormal));
						fixed4 cube = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, reflectDir);
						col += tex2D(_Illum, uv) * _IllumIntensity * _IllumCol;
						col.xyz += DecodeHDR(cube, unity_SpecCube0_HDR) * 0.15f * refCol.rgb;
						col.a = tempAlpha;
						col.a = 1.0f;

						clip(i.distance < _RetroClippingDistance ? 1 : -1);
						return col;
					}

					ENDCG
				}
			}
		}
}
