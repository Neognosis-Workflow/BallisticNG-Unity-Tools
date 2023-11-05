Shader "BallisticNG/Ship Iridescent"
{
    Properties
    {
        _MainTex("Base", 2D) = "white" {}
        _IridescentMask("Iridescent Mask", 2D) = "white" {}
        _ReflectionMask("Reeflection", 2D) = "white" {}
		_Illum("Illumination", 2D) = "black" {}
		_IllumIntensity("Illumination Intensity", Range(0, 1)) = 1
		_IllumCol("Illumination Color", Color) = (1.0, 1.0, 1.0, 1)
        _Color("Color", Color) = (0.5, 0.5, 0.5, 1)
		_IridescentColor1("Iridescent Color 1", Color) = (0.0, 0.7, 1.0, 1.0)
		_IridescentColor2("Iridescent Color 2", Color) = (0.8, 0.4, 1.0, 1.0)
		_IridescentPower("Iridescent Power", Float) = 1.0
		_IridescentAngle("Iridescent Angle", Float) = 1.0
		_ColorShiftFrequency("Color Shift Frequency", Float) = 1.0
		_ColorShiftIntensity("Color Shift Intesity", Range(0, 2)) = 1.0
		_PhyscoBlend("Psycodelic Illum Blend", Range(0, 1)) = 0.0
		[Toggle(_ALLOW_AFFINE_MAPPING)] _AllowAfineMapping("Allow Afine Mapping", Float) = 1
		_AffineBlend("Affine Blend", Range(0, 1)) = 1
    }
		Category{
			SubShader
			{
				Pass
				{
					CGPROGRAM
					#pragma multi_compile __ _ALLOW_AFFINE_MAPPING
					#include "UnityCG.cginc"
					#include "BallisticNG.cginc"

					#pragma vertex vert
					#pragma fragment frag

					struct v2f
					{
						float4 position : SV_POSITION;
						half2 texcoord : TEXCOORD;
						half2 distance : TEXCOORD1;
						float3 cubenormal : TEXCOORD2;
						float3 vertPos : TEXCOORD3;
						#if defined(_ALLOW_AFFINE_MAPPING)
							half4 affineTexCoord : TEXCOORD4;
						#endif
						float3 vertObjSpace : TEXCOORD5;
						fixed4 color : COLOR;
					};

					sampler2D _MainTex;
					sampler2D _IridescentMask;
					sampler2D _ReflectionMask;
					float4 _MainTex_ST;
					sampler2D _Illum;
					float _IllumIntensity;
					float4 _IllumCol;
					float4 _Color;
					float4 _IridescentColor1;
					float4 _IridescentColor2;
					float _IridescentPower;
					float _IridescentAngle;
					float _ColorShiftFrequency;
					float _ColorShiftIntensity;
					float _PhyscoBlend;
					float _RetroClippingDistance;
					float _AffineTextureMapping;
					float _AffineBlend;
					float3 pos;
					float ps1effects;
					float shiprefl;
					uniform float4x4 _Rotation;

					v2f vert(appdata_full v)
					{
						v2f o;

						// get vertex position
						float4 normalPosition = mul(UNITY_MATRIX_MV, v.vertex);
						float4 ps1Position = TruncateVertex(v.vertex, GEO_RES);
						float4 finalPosition = lerp(normalPosition, ps1Position, ps1effects);

						// get distance for clipping
						float dist = GetClippingDistance(v.vertex, 0.3, ps1effects);

						// apply position and distance information
						o.position = mul(UNITY_MATRIX_P, finalPosition);
						o.vertObjSpace = v.vertex;
						o.vertPos =  mul(unity_ObjectToWorld, v.vertex);
						o.distance = finalPosition.xy;
						o.distance.x = dist;

						//OLD: o.cubenormal = mul(_Rotation, mul(unity_ObjectToWorld, v.normal));
						o.cubenormal = mul(unity_ObjectToWorld, v.normal);
						fixed4 newC = v.color;
						newC.a = 1.0;
						o.color = newC * _Color;

						/*---Uvs---*/
						half2 uv = TRANSFORM_TEX(v.texcoord, _MainTex);

						#if defined(_ALLOW_AFFINE_MAPPING)
							o.texcoord = uv;
							o.affineTexCoord = CalculateAffineUvs(_AffineTextureMapping, uv, o.position);
						#else
							o.texcoord = uv;
						#endif

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

						float4 finalCol = i.color;
						finalCol.a = 1.0f;
						fixed4 col = (tex2D(_MainTex, uv) * finalCol);
						fixed4 refCol = tex2D(_ReflectionMask, uv);
						i.distance.y = col.a;

						/*---Iridescent---*/
						fixed4 iridMask = tex2D(_IridescentMask, uv);

						float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.vertPos);
						float3 normalDir = normalize(i.cubenormal);
						float3 camDir = normalize(_WorldSpaceCameraPos.xyz - i.vertPos);
						float d = pow(clamp(dot(normalDir, viewDir), 0, 1), _IridescentAngle);

						float3 colorFreqIn = (i.vertObjSpace + (i.position * 0.01f)) * dot(normalDir, viewDir) * 0.1f * 3.141f * _ColorShiftFrequency;
						float3 iridescentCol = clamp(lerp(_IridescentColor1, _IridescentColor2, pow(d, _IridescentPower)) + (abs(sin(colorFreqIn))  * _ColorShiftIntensity * 0.2f), 0, 1);

						col = lerp(col, lerp(float4(iridescentCol, 1.0), float4(1.0, 1.0, 1.0, 1.0), 1 - iridMask) * lerp(_Color, float4(1.0, 1.0, 1.0, 1.0f), 0.4), iridMask);

						float3 reflectDir = -reflect(camDir, normalize(i.cubenormal));
						fixed4 cube = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, reflectDir);

						float4 illumCol = tex2D(_Illum, uv) * _IllumIntensity * _IllumCol;
						col += lerp(illumCol, illumCol * (1.0 - float4(iridescentCol.xyz, 1.0)), _PhyscoBlend);

						float3 cubeColor = DecodeHDR(cube, unity_SpecCube0_HDR) * 0.15f * shiprefl * refCol.rgb;

						float reflectionBlend = lerp(cubeColor.xyz, iridMask.xyz, iridMask.xyz);
						col.xyz += lerp(cubeColor, cubeColor * iridMask.xyz * 2.0f * iridescentCol * 2.0f, clamp(reflectionBlend, 0.4, 1));
						col.a = i.distance.y;
						col.a = 1.0f;
						
						clip(i.distance.x < _RetroClippingDistance ? 1 : -1);
						return col;
					}

					ENDCG
				}
			}
		}
}
