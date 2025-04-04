Shader "Editor/Editor Highlight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _MaxDistance("Max Distance", Float) = 100
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+100"}
        Cull Off
        //ZTest Always
        Offset -1, -1
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _MaxDistance;
                        fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv.z = distance(_WorldSpaceCameraPos, mul(unity_ObjectToWorld, v.vertex));
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv.xy);
                fixed4 finalCol = lerp(_Color, tex, tex.rgba);
                finalCol.a *= 1 - clamp(i.uv.z / _MaxDistance, 0, 1);
                return finalCol;
            }
            ENDCG
        }
    }
}
