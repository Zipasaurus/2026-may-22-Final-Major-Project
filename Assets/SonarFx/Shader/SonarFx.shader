//
// Sonar FX
//
// Copyright (C) 2013, 2014 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
Shader "Hidden/SonarFX"
{
    Properties
    {
        _SonarBaseColor  ("Base Color",  Color)  = (0.1, 0.1, 0.1, 0)
        _SonarWaveColor  ("Wave Color",  Color)  = (1.0, 0.1, 0.1, 0)
        _SonarWaveParams ("Wave Params", Vector) = (1, 20, 2, 10)
        _SonarWaveVector ("Wave Vector", Vector) = (0, 0, 1, 0)
        _SonarAddColor   ("Add Color",   Color)  = (0, 0, 0, 0)
        _SonarGhostColor ("Ghost Color", Color) = (0.2, 0.4, 1.0, 0.25)
        _SonarPulseWidth ("Pulse Width", Float) = 2.0
        _SonarGhostDuration ("Ghost Duration", Float) = 1.0
        _SonarPulseCount ("Pulse Count", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        CGPROGRAM

        #pragma surface surf Lambert
        #pragma multi_compile SONAR_DIRECTIONAL SONAR_SPHERICAL

        struct Input
        {
            float3 worldPos;
        };

        float3 _SonarBaseColor;
        float3 _SonarWaveColor;
        float4 _SonarWaveParams; // Amp, Exp, Interval, Speed
        float3 _SonarWaveVector;
        float3 _SonarAddColor;
        float4 _SonarGhostColor;
        float _SonarPulseWidth;
        float _SonarGhostDuration;
        float _SonarPulseCount;
        float _SonarPulseTimes[8];
        float4 _SonarPulseOrigins[8];

        void surf(Input IN, inout SurfaceOutput o)
        {
            float maxPulse = 0.0;
            float maxGhost = 0.0;
            int count = (int)_SonarPulseCount;
            if (count > 8) count = 8;

            float3 pulseColor = _SonarWaveColor;
            if (length(pulseColor) < 0.01) pulseColor = float3(0.1, 0.1, 0.1);

            float3 ghostColor = _SonarGhostColor.rgb;
            if (length(ghostColor) < 0.01) ghostColor = float3(0.1, 0.1, 0.1);

            for (int i = 0; i < count; i++)
            {
                float t = _Time.y - _SonarPulseTimes[i];
                if (t < 0.0 || t > _SonarWaveParams.z + _SonarGhostDuration) continue;

                float w;
#ifdef SONAR_DIRECTIONAL
                w = dot(IN.worldPos, _SonarWaveVector);
#else
                w = length(IN.worldPos - _SonarPulseOrigins[i].xyz);
#endif

                float activeTime = min(t, _SonarWaveParams.z);
                float waveFront = activeTime * _SonarWaveParams.w;
                float dist = abs(w - waveFront);

                if (t <= _SonarWaveParams.z)
                {
                    float pulseValue = exp(-dist * dist / (_SonarPulseWidth * _SonarPulseWidth));
                    pulseValue *= _SonarWaveParams.x;
                    pulseValue *= saturate(1.0 - (t / _SonarWaveParams.z));
                    maxPulse = max(maxPulse, pulseValue);
                }

                float behind = waveFront - w;
                if (behind > 0.0)
                {
                    float trailWidth = max(_SonarPulseWidth * 4.0, 6.0);
                    float ghostFade = t <= _SonarWaveParams.z
                        ? saturate(1.0 - (t / _SonarWaveParams.z))
                        : saturate(1.0 - ((t - _SonarWaveParams.z) / max(_SonarGhostDuration, 0.0001)));
                    float ghostValue = saturate(1.0 - (behind / trailWidth)) * ghostFade * _SonarGhostColor.a;
                    maxGhost = max(maxGhost, ghostValue);
                }
            }

            o.Albedo = _SonarBaseColor;
            o.Emission = pulseColor * maxPulse + ghostColor * maxGhost + _SonarAddColor;
        }

        ENDCG
    } 
    Fallback "Diffuse"
}
