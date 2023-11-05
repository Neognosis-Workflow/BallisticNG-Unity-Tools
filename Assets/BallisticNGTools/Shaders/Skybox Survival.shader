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
			float gradient : TEXCOORD0;
			float4 sPos : TEXCOORD1;
			float4 pos : TEXCOORD2;
        };

        v2f vert (appdata_t v)
        {
            v2f o;
            o.position = UnityObjectToClipPos(v.vertex);
			o.sPos = ComputeScreenPos(o.position);
			o.pos = mul(unity_ObjectToWorld, v.vertex);

			o.gradient = clamp((v.vertex.y * 0.5) + 0.8, 0, 1);
            return o;
        }

        half4 frag (v2f i) : SV_Target
        {
            half3 col = lerp(half3(0.0, 0.0, 0.0), half3(_SkyTint.r, _SkyTint.g, _SkyTint.b), i.gradient);

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
