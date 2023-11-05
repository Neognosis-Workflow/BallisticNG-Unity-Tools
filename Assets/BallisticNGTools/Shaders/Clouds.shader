// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "BallisticNG/Clouds"
{
    Properties
    {
        _MainTex("Clouds Base", 2D) = "white" {}
        _SecondTex("Clouds Subtract", 2D) = "white" {}
		_CloudMask("Cloud Mask", 2D) = "white" {}
		_Color("Color", Color) = (1, 1, 1, 1)
		_Color2("Darker Color", Color) = (1, 1, 1, 1)
		_Density("Density", Float) = 1
		_Intensity("Intensity", Float) = 1
		_Wind("Wind", Vector) = (0, 0, 0, 0)
    }
		Category{
			Cull Off
			SubShader
			{
				Tags{ "Queue" = "AlphaTest" "IgnoreProjector" = "False" "RenderType" = "Background" }
				Blend SrcAlpha OneMinusSrcAlpha
				Pass
				{
					CGPROGRAM
					#include "UnityCG.cginc"

					#pragma vertex vert
					#pragma fragment frag

					struct v2f
					{
						float4 position : SV_POSITION;
						half2 texcoord3 : TEXCOORD2;
						half2 texcoord2 : TEXCOORD1;
						half2 texcoord : TEXCOORD;
						fixed4 color : COLOR;
					};

					sampler2D _MainTex;
					sampler2D _SecondTex;
					sampler2D _CloudMask;
					float4 _MainTex_ST;
					float4 _SecondTex_ST;
					float4 _CloudMask_ST;
					float4 _Color;
					float4 _Color2;
					float3 pos;
					fixed _Cutoff;
					float ps1effects;

					float4 _Wind;
					float _Density;
					float _Intensity;

					v2f vert(appdata_full v)
					{
						v2f o;

						// apply position and distance information
						o.position = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_MV, v.vertex));
						o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
						o.texcoord2 = TRANSFORM_TEX(v.texcoord, _SecondTex);
						o.texcoord3 = TRANSFORM_TEX(v.texcoord, _CloudMask);

						o.color = _Color;
						return o;
					}

					fixed4 frag(v2f i) : SV_Target
					{
						half2 baseOffset = i.texcoord;
						half2 subtractoffset = i.texcoord2;

						baseOffset.xy += _Time * _Wind.xy;
						subtractoffset.xy += _Time * _Wind.zw;

						fixed4 col = tex2D(_MainTex, baseOffset) * _Density;
						col -= tex2D(_SecondTex, subtractoffset);
						float alpha = col.rgb;

						col.rgb *= _Intensity;

						float3 uninterpolatedColor = col.rgb;
						col.rgb *= lerp(i.color, _Color2, 1 - clamp(col.rgb, 0, 1));

						alpha *= lerp(i.color.a, _Color2.a, 1 - clamp(uninterpolatedColor, 0, 1));
						col.a = alpha * tex2D(_CloudMask, i.texcoord3);

						return col;
					}

					ENDCG
				}
			}
		}
}
