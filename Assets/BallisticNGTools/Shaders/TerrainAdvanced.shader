Shader "BallisticNG/Terrain/Terrain Advanced"
{
    Properties
    {
		_NoisePeriod("Noise Period", Float) = 20
		_NoiseScale("Noise Scale", Float) = 1

        _MainTex("Red Texture", 2D) = "black" {}
		_RedNoiseTintA("Red Noise Tint A", Color) = (1, 1, 1, 1)
		_RedNoiseTintB("Red Noise Tint B", Color) = (1, 1, 1, 1)

		_Texture1("Green Texture", 2D) = "black" {}
		_GreenNoiseTintA("Green Noise Tint A", Color) = (1, 1, 1, 1)
		_GreenNoiseTintB("Green Noise Tint B", Color) = (1, 1, 1, 1)

		_Texture2("Blue Texture", 2D) = "black" {}
		_BlueNoiseTintA("Blue Noise Tint A", Color) = (1, 1, 1, 1)
		_BlueNoiseTintB("Blue Noise Tint B", Color) = (1, 1, 1, 1)

		_Texture3("Alpha Texture", 2D) = "black" {}
		_AlphaNoiseTintA("Alpha Noise Tint A", Color) = (1, 1, 1, 1)
		_AlphaNoiseTintB("Alpha Noise Tint B", Color) = (1, 1, 1, 1)

		_UvSettings("Uv Settings. X: Min Distance, Y: Max Distance, Y: Min Scale, W: Max Scale", Vector) = (1, 1, 1, 1)
        _Color("Color", Color) = (0.5, 0.5, 0.5, 1)
		[Toggle()] _CanClip("Allow Distance Clip", Float) = 1
		[Toggle(_ALLOW_AFFINE_MAPPING)] _AllowAfineMapping("Allow Afine Mapping", Float) = 1
		_AffineBlend("Affine Blend", Range(0, 1)) = 1
    	
	    _FogColor("Fog Color", Color) = (0.0, 0.0, 0.0, 0.0)
		_FogDistance("Fog Distance", Float) = 60
    }

	Category
	{
		SubShader
		{
			Pass
			{
				CGPROGRAM

				#pragma multi_compile __ _ALLOW_AFFINE_MAPPING
				#include "UnityCG.cginc"
				#include "BallisticNG.cginc"
				#include "ShaderExtras.cginc"
				#include "PerlinNoise3D.hlsl"

				#pragma vertex vert
				#pragma fragment frag

				struct v2f
				{
					float4 position : SV_POSITION;
					half2 texcoord : TEXCOORD;
					half2 distance : TEXCOORD1;
					float4 blends : TEXCOORD2;
					float noise : TEXCOORD3;
					#if defined(_ALLOW_AFFINE_MAPPING)
						half4 affineTexCoord : TEXCOORD5;
					#endif
					fixed4 color : COLOR;
				};

				sampler2D _MainTex;
				sampler2D _Texture1;
				sampler2D _Texture2;
				sampler2D _Texture3;
				float4 _MainTex_ST;
				float4 _Color;
				float4 _UvSettings;
				float _RetroClippingDistance;
				float _AffineTextureMapping;
				float _AffineBlend;
				float3 pos;
				float _VertexSnapping;

				float _NoisePeriod;
				float _NoiseScale;

				float4 _RedNoiseTintA;
				float4 _RedNoiseTintB;
				float4 _GreenNoiseTintA;
				float4 _GreenNoiseTintB;
				float4 _BlueNoiseTintA;
				float4 _BlueNoiseTintB;
				float4 _AlphaNoiseTintA;
				float4 _AlphaNoiseTintB;

				float4 _FogColor;
				float _FogDistance;
				
				bool _CanClip;

				float4 getUvBlendedTexture(sampler2D tex, float2 uv, float maxUvScale, float uvDist)
				{
					fixed4 min = tex2D(tex, uv);
					fixed4 max = tex2D(tex, uv * maxUvScale);
				    return lerp(min, max, uvDist);
				}

				v2f vert(appdata_full v)
				{
					v2f o;

					float4 worldPosition = mul(unity_ObjectToWorld, v.vertex);

					/*---Vertex Position---*/
					float4 vert = mul(UNITY_MATRIX_MV, v.vertex);

					float4 ps1Vert = TruncateVertex(v.vertex, GEO_RES);
					vert = lerp(vert, ps1Vert, _VertexSnapping);	

					float4 outVert = mul(UNITY_MATRIX_P, vert);
					o.position = outVert;

					/*---Distance To Camera---*/
					float dist = GetClippingDistance(v.vertex, 0.3, _VertexSnapping);

					o.distance.x = dist;
					o.distance.y = distance(_WorldSpaceCameraPos, worldPosition);

					/*---Uvs---*/
					half2 uv = TRANSFORM_TEX(v.texcoord, _MainTex);

					#if defined(_ALLOW_AFFINE_MAPPING)
						o.texcoord = uv;
						o.affineTexCoord = CalculateAffineUvs(_AffineTextureMapping, uv, o.position);
					#else
						o.texcoord = uv;
					#endif

					o.color = v.tangent * _Color;
					o.blends = v.color;
					o.noise = pnoise(worldPosition * _NoiseScale, _NoisePeriod);

					// normals
					half3 worldNormal = UnityObjectToWorldNormal(v.normal);
					float normalDot = dot(worldNormal, float3(0, 1, 0));
					float vertToHoriDot = 1 - abs(normalDot);

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					/*--Uv---*/
					float2 uv;

					#if defined(_ALLOW_AFFINE_MAPPING)
						uv = lerp(i.texcoord, i.affineTexCoord.xy / i.affineTexCoord.z, i.affineTexCoord.w * _AffineBlend);
					#else
						uv = i.texcoord.xy;
					#endif

					float uvDistScale = InverseLerp(_UvSettings.x, _UvSettings.y, i.distance.y);
					float uvScale = lerp(_UvSettings.z, _UvSettings.w, uvDistScale);

					if (_CanClip) clip(i.distance.x < _RetroClippingDistance ? 1 : -1);

					float noise = i.noise;
					float4 outTex = getUvBlendedTexture(_MainTex, uv, _UvSettings.w, uvDistScale) * i.blends.x * lerp(_RedNoiseTintA, _RedNoiseTintB, i.noise);
					outTex += getUvBlendedTexture(_Texture1, uv, _UvSettings.w, uvDistScale) * i.blends.y * lerp(_GreenNoiseTintA, _GreenNoiseTintB, i.noise);
					outTex += getUvBlendedTexture(_Texture2, uv, _UvSettings.w, uvDistScale) * i.blends.z * lerp(_BlueNoiseTintA, _BlueNoiseTintB, i.noise);
					outTex += getUvBlendedTexture(_Texture3, uv, _UvSettings.w, uvDistScale) * i.blends.w * lerp(_AlphaNoiseTintA, _AlphaNoiseTintB, i.noise);

					const half fogT = clamp(i.distance.y, 0.0f, _FogDistance) / _FogDistance;
					return lerp(outTex * i.color, float4(_FogColor.rgb, 1.0), fogT * _FogColor.a);
				}

				ENDCG
			}
		}
	}
}
