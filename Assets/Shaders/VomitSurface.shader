// PBR surface shader for vomit-painted ground.
// Assign this material to surfaces that have a VomitSurface component —
// VomitSurface.cs will inject _InkTex, _InkNormalTex, and _RippleTex at runtime.
Shader "Custom/VomitSurface"
{
    Properties
    {
        _MainTex        ("Base Albedo",    2D)         = "white" {}
        _Color          ("Base Color",     Color)      = (1,1,1,1)
        _VomitColor     ("Vomit Color",    Color)      = (0.55, 0.78, 0.04, 1)

        [HideInInspector] _InkTex       ("Ink Mask",       2D) = "black" {}
        [HideInInspector] _InkNormalTex ("Ink Normals",    2D) = "bump"  {}
        [HideInInspector] _RippleTex    ("Ripple Heights", 2D) = "black" {}

        _NormalStrength  ("Ink Normal Strength",    Range(0,4)) = 1.5
        _RippleStrength  ("Ripple Normal Strength", Range(0,6)) = 3.0
        _RippleTexelSize ("Ripple Texel Size",      Float)      = 0.00390625
        _WetSmoothness   ("Wet Smoothness",         Range(0,1)) = 0.88
        _DrySmoothness   ("Dry Smoothness",         Range(0,1)) = 0.35
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _InkTex;
        sampler2D _InkNormalTex;
        sampler2D _RippleTex;

        float4 _Color;
        float4 _VomitColor;
        float  _NormalStrength;
        float  _RippleStrength;
        float  _RippleTexelSize;
        float  _WetSmoothness;
        float  _DrySmoothness;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_InkTex;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv  = IN.uv_InkTex;
            float  ink = saturate(tex2D(_InkTex, uv).r);

            // Albedo: blend base surface into vomit colour
            float3 base = tex2D(_MainTex, IN.uv_MainTex).rgb * _Color.rgb;
            o.Albedo    = lerp(base, _VomitColor.rgb, ink);

            // Ink normal (Sobel-precomputed, stored as packed [0,1] RGB)
            float3 inkN = tex2D(_InkNormalTex, uv).rgb * 2.0 - 1.0;
            inkN.xy *= _NormalStrength * ink;

            // Ripple normal: compute gradient of wave height field on the fly
            float ts  = _RippleTexelSize;
            float hL  = tex2D(_RippleTex, uv + float2(-ts,  0)).r;
            float hR  = tex2D(_RippleTex, uv + float2( ts,  0)).r;
            float hD  = tex2D(_RippleTex, uv + float2(  0, -ts)).r;
            float hU  = tex2D(_RippleTex, uv + float2(  0,  ts)).r;
            float2 rippleGrad = float2(hL - hR, hD - hU) * _RippleStrength * ink;

            // UDN blend: add XY contributions, keep Z = 1 (tangent space)
            o.Normal     = normalize(float3(inkN.xy + rippleGrad, 1.0));

            o.Metallic   = 0.0;
            o.Smoothness = lerp(_DrySmoothness, _WetSmoothness, ink);
            o.Alpha      = 1.0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
