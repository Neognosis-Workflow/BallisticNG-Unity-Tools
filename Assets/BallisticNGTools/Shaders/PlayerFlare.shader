Shader "BallisticNG/Ships/Player Flare" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Texture", 2D) = "white" {}
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Trasnparent" "PreviewType"="Plane" }
	Blend SrcAlpha One
	ColorMask RGB
	Lighting Off ZWrite Off ZTest Always
	
	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_particles
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _TintColor;

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float3 pos : TEXCOORD1;
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_full v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = float4(1, 1, 1, 1);
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.pos = mul(unity_ObjectToWorld, v.vertex);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord);
				return col;
			}
			ENDCG 
		}
	}	
}
}
