Shader "Custom/ExpandingRingGhost"
{
    Properties
    {
        _Color ("Main Ring Color", Color) = (1,1,1,1)
        _GhostColor ("Ghost Ring Color", Color) = (1,1,1,0.25)

        _RingRadius ("Ring Radius", Float) = 0
        _RingWidth ("Ring Width", Float) = 0.5

        _GhostOffset ("Ghost Distance Offset", Float) = 1.5

        _Center ("World Center", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalRenderPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos   : TEXCOORD0;
            };

            float4 _Color;
            float4 _GhostColor;

            float _RingRadius;
            float _RingWidth;
            float _GhostOffset;

            float3 _Center;

            Varyings vert (Attributes v)
            {
                Varyings o;
                float3 worldPos = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(worldPos);
                o.worldPos = worldPos;
                return o;
            }

            float Ring(float dist, float radius, float width)
            {
                return smoothstep(radius + width, radius, dist) *
                       smoothstep(radius - width, radius, dist);
            }

            half4 frag (Varyings i) : SV_Target
            {
                float dist = distance(i.worldPos.xz, _Center.xz);

                float mainRing  = Ring(dist, _RingRadius, _RingWidth);
                float ghostRing = Ring(dist, _RingRadius - _GhostOffset, _RingWidth);

                float4 col =
                    _Color * mainRing +
                    _GhostColor * ghostRing;

                return col;
            }
            ENDHLSL
        }
    }
}