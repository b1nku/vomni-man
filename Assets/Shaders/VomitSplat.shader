Shader "Hidden/VomitSplat"
{
    Properties
    {
        _SplatTex ("Splat Brush", 2D) = "white" {}
        _SplatParams ("Center UV + Half-Size UV", Vector) = (0.5, 0.5, 0.1, 0.1)
    }
    SubShader
    {
        Blend One One
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _SplatTex;
            float4 _SplatParams;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f    { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 center   = _SplatParams.xy;
                float  halfSize = _SplatParams.z;

                float2 local = (i.uv - center) / (halfSize * 2.0); // -0.5 to 0.5
                float  dist  = length(local);
                float  angle = atan2(local.y, local.x);

                // Polar noise: overlapping sines on the angle warp the edge outline
                float warp = sin(angle * 5.0)  * 0.004
                           + sin(angle * 11.0) * 0.002
                           + sin(angle * 3 + 1.2) * 0.003;

                // Shrink effective distance by warp so edge becomes irregular
                float warpedDist = dist - warp;

                float2 splatUV = local / max(warpedDist, 0.001) * dist + 0.5;

                if (warpedDist > 0.5 || any(splatUV < 0) || any(splatUV > 1))
                    return float4(0, 0, 0, 0);

                return tex2D(_SplatTex, splatUV);
            }
            ENDCG
        }
    }
}
