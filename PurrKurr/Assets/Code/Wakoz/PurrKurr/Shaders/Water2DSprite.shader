Shader "Wakoz/2DWaterSprite"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _WaterTopColor("Water Top Color", Color) = (0.0, 0.5, 1.0, 1.0)
        _WaterColor("Water Color", Color) = (0.0, 0.3, 0.8, 1.0)
        _WaterLevel("Water Level", Range(0, 1)) = 0.5
        _WaterTopWidth("Water Top Width", Float) = 0.1

        _WaveSpeed("Wave Speed", Float) = 1.0
        _WaveFrequency("Wave Frequency", Float) = 10.0
        _WaveDepth("Wave Depth", Float) = 0.05

        _SurfaceWaveSpeed("Surface Wave Speed", Float) = 2.0 // Speed of the surface wave
        _SurfaceWaveFrequency("Surface Wave Frequency", Float) = 15.0 // Frequency of the surface wave
        _SurfaceWaveFrequencyMultiplier("Surface Wave Frequency Multiplier", Float) = 1.0 // Frequency Multiplier of the surface wave
        _SurfaceWaveAmplitude("Surface Wave Amplitude", Float) = 0.1 // Amplitude of the surface wave

        _ReflectionSpeed("Reflection Speed", Float) = 0.5
        _ReflectionNoise("Reflection Noise", Float) = 0.1
        _ReflectionStrength("Reflection Strength", Float) = 0.5
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _WaterTopColor;
            float4 _WaterColor;
            float _WaterLevel;
            float _WaterTopWidth;

            float _WaveSpeed;
            float _WaveFrequency;
            float _WaveDepth;

            float _SurfaceWaveSpeed;
            float _SurfaceWaveFrequency;
            float _SurfaceWaveFrequencyMultiplier;
            float _SurfaceWaveAmplitude;

            float _ReflectionSpeed;
            float _ReflectionNoise;
            float _ReflectionStrength;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Calculate surface wave distortion
                float surfaceWave = sin(v.vertex.x * _SurfaceWaveFrequency * _SurfaceWaveFrequencyMultiplier + _Time.y * _SurfaceWaveSpeed) * _SurfaceWaveAmplitude;

                // Apply the surface wave distortion to the vertex position
                o.vertex.y += surfaceWave;

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Calculate wave distortion for color manipulation
                float wave = sin(i.uv.x * _WaveFrequency + _Time.y * _WaveSpeed) * _WaveDepth;

                // Determine water level with wave distortion
                float waterLevel = _WaterLevel + wave;

                // Top gradient effect
                float gradient = smoothstep(waterLevel - _WaterTopWidth, waterLevel, i.uv.y);

                // Base water color
                fixed4 waterColor = lerp(_WaterColor, _WaterTopColor, gradient);

                // Reflection effect
                float reflection = sin(i.uv.x * _WaveFrequency * 0.5 + _Time.y * _ReflectionSpeed) * _ReflectionNoise;
                reflection = reflection * _ReflectionStrength * (1.0 - gradient);

                // Combine reflection with water color
                waterColor.rgb += reflection;

                // Sample the main texture
                fixed4 texColor = tex2D(_MainTex, i.uv);

                // Combine texture with water effect
                fixed4 finalColor = texColor * waterColor;

                return finalColor;
            }
            ENDCG
        }
    }
}