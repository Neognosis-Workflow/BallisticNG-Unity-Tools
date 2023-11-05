
Shader "BallisticNG/Hologram (Transparent)"
{
    Properties
    {
        _MainTex("Base", 2D) = "white" {}
		_ScreenTex("Screen Texture", 2D) = "black" {}
		_Illum("Illumination", 2D) = "black" {}
		_IllumIntensity("Illumination Intensity", Range(0, 1)) = 1
        _Color("Color", Color) = (0.5, 0.5, 0.5, 1)
		_RimColor("Rim Color", Color) = (0.5, 0.5, 0.5, 1)
		_IllumCol("Illumination Color", Color) = (1, 1, 1, 1)
		_ScreenScale("Screen Scale", Float) = 1
		_ScreenIntensity("Screen Intensity", Range(0, 1)) = 0
		_ScreenOffset("Screen Offset", Vector) = (0, 0, 0, 0)
		_ScreenScroll("Screen Scroll", Vector) = (0, 0, 0, 0)
		_WobbleScale("Wobble Scale", Float) = 1
		[Toggle(_ALLOW_AFFINE_MAPPING)] _AllowAfineMapping("Allow Afine Mapping", Float) = 1
		_AffineBlend("Affine Blend", Range(0, 1)) = 1
    }
    SubShader
    {
		//Cull Off
		Tags{"Queue" = "Transparent" "RenderType" = "Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha

		ZWrite Off
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
				half2 distance : TEXCOORD2;
				half3 screenPos : TEXCOORD3;
				#if defined(_ALLOW_AFFINE_MAPPING)
					half4 affineTexCoord : TEXCOORD4;
				#endif
				fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
			float4 _RimColor;
			float4 _IllumCol;
			sampler2D _Illum;
			sampler2D _ScreenTex;
			float _IllumIntensity;
			float _RetroClippingDistance;
			float _AffineTextureMapping;
			float _AffineBlend;
			float3 pos;
			float ps1effects;
			float _ScreenScale;
			float2 _ScreenOffset;
			float2 _ScreenScroll;
			float _ScreenIntensity;
			float _WobbleScale;

            v2f vert(appdata_full v)
            {
                v2f o;

				float4 vert = v.vertex;
				vert.x = vert.x + sin(_Time * 1000 * vert.x) * 0.01 * _WobbleScale;

				// get vertex position
				float4 normalPosition = mul(UNITY_MATRIX_MV, vert);
				float4 ps1Position = TruncateVertex(vert, GEO_RES);
				float4 finalPosition = lerp(normalPosition, ps1Position, ps1effects);

				// get distance for clipping
				float dist = GetClippingDistance(vert, 0.3, ps1effects);

				// apply position and distance information
				o.position = mul(UNITY_MATRIX_P, finalPosition);
				o.distance = finalPosition.xy;
				o.distance.x = dist;
				o.screenPos = mul(UNITY_MATRIX_MV, v.vertex);

				// rim lighting
				float3 vertPos = mul(unity_ObjectToWorld, vert).xyz;
				float3 normal = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
				float3 vDir = normalize(_WorldSpaceCameraPos.xyz - vertPos.xyz);
				half rim = pow(1.0 - saturate(dot(vDir, normal)), 4.0);
				float4 rimCol = _RimColor * rim;
				rimCol.a = 0.0;

				o.color = lerp(v.color * _Color, rimCol, rim);
				o.color.a *= clamp(vert.z + 2.0, 0.2, 1.0);
				o.color *= clamp(sin(_Time * 10), 0.8, 1.0);

				
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

				clip(i.distance.x < _RetroClippingDistance ? 1 : -1);
				fixed4 col = tex2D(_MainTex, uv) * i.color;
				col += tex2D(_Illum, uv) * _IllumIntensity * _IllumCol;

				// screen scroll
				_ScreenOffset.x = _Time *_ScreenScroll.x;
				_ScreenOffset.y = _Time *_ScreenScroll.y;
				col.rgb -= tex2D(_ScreenTex, ((i.screenPos.xy) * _ScreenScale) + _ScreenOffset) * _ScreenIntensity * col.rgba;

				return col;
            }

            ENDCG
        }
    }
}
