// Discrete wave equation on a single RGFloat texture.
// R = current wave height, G = previous wave height.
// Reads src, writes (next, curr) into dest — no read/write hazard.
Shader "Hidden/VomitRipple"
{
    Properties
    {
        _StateTex  ("State (RG)",  2D)    = "black" {}
        _TexelSize ("Texel Size",  Float) = 0.00390625  // 1/256
        _WaveSpeed ("Wave Speed",  Float) = 0.5
        _Damping   ("Damping",     Float) = 0.985
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

            sampler2D _StateTex;
            float _TexelSize;
            float _WaveSpeed;
            float _Damping;

            float4 frag(v2f_img i) : SV_Target
            {
                float ts = _TexelSize;
                float2 state = tex2D(_StateTex, i.uv).rg;
                float  curr  = state.r;
                float  prev  = state.g;

                float cL = tex2D(_StateTex, i.uv + float2(-ts,  0)).r;
                float cR = tex2D(_StateTex, i.uv + float2( ts,  0)).r;
                float cD = tex2D(_StateTex, i.uv + float2(  0, -ts)).r;
                float cU = tex2D(_StateTex, i.uv + float2(  0,  ts)).r;

                float laplacian = cL + cR + cD + cU - 4.0 * curr;
                float next = (2.0 * curr - prev + _WaveSpeed * _WaveSpeed * laplacian) * _Damping;

                return float4(clamp(next, -1.0, 1.0), curr, 0, 1);
                // R = new current, G = old current becomes new previous
            }
            ENDCG
        }
    }
}
