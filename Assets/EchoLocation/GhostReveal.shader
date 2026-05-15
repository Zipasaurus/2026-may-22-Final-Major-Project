Shader "Hidden/EcholocationOverlay"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _GhostColor ("Ghostly Color", Color) = (0.2, 0.4, 0.8, 0.3)
        _RingColor ("Ring Edge Color", Color) = (0, 1, 1, 1)
        _RingWidth ("Ring Width", Float) = 0.5
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture; // Unity automatically provides this
            float4x4 _InverseViewProj;

            fixed4 _GhostColor;
            fixed4 _RingColor;
            float _RingWidth;

            // Global variables from your EcholocationController script
            float3 _EchoCenter;
            float _EchoRadius;
            float _EchoIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Function to reconstruct 3D world position from the 2D depth texture
            float3 GetWorldPositionFromDepth(float2 uv, float depth)
            {
                #if defined(UNITY_REVERSED_Z)
                    depth = 1.0 - depth;
                #endif
                
                float4 clipPos = float4(uv.x * 2.0 - 1.0, uv.y * 2.0 - 1.0, depth * 2.0 - 1.0, 1.0);
                float4 worldPos = mul(_InverseViewProj, clipPos);
                return worldPos.xyz / worldPos.w;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. Get the normal camera color
                fixed4 originalColor = tex2D(_MainTex, i.uv);

                // 2. Get the depth and calculate world position
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                
                // If it's the skybox (depth is exactly 1 or 0 depending on platform), ignore it
                #if defined(UNITY_REVERSED_Z)
                    if (depth == 0) return originalColor;
                #else
                    if (depth == 1) return originalColor;
                #endif

                float3 worldPos = GetWorldPositionFromDepth(i.uv, depth);

                // 3. Echolocation Math (Same as before)
                float dist = distance(worldPos, _EchoCenter);
                float distFromRing = _EchoRadius - dist;
                
                float ringEdge = saturate(1.0 - abs(distFromRing) / _RingWidth);
                ringEdge = pow(ringEdge, 3.0); 

                float isInsideRadius = step(0.0, distFromRing);

                // 4. Combine
                // Add the ring glow
                fixed4 finalColor = originalColor + (ringEdge * _RingColor * _EchoIntensity);
                
                // Add the ghost tint overlay inside the radius
                fixed4 ghostTint = lerp(originalColor, originalColor * _GhostColor, _GhostColor.a);
                finalColor = lerp(finalColor, ghostTint, isInsideRadius * (1.0 - ringEdge) * _EchoIntensity);

                return finalColor;
            }
            ENDCG
        }
    }
}