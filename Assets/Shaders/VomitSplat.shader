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

                float2 splatUV = (i.uv - center) / (halfSize * 2.0) + 0.5;

                if (any(splatUV < 0) || any(splatUV > 1))
                    return float4(0, 0, 0, 0);

                return tex2D(_SplatTex, splatUV);
            }
            ENDCG
        }
    }
}
