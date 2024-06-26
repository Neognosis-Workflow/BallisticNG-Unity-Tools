// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "BallisticNG/Ships/AI Flare" {
Properties {
    [Header(Color Settings)][Space(10)] _TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
    _MainTex ("Particle Texture", 2D) = "white" {}
    _InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
    [Header(Interpolate Distance)][Space(10)] _MaxDistance ("Max Distance", Float) = 0.0
    _MinDistance ("Min Distance", Float) = 0.0
    
    [Header(Interpolate Values)][Space(10)] _MaxSize ("Max Size", Float) = 1.0
    _MinSize ("Min Size", Float) = 1.0
    _MaxTint ("Max Tint", Color) = (1.0, 1.0, 1.0, 1.0)
    _MinTint ("Min Tint", Color)= (1.0, 1.0, 1.0, 0.0)
    
    [Header(Depth Adjustment)][Space(10)] _DepthBias ("Depth Bias", Range(0, 1)) = 0.5
    _MaxDepthOffset ("Max Depth Offset", Float) = 0.2
}

Category {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "DisableBatching"="True"}
    Blend SrcAlpha One
    ColorMask RGB
    Lighting Off ZWrite Off
    
    SubShader {
        Pass {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_particles
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "ShaderExtras.cginc"

            sampler2D _MainTex;
            fixed4 _TintColor;
            fixed _MaxDistance;
            fixed _MinDistance;
            fixed _MaxSize;
            fixed _MinSize;
            fixed4 _MaxTint;
            fixed4 _MinTint;
            fixed _DepthBias;
            fixed _MaxDepthOffset;

            struct appdata_t {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                #ifdef SOFTPARTICLES_ON
                float4 projPos : TEXCOORD2;
                #endif
                float distance : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _MainTex_ST;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // vertex information
                float4 vertex = v.vertex;
                float3 cameraPos = _WorldSpaceCameraPos;

                float4 objClip = UnityObjectToClipPos(float4(0.0, 0.0, 0.0, 1.0));
                float objDepth = LinearEyeDepth(objClip.z / objClip.w);

                vertex.xyz *= lerp(_MinSize, _MaxSize, 1.0f - InverseLerp(_MinDistance, _MaxDistance, objDepth));
                o.distance = objDepth;
                
                // depth bias
                float4 worldVert = mul(unity_ObjectToWorld, vertex);
                float blendFactor = clamp(_MaxDepthOffset / objDepth, 0, 1);
                worldVert.xyz = lerp(worldVert.xyz, cameraPos, _DepthBias * blendFactor);
                
                o.vertex = UnityObjectToClipPos(mul(unity_WorldToObject, worldVert));
                
                #ifdef SOFTPARTICLES_ON
                o.projPos = ComputeScreenPos (o.vertex);
                COMPUTE_EYEDEPTH(o.projPos.z);
                #endif
                o.color = v.color;
                o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            float _InvFade;

            fixed4 frag (v2f i) : SV_Target
            {
                #ifdef SOFTPARTICLES_ON
                float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
                float partZ = i.projPos.z;
                float fade = saturate (_InvFade * (sceneZ-partZ));
                i.color.a *= fade;
                #endif

                fixed4 col = 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord);
                col.a = saturate(col.a); // alpha should not have double-brightness applied to it, but we can't fix that legacy behaior without breaking everyone's effects, so instead clamp the output to get sensible HDR behavior (case 967476)
                col *= lerp(_MinTint, _MaxTint, 1.0f - InverseLerp(_MinDistance, _MaxDistance, i.distance));

                UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0,0,0,0)); // fog towards black due to our blend mode
                return col;
            }
            ENDCG
        }
    }
}
}