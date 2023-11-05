Shader "BallisticNG/Holofield" {
Properties {
	_MainTex ("Hologram Texture", 2D) = "white" {}
	_FillColor ("Fill Color", Color) = (1.0, 1.0, 1.0, 0.0)
	_DistortionDir ("Distortion (XY)", 2D) = "black" {}
	_DistortionAmount ("Distortion Amount", Vector) = (1, 1, 0, 0)
	_DistortionScale ("Distortion Scale", Vector) = (1, 1, 0, 0)
    _TintColor ("Tint Color", Color) = (1.0, 1.0, 1.0, 1.0)
    _GlowColor ("Glow Color", Color) = (1.0, 1.0, 1.0, 1.0)
	_InvFade ("Glow Power", Float) = 1.0
    _RimGlow ("Rim Color", Color) = (1.0, 1.0, 1.0, 1.0)
	_RimPower("Rim Power", Float) = 1.0
	_MinDistance("Min Fade Distance", Float) = 0.0
	_MinAlpha("Min Fade Alpha", Float) = 0.0
	_MaxDistance("Max Fade Distance", Float ) = 1.0
	_MaxAlpha("Max Fade Alpha", Float ) = 1.0
	_Anim("Animation Scroll", Vector ) = (0.5, 0, 0, 0)
	_AnimDistortion("Distortion Animation Scroll", Vector ) = (0.5, 0, 0, 0)
	_FogBlend("Fog Blend", Float) = 1.0
}

Category {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
    Blend SrcAlpha One
    ColorMask RGB
    Cull Off Lighting Off ZWrite Off

    SubShader {
        Pass {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
			#include "ShaderExtras.cginc"

            sampler2D _MainTex;
            sampler2D _DistortionDir;
            float2 _DistortionAmount;
            float2 _DistortionScale;
            fixed4 _TintColor;
            fixed4 _GlowColor;
            fixed4 _FillColor;
            fixed4 _RimGlow;
            float _RimPower;

            struct appdata_t {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float3 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 projPos : TEXCOORD2;
                float rim : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _MainTex_ST;
			float _MinDistance;
			float _MinAlpha;
			float _MaxDistance;
			float _MaxAlpha;
			float4 _Anim;
			float4 _AnimDistortion;
			float _FogBlend;

            v2f vert (appdata_full v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.projPos = ComputeScreenPos (o.vertex);
                COMPUTE_EYEDEPTH(o.projPos.z);
                o.color = v.color;
                o.texcoord.xy = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.texcoord.z = InverseLerp(_MinDistance, _MaxDistance, length(ObjSpaceViewDir(v.vertex)));
            	
            	float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
            	float dotProduct = 1.0f - abs(dot(v.normal,viewDir));
            	o.rim = smoothstep(1.0f - _RimPower, 1.0f, dotProduct);
            	
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            float _InvFade;

            fixed4 frag (v2f i) : SV_Target
            {
                float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
                float partZ = i.projPos.z;
                float fade = 1.0f - saturate (_InvFade * (sceneZ-partZ));

				float4 baseColor = lerp(_TintColor, _RimGlow, i.rim);

				float4 distortion = tex2D(_DistortionDir, (i.texcoord.xy + float2(_AnimDistortion.x * _Time.y, _AnimDistortion.y * _Time.y)) * _DistortionScale);
            	float2 distortedUvs = i.texcoord.xy + (distortion.xy * _DistortionAmount);
            	
				float4 texA = tex2D(_MainTex, distortedUvs + float2(_Anim.x * _Time.y, _Anim.y * _Time.y));
				float4 texB = tex2D(_MainTex, distortedUvs + float2(_Anim.z * _Time.y, _Anim.w * _Time.y));
            	float4 tex = (texA + texB) / 2.0f;
            	
            	float texSum = tex.rgb;
            	tex = lerp(_FillColor, tex, texSum);
            	
                fixed4 col = 2.0f * i.color * lerp(baseColor * tex, _GlowColor, fade);
				col.a *= i.texcoord.z;
                if (_FogBlend > 0.0) UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0,0,0,0)); // fog towards black due to our blend mode
                return col;
            }
            ENDCG
        }
    }
}
}
