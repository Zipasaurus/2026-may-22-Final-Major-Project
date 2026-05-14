Shader "Custom/SwirlVortex"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Center ("Center", Vector) = (0.5, 0.5, 0, 0)     // UV center (0-1)
        _Strength ("Swirl Strength", Range(0, 20)) = 8.0
        _Radius ("Effect Radius", Range(0.1, 2)) = 1.0
        _Speed ("Rotation Speed", Range(-10, 10)) = -3.0   // Negative = clockwise
        _Falloff ("Falloff Power", Range(0.5, 5)) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

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
            float4 _MainTex_ST;
            float2 _Center;
            float _Strength;
            float _Radius;
            float _Speed;
            float _Falloff;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 center = _Center;
                
                // Vector from center
                float2 offset = uv - center;
                float dist = length(offset);
                
                // Normalized direction
                float2 dir = offset / (dist + 0.0001);
                
                // Swirl amount (stronger near center, falls off)
                float swirlAmount = _Strength * pow(saturate(1.0 - dist / _Radius), _Falloff);
                
                // Rotation (clockwise is negative angle)
                float angle = swirlAmount * _Time.y * _Speed;
                
                float s = sin(angle);
                float c = cos(angle);
                
                // Rotation matrix
                float2x2 rot = float2x2(c, -s, s, c);
                offset = mul(rot, offset);
                
                uv = center + offset;
                
                fixed4 col = tex2D(_MainTex, uv);
                
                // Optional: fade edges outside radius
                float alphaMask = smoothstep(_Radius + 0.1, _Radius - 0.2, dist);
                col.a *= alphaMask;
                
                return col;
            }
            ENDCG
        }
    }
}