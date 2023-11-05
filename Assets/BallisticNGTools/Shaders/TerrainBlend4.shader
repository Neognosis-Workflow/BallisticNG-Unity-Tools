// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "BallisticNG/Terrain/Terrain (4 Blend)"
{
    Properties
    {
        _MainTex("Top", 2D) = "white" {}
        _MainTex2("Side Upper", 2D) = "white" {}
        _MainTex3("Side Lower", 2D) = "white" {}
        _MainTex4("Bottom", 2D) = "white" {}
		_UvSettings("Uv Settings. X: Min Distance, Y: Max Distance, Y: Min Scale, W: Max Scale", Vector) = (1, 1, 1, 1)
		_FloorMin("Floor Min", Range(0, 1)) = 0
		_FloorMax("Floor Max", Range(0, 1)) = 1
		_WallDivideMin("Wall Divide Min", Float) = -0.5
		_WallDivideMax("Wall Divide Max", Float) = 0.5
		_ColorVariationFrequency("Color Variation Frequency", Float) = 0.5
		_ColorVariationAmount("Color Variation Amount", Float) = 0.5
		_ColorVariationTint("Color Variation Tint", Color) = (0, 0, 0, 0)
		_ColorVariationMin("Color Variation Min", Float) = 0
		_ColorVariationMax("Color Variation Max", Float) = 1
        _Color("Color", Color) = (0.5, 0.5, 0.5, 1)
    	
    	/*---Fog Settings---*/
	    _FogColor("Fog Color", Color) = (0.0, 0.0, 0.0, 0.0)
		_FogDistance("Fog Distance", Float) = 60

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
					#include "BallisticNG.cginc"
					#include "UnityCG.cginc"
					#include "ShaderExtras.cginc"

					#pragma multi_compile __ _ALLOW_AFFINE_MAPPING
					#pragma vertex vert
					#pragma fragment frag

					struct v2f
					{
						float4 vertex : SV_POSITION;
						half2 uv : TEXCOORD0;
						half3 textureBlend : TEXCOORD1;
						half2 distance : TEXCOORD2;
						
						#if defined(_ALLOW_AFFINE_MAPPING)
							half4 affineuv : TEXCOORD3;
						#endif

						fixed4 color : COLOR;
					};

					sampler2D _MainTex;
					sampler2D _MainTex2;
					sampler2D _MainTex3;
					sampler2D _MainTex4;
					float _FloorMin;
					float _FloorMax;
					float _WallDivideMin;
					float _WallDivideMax;
					float _ColorVariationFrequency;
					float _ColorVariationAmount;
					float4 _ColorVariationTint;
					float _ColorVariationMin;
					float _ColorVariationMax;
					float4 _MainTex_ST;
					float4 _Color;	
					float4 _UvSettings;
					
					/*---Retro Settings---*/
					half _RetroClippingDistance;
					half _AffineTextureMapping;
					half _AffineBlend;
					half _VertexSnapping;

					/*--Fog Settings---*/
					float4 _FogColor;
					float _FogDistance;

					v2f vert(appdata_full v)
					{
						v2f o;

						float4 worldPosition = mul(unity_ObjectToWorld, v.vertex);

						/*---Vertex Position---*/
						float4 vert = mul(UNITY_MATRIX_MV, v.vertex);

						float4 ps1Vert = TruncateVertex(v.vertex, GEO_RES);
						vert = lerp(vert, ps1Vert, _VertexSnapping);	

						float4 outVert = mul(UNITY_MATRIX_P, vert);
						o.vertex = outVert;
						
						/*---Distance To Camera---*/
						float dist = GetClippingDistance(v.vertex, 0.3, _VertexSnapping);

						o.distance.x = dist;
						o.distance.y = distance(_WorldSpaceCameraPos, worldPosition);
													
						/*---Uvs---*/
						half2 uv = TRANSFORM_TEX(v.texcoord, _MainTex);

						#if defined(_ALLOW_AFFINE_MAPPING)
							o.uv = uv;
							o.affineuv = CalculateAffineUvs(_AffineTextureMapping, uv, o.vertex);
						#else
							o.uv = uv;
						#endif

						o.color = v.color * _Color;

						float colorVariation = lerp(_ColorVariationMin, _ColorVariationMax, sin(length(worldPosition.xyz) * _ColorVariationFrequency) + 1.0f / 2.0f);
						o.color.rgb = lerp(o.color.rgb, o.color.rgb * _ColorVariationTint, colorVariation * _ColorVariationAmount);

						// normals
						half3 worldNormal = UnityObjectToWorldNormal(v.normal);
						float normalDot = dot(worldNormal, float3(0, 1, 0));
						float vertToHoriDot = 1 - abs(normalDot);

						o.textureBlend.x = InverseLerp(_FloorMin, _FloorMax, vertToHoriDot);
						o.textureBlend.y = 1.0f - round(normalDot + 1 / 2.0f);
						o.textureBlend.z = 1.0f - InverseLerp(_WallDivideMin, _WallDivideMax, worldPosition.y);

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

						float uvDistScale = InverseLerp(_UvSettings.x, _UvSettings.y, i.distance.y);
						float uvScale = lerp(_UvSettings.z, _UvSettings.w, uvDistScale);

						clip(i.distance.x < _RetroClippingDistance ? 1 : -1);

						fixed4 topMin = tex2D(_MainTex, uv);
						fixed4 bottomMin = tex2D(_MainTex4, uv);
						fixed4 wallsMin = tex2D(_MainTex2, uv);
						fixed4 walls2Min = tex2D(_MainTex3, uv);

						fixed4 topMax = tex2D(_MainTex, uv * _UvSettings.w);
						fixed4 bottomMax = tex2D(_MainTex4, uv * _UvSettings.w);
						fixed4 wallsMax = tex2D(_MainTex2, uv * _UvSettings.w);
						fixed4 walls2Max = tex2D(_MainTex3, uv * _UvSettings.w);

				        fixed4 top = lerp(topMin, topMax, uvDistScale);
						fixed4 bottom = lerp(bottomMin, bottomMax, uvDistScale);
						fixed4 walls = lerp(wallsMin, wallsMax, uvDistScale);
						fixed4 walls2 = lerp(walls2Min, walls2Max, uvDistScale);

						fixed4 texOut = lerp(lerp(top, bottom, i.textureBlend.y), lerp(walls, walls2, i.textureBlend.z), i.textureBlend.x) * i.color;
						const half fogT = clamp(i.distance.y, 0.0f, _FogDistance) / _FogDistance;
						return lerp(texOut, float4(_FogColor.rgb, 1.0), fogT * _FogColor.a);
					}

					ENDCG
				}
			}
		}
}
