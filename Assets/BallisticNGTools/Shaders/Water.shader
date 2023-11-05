// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "BallisticNG/Water"
{
    Properties
    {
        _MainTex("Water Surface", 2D) = "white" {}
        _Distortion("Distortion (Normal Map)", 2D) = "black" {}
		_DistortionMap("Distortion Intensity Map (Alpha)", 2D) = "white" {}
		_WaveShineMap("Wave Shine Map", 2D) = "black" {}
        _Color("Global Color", Color) = (1.0, 1.0, 1.0, 1)
        _WaveColor("Wave Shine Color", Color) = (1.0, 1.0, 1.0, 1)
		_WaveSize("Wave Size", Float) = 1
		_LightBlend("Light Blend", Range(0, 1)) = 1
		_RefractColor("Water Refractive Color", Color) = (1.0, 1.0, 1.0, 1.0)
    	
    	[Toggle(_FOG_USE_DEPTH_BUFFER)] _FogUseDepthBuffer("Fog Uses Depth Buffer", Float) = 0
		_FogColor("Water Fog Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_FogDistance("Water Fog Distance", Float) = 60
		_AlphaBlend("Alpha Blend", Range(0, 1)) = 0
		_WaveOpacityMinDistance("Wave Opacity Min Distance", Float) = 10
		_WaveOpacityMaxDistance("Wave Opacity Max Distance", Float) = 40
		_SurfaceAnimation("Surface Scroll", Vector) = (0, 0, 0, 0)
		_SurfaceOverlayAnimation("Surface Overlay Scroll", Vector) = (0, 0, 0, 0)
		_RefractionIntensity("Refraction Intensity", Float) = 1
		_ReflectionRefractionIntensity("Reflection Refraction Intensity", Float) = 1
		_BaseWave("Base Wave Speed", Vector) = (1, 1, 1, 1)
		_BlendWave("Blend Wave Speed", Vector) = (1, -1, 1, 1)
		_ReflectionColor("Reflection Color", Color) = (1, 1, 1, 1)
    	
    	[Toggle(_ALLOW_EDGE_BLEND)] _AllowEdgeBlend("Allow Edge Blend", Float) = 0
        _DepthColor("Depth Color", Color) = (1.0, 1.0, 1.0, 1.0)
    	_FadeSettings("Fade Settings (x: color power, y: alpha power, z: color blend, w: alpha blend", Vector) = (0, 0, 0, 0)
    	_UvSettings("Uv Settings (x: min distance, y: max distance, z: far scale, w: refraction add", Vector) = (0, 1, 1, 0)
    }
    SubShader
    {
		Cull Off
		Tags{"Queue" = "Transparent" "RenderType" = "Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha

		GrabPass
		{
			"_BackgroundTexture"
		}

		ZWrite Off
        Pass
        {
            CGPROGRAM

            #pragma multi_compile __ _ALLOW_EDGE_BLEND
            #pragma multi_compile __ _FOG_USE_DEPTH_BUFFER
            #include "UnityCG.cginc"
			#include "BallisticNG.cginc"
            #include "ShaderExtras.cginc"

            #pragma vertex vert
            #pragma fragment frag
			#pragma multi_compile_fog

            struct v2f
            {
                float4 position : SV_POSITION;
                half3 texcoord : TEXCOORD;
				half2 distance : TEXCOORD2;
				float4 grabPos : TexCOORD3;
				float3 cubenormal : TEXCOORD5;
				fixed4 color : COLOR;
            };

            sampler2D _MainTex;
			sampler2D _Distortion;
			sampler2D _DistortionMap;
			sampler2D _BackgroundTexture;
			sampler2D _WaveShineMap;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _DepthColor;
            float4 _FogColor;
            float4 _RefractColor;
			float _FogDistance;
			float _AlphaBlend;
			float _WaveOpacityMinDistance;
			float _WaveOpacityMaxDistance;
			float _WaveSize;
			float _LightBlend;
			float _RetroClippingDistance;
			float3 pos;
			float _VertexSnapping;

			float3 _SurfaceAnimation;
			float3 _SurfaceOverlayAnimation;
			float _RefractionIntensity;
			float _ReflectionRefractionIntensity;
			float4 _BaseWave;
			float4 _BlendWave;
			
			float4 _ReflectionColor;
			float3 _WaveColor;
			float4 _FadeSettings;
			float4 _UvSettings;

			fixed4 ApplyFog(float4 color, float distance)
			{
				#if !defined(FOG_LINEAR) && !defined(FOG_EXP) && !defined(FOG_EXP2)
					return color;
				#else
					UNITY_CALC_FOG_FACTOR_RAW(distance);

					color.rgb = lerp(unity_FogColor.rgb, color.rgb, saturate(unityFogFactor));
					return color;
				#endif
			}

            v2f vert(appdata_full v)
            {
                v2f o;

				float4 vert = v.vertex;

				// get vertex position
				float4 normalPosition = mul(UNITY_MATRIX_MV, vert);
				float4 ps1Position = TruncateVertex(vert, GEO_RES);
				float4 finalPosition = lerp(normalPosition, ps1Position, _VertexSnapping);

				// get distance for clipping
				float dist = GetClippingDistance(vert, 0.3, _VertexSnapping);

				// apply position and distance information
				o.position = mul(UNITY_MATRIX_P, finalPosition);
				o.distance.x = dist;
				o.distance.y = distance(_WorldSpaceCameraPos, mul(unity_ObjectToWorld, v.vertex));

                o.texcoord.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
				#ifdef _ALLOW_EDGE_BLEND
				o.texcoord.z = InverseLerp(_FadeSettings.x, _FadeSettings.y, length(ObjSpaceViewDir(v.vertex)));
				#endif				
				o.color = lerp(float4(1.0, 1.0, 1.0, 1.0), v.color, _LightBlend);
				o.grabPos = ComputeGrabScreenPos(UnityObjectToClipPos(v.vertex));

				float3 normal = mul(unity_ObjectToWorld, v.normal);
				float3 worldVert = mul(unity_ObjectToWorld, v.vertex);
				o.cubenormal = -reflect(_WorldSpaceCameraPos.xyz - worldVert, normalize(normal));

                return o;
            }

			float4 MultiUvSample(sampler2D tex, float2 uvClose, float2 uvFar, float blend)
			{
				float4 texA = tex2D(tex, uvClose);
				float4 texB = tex2D(tex, uvFar);
				return lerp(texA, texB, blend);
			}

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            fixed4 frag(v2f i) : SV_Target
            {
				clip(i.distance.x < _RetroClippingDistance ? 1 : -1);

				float uvT = InverseLerp(_UvSettings.x, _UvSettings.y, i.distance.y);
            	
            	/*---Background Distortion Uvs---*/
				half2 baseUvsClose = (i.texcoord * _BaseWave.zw) + float2(_Time.y * _BaseWave.x, _Time.y * _BaseWave.y);
				half2 baseUvsFar = (i.texcoord * _BaseWave.zw * _UvSettings.z) + float2(_Time.y * _BaseWave.x * _UvSettings.z, _Time.y * _BaseWave.y * _UvSettings.z);
            	
				half2 baseUvs2Close = float2(i.texcoord.y * _BlendWave.z, i.texcoord.x * _BlendWave.w) + float2(_Time.y * _BlendWave.x, -_Time.y * _BlendWave.y);
				half2 baseUvs2Far = float2(i.texcoord.y * _BlendWave.z * _UvSettings.z, i.texcoord.x * _BlendWave.w * _UvSettings.z) + float2(_Time.y * _BlendWave.x * _UvSettings.z, -_Time.y * _BlendWave.y * _UvSettings.z);
            	
				fixed3 distortion = UnpackNormal(MultiUvSample(_Distortion, baseUvsClose, baseUvsFar, uvT));
				fixed3 distortion2 = UnpackNormal(MultiUvSample(_Distortion, baseUvs2Close, baseUvs2Far, uvT));

            	float2 distort = (distortion.xyz * distortion2.xyz) * _RefractionIntensity;
				float2 distort2Close = (distortion.xyz * distortion2.xyz) * _ReflectionRefractionIntensity;
				float2 distort2Far = (distortion.xyz * distortion2.xyz) * (_ReflectionRefractionIntensity + _UvSettings.w);
				float2 distort2 = lerp(distort2Close, distort2Far, uvT);
				fixed2 uv = i.texcoord + distort;

            	fixed4 distortionMap = tex2D(_DistortionMap, uv + (_SurfaceAnimation * _Time.y));
            	
            	/*---Edge Blend---*/
            	#ifdef _ALLOW_EDGE_BLEND
            	float depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.grabPos));
				float sceneZ = LinearEyeDepth(depth);
            	float partZ = i.grabPos.w;
            	float fadeColor = 1.0f - saturate(_FadeSettings.x * (sceneZ - partZ));
                float fadeAlpha = 1.0f - saturate(_FadeSettings.y * (sceneZ - partZ));
            	float4 grabUvDist = i.grabPos + float4(distort.x, distort.y, 0.0f, 0.0f) * distortionMap.w * (1.0f - fadeAlpha);
            	#else
            	float4 grabUvDist = i.grabPos + float4(distort.x, distort.y, 0.0f, 0.0f) * distortionMap.w;
            	#endif

            	#ifdef _ALLOW_EDGE_BLEND
            	float4 surfaceColor = lerp(_Color, _DepthColor, (1.0f - fadeColor) * _FadeSettings.z);
            	#else
            	float4 surfaceColor = _Color;
            	#endif

            	fixed2 anim = _SurfaceAnimation * _Time.y;
            	fixed2 texUvClose = uv + anim;
            	fixed2 texUvFar = (uv * _UvSettings.z) + anim;
				fixed4 col = MultiUvSample(_MainTex, texUvClose, texUvFar, uvT) * i.color * surfaceColor;
				float aBlend = lerp(1, col.a, _AlphaBlend);

				fixed4 waveShine = tex2D(_WaveShineMap, (uv * _WaveSize) + (_SurfaceAnimation.xy * _Time.y));
				fixed4 waveShine2 = tex2D(_WaveShineMap, (uv * _WaveSize) + (_SurfaceOverlayAnimation.xy * _Time.y));
				fixed4 background = tex2Dproj(_BackgroundTexture, UNITY_PROJ_COORD(grabUvDist));
				fixed4 reflectionColor = _ReflectionColor * aBlend * UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.cubenormal + (float4(distort2.x, distort2.y, 0.0f, 0.0f) * distortionMap.w));

            	#if defined(_FOG_USE_DEPTH_BUFFER) && defined(_ALLOW_EDGE_BLEND)
				half fogDistance = saturate(_FogDistance * (sceneZ - partZ));
            	#else
            	half fogDistance = clamp(i.distance.y, 0.0f, _FogDistance) / _FogDistance;
            	#endif
				fixed3 foggedBackground = lerp(background.rgb * lerp(float3(1, 1, 1), _RefractColor.rgb, aBlend), _FogColor.rgb, fogDistance);

            	#ifdef _ALLOW_EDGE_BLEND

            	/*---Edge Blend Ver---*/
            	col.rgb = lerp(col.rgb, foggedBackground, 1 - col.a);
				col.rgb += reflectionColor.rgb * _ReflectionColor.a;
				col.rgb += ((waveShine * _SurfaceAnimation.z) + (waveShine2 * _SurfaceOverlayAnimation.z)) * _WaveColor * aBlend * (clamp((i.distance.y - _WaveOpacityMinDistance) / (_WaveOpacityMaxDistance - _WaveOpacityMinDistance), 0, 1));
				col.rgb = ApplyFog(col, i.distance.y);
				col.a = 1;

            	return float4(lerp(col.rgb, foggedBackground, fadeAlpha * _FadeSettings.w), 1.0);
            	#else
            	
            	/*---Non Edge Blend Ver---*/
				col.rgb = lerp(col.rgb, foggedBackground, 1 - col.a);
				col.rgb += reflectionColor.rgb * _ReflectionColor.a;
				col.rgb += ((waveShine * _SurfaceAnimation.z) + (waveShine2 * _SurfaceOverlayAnimation.z)) * _WaveColor * aBlend * (clamp((i.distance.y - _WaveOpacityMinDistance) / (_WaveOpacityMaxDistance - _WaveOpacityMinDistance), 0, 1));
				col.rgb = ApplyFog(col, i.distance.y);
				col.a = 1;

				return col;
            	
            	#endif
            	
            
           
            }

            ENDCG
        }
    }
}
