Shader "BallisticNG/Track Pad"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Illum ("Illumination", 2D) = "black" {}
		_Color("Color", Color) = (1, 1, 1, 1)
		_IllumColor("Illum Color", Color) = (1, 1, 1, 1)
		_IllumIntensity("Illum Intensity", Range(0, 1)) = 1
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
				fixed4 color : COLOR;
			};

			sampler2D _MainTex;
			sampler2D _Illum;
			float4 _MainTex_ST;
			float4 _Color;
			float4 _IllumColor;
			float _IllumIntensity;			

			/*---Retro Settings---*/
			half _RetroClippingDistance;
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

				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.color = v.color * _Color;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample texture
				fixed4 col = tex2D(_MainTex, i.uv) * i.color;
				// add illum
				col += tex2D(_Illum, i.uv) * _IllumColor * _IllumIntensity;

				clip(i.distance < _RetroClippingDistance ? 1 : -1);
				return col;
			}
			ENDCG
		}
	}
}
