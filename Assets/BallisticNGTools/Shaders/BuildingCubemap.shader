Shader "BallisticNG/Building (Cubemap)"
{
    Properties
    {
        _MainTex("Base", 2D) = "white" {}
		_Illum("Illumination", 2D) = "black" {}
		_IllumIntensity("Illumination Intensity", Range(0, 1)) = 1
        _Color("Color", Color) = (0.5, 0.5, 0.5, 1)
		_ReflectionColor("Reflection Color", 2D) = "black" {}
		_ReflectionCUbe("Reflection Map(RGB)", CUBE) = "" { }

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
						float3 cubenormal : TEXCOORD0;
						half2 uv : TEXCOORD1;

						float distance : TEXCOORD2;

						#if defined(_ALLOW_AFFINE_MAPPING)
							half4 affineuv : TEXCOORD3;
						#endif

						fixed4 color : COLOR;
					};

					sampler2D _MainTex;
					sampler2D _Illum;
					sampler2D _ReflectionColor;
					float _IllumIntensity;
					float4 _MainTex_ST;
					float4 _Color;
					samplerCUBE _ReflectionCUbe;

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

						/*---Color---*/
						o.color = v.color * _Color;

						/*---Uvs---*/
						half2 uv = TRANSFORM_TEX(v.texcoord, _MainTex);

						#if defined(_ALLOW_AFFINE_MAPPING)
							o.uv = uv;
							o.affineuv = CalculateAffineUvs(_AffineTextureMapping, uv, o.vertex);
						#else
							o.uv = uv;
						#endif

						/*---Cubemap Normals---*/
						float3 normal = mul(unity_ObjectToWorld, v.normal);
						float3 worldVert = mul(unity_ObjectToWorld, v.vertex);
						o.cubenormal = -reflect(_WorldSpaceCameraPos.xyz - worldVert, normalize(normal));

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

						fixed4 cube = texCUBE(_ReflectionCUbe, i.cubenormal);
						cube *= tex2D(_ReflectionColor, uv);

						clip(i.distance < _RetroClippingDistance ? 1 : -1);
						return (tex2D(_MainTex, uv) * i.color + cube) + (tex2D(_Illum, uv) * _IllumIntensity);
					}

					ENDCG
				}
			}
		}
}
