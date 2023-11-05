Shader "BallisticNG/Track Pad (Animated)"
{
    Properties
    {
        _MainTex("Base", 2D) = "white" {}
		_Illum("Illumination", 2D) = "black" {}
		_IllumIntensity("Illumination Intensity", Range(0, 1)) = 1
        _Color("Color", Color) = (0.5, 0.5, 0.5, 1)
        _IllumGradient("Illumination Gradient", 2D) = "black" {}
        _IllumAnimSpeed("Illum Animation Speed", Float) = 0.5
    	_IllumAnimSeedMul("Illum Anim Seed Mult", Float) = 0.2
    }
		Category{
			SubShader
			{
				Pass
				{
					CGPROGRAM
					#include "UnityCG.cginc"
					#include "BallisticNG.cginc"

					#pragma vertex vert
					#pragma fragment frag

					struct v2f
					{
						float4 vertex : SV_POSITION;
						half2 distance : TEXCOORD1;
						half2 uv : TEXCOORD;
						half animSeed : TEXCOORD2;
						fixed4 color : COLOR;
					};

					sampler2D _MainTex;
					sampler2D _Illum;
					float _IllumIntensity;
					float4 _MainTex_ST;
					float4 _Color;
					sampler2D _IllumGradient;
					float _IllumAnimSpeed;
					float _IllumAnimSeedMul;
					float3 pos;

					/*---Retro Settings---*/
					half _RetroClippingDistance;
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
						
						o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
						o.color = v.color * _Color;

						o.animSeed = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
						return o;
					}

					fixed4 frag(v2f i) : SV_Target
					{
						//float2 uv = i.texcoord;
						clip(i.distance.x < _RetroClippingDistance ? 1 : -1);

						float seed = _IllumAnimSeedMul * i.animSeed;
						float4 illumColor = tex2D(_IllumGradient, float2((seed + _Time.y) * _IllumAnimSpeed % 1.0f, 0.0f));
						return (tex2D(_MainTex, i.uv) * i.color) + (tex2D(_Illum, i.uv) * _IllumIntensity * illumColor);
					}

					ENDCG
				}
			}
		}
}
