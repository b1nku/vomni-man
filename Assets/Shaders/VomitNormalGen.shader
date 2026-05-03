Shader "Hidden/VomitNormalGen"
{
    Properties
    {
        _MainTex  ("Ink", 2D)            = "black" {}
        _TexelSize ("Texel Size", Float) = 0.001
        _Strength  ("Strength",   Float) = 2.0
    }
    SubShader
    {
        ZWrite Off ZTest Always Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _TexelSize;
            float _Strength;

            float h(float2 uv) { return saturate(tex2D(_MainTex, uv).r); }

            float4 frag(v2f_img i) : SV_Target
            {
                float ts = _TexelSize;

                // 3x3 Sobel
                float h00 = h(i.uv + float2(-ts,-ts)); float h10 = h(i.uv + float2(0,-ts)); float h20 = h(i.uv + float2( ts,-ts));
                float h01 = h(i.uv + float2(-ts,  0));                                       float h21 = h(i.uv + float2( ts,  0));
                float h02 = h(i.uv + float2(-ts, ts)); float h12 = h(i.uv + float2(0, ts)); float h22 = h(i.uv + float2( ts, ts));

                float dX = (h20 + 2*h21 + h22) - (h00 + 2*h01 + h02);
                float dY = (h02 + 2*h12 + h22) - (h00 + 2*h10 + h20);

                float3 n = normalize(float3(-dX * _Strength, -dY * _Strength, 1.0));
                return float4(n * 0.5 + 0.5, 1.0); // pack to [0,1]
            }
            ENDCG
        }
    }
}
