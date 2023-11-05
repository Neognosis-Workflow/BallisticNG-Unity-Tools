Shader "BallisticNG/VertexLit (No Clip)"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Illum ("Illumination", 2D) = "black" {}
		_Color("Color", Color) = (1, 1, 1, 1)

		/*---Affine Settings--*/
		[Toggle(_ALLOW_AFFINE_MAPPING)] _AllowAfineMapping("Allow Affine Mapping", Float) = 1
		_AffineBlend("Affine Blend", Range(0, 1)) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

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
                float2 uv : TEXCOORD;
				half distance : TEXCOORD1;

				#if defined(_ALLOW_AFFINE_MAPPING)
					half4 affineuv : TEXCOORD5;
				#endif

				fixed4 color : COLOR;
			};

			sampler2D _MainTex;
			sampler2D _Illum;
			float4 _MainTex_ST;
			float4 _Color;
						
			/*---Retro Settings---*/
			half _RetroClippingDistance;
			half _AffineTextureMapping;
			half _AffineBlend;
			half _VertexSnapping;
			
			v2f vert (appdata_full v)
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

				o.color = v.color * _Color;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				/*--Uv---*/
				float2 uv;

				#if defined(_ALLOW_AFFINE_MAPPING)
					uv = lerp(i.uv, i.affineuv.xy / i.affineuv.z, i.affineuv.w * _AffineBlend);
				#else
					uv = i.uv.xy;
				#endif

				// sample texture
				fixed4 col = tex2D(_MainTex, uv) * i.color;
				// add illum
				col += tex2D(_Illum, uv);

				return col;
			}
			ENDCG
		}
	}
}
