// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "BallisticNG/Survival Skybox" {
Properties {
    _SkyTint ("Sky Tint", Color) = (.5, .5, .5, 1)
}

SubShader {
    Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
    Cull Off ZWrite Off

    Pass {

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag

        #include "UnityCG.cginc"
        #include "Lighting.cginc"
        uniform half3 _SkyTint;

        int _SkyScanLines;

        struct appdata_t
        {
            float4 vertex : POSITION;
        };

        struct v2f
        {
            float4 position : SV_POSITION;
			float4 sPos : TEXCOORD1;
			float4 pos : TEXCOORD2;
        };

        v2f vert (appdata_t v)
        {
            v2f o;
            o.position = UnityObjectToClipPos(v.vertex);
			o.sPos = ComputeScreenPos(o.position);
			o.pos = mul(unity_ObjectToWorld, v.vertex);
            return o;
        }

        half4 frag (v2f i) : SV_Target
        {
        	// compute gradient on sphere
        	float2 screenUv = i.sPos.xy / i.sPos.w;
        	float2 nc = screenUv * 2.0f - 1.0f;

        	float4 clipPos = float4(nc, 1.0f, 1.0f);
        	float4 viewPos = mul(unity_CameraInvProjection, clipPos);
        	float3 viewDir = normalize(viewPos.xyz / viewPos.w);
        	float3 worldDir = mul((float3x3)unity_MatrixInvV, viewDir);

        	float gradientDot = (dot(normalize(worldDir), float3(0, 1, 0)) * 0.5f) + 0.8f;
        	float gradient = saturate(gradientDot);
        	
            half3 col = lerp(half3(0.0, 0.0, 0.0), half3(_SkyTint.r, _SkyTint.g, _SkyTint.b), gradient);

			fixed lineSize = _ScreenParams.y*0.005;
			float displacement = (_Time.y * 50)%_ScreenParams.y;
			float ps = displacement+(i.sPos.y * _ScreenParams.y / i.sPos.w);
			if (_SkyScanLines == 1) col += (ps / floor(0.4*lineSize) % 5) * 0.003;

            return half4(col,1.0);

        }
        ENDCG
    }
}


Fallback Off
}
