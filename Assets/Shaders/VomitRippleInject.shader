// Additively stamps a Gaussian depression into the current wave buffer (R channel).
// G channel (previous height) is left unchanged by the zero write + additive blend.
Shader "Hidden/VomitRippleInject"
{
    Properties
    {
        _InjectUV       ("Inject UV",       Vector) = (0.5, 0.5, 0, 0)
        _InjectRadius   ("Inject Radius",   Float)  = 0.06
        _InjectStrength ("Inject Strength", Float)  = 0.9
    }
    SubShader
    {
        Blend One One   // additive: existing value + fragment output
        ZWrite Off ZTest Always Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _InjectUV;
            float  _InjectRadius;
            float  _InjectStrength;

            float4 frag(v2f_img i) : SV_Target
            {
                float2 delta = i.uv - _InjectUV.xy;
                float  r     = _InjectRadius;
                float  g     = exp(-dot(delta, delta) / (r * r)) * _InjectStrength;
                return float4(-g, 0, 0, 0); // depression → outward pressure ring
            }
            ENDCG
        }
    }
}
