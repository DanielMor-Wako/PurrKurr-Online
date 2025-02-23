Shader "Wakoz/RippleDistortion"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {} // Main texture
        _RippleSpeed("Ripple Speed", Float) = 1.0 // Speed of the ripple effect
        _RippleFrequency("Ripple Frequency", Float) = 10.0 // Frequency of the ripples
        _RippleIntensity("Ripple Intensity", Float) = 0.05 // Intensity of the distortion
        _RippleCenter("Ripple Center", Vector) = (0.5, 0.5, 0, 0) // Center of the ripple (UV space)
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

            float _RippleSpeed;
            float _RippleFrequency;
            float _RippleIntensity;
            float4 _RippleCenter;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Calculate the distance from the ripple center
                float2 rippleCenter = _RippleCenter.xy;
                float2 uvOffset = i.uv - rippleCenter;
                float distance = length(uvOffset);

                // Create the ripple effect using sine waves
                float ripple = sin(distance * _RippleFrequency - _Time.y * _RippleSpeed);

                // Apply distortion based on the ripple effect
                float distortion = ripple * _RippleIntensity;
                float2 distortedUV = i.uv + normalize(uvOffset) * distortion;

                // Sample the texture with the distorted UVs
                fixed4 texColor = tex2D(_MainTex, distortedUV);

                return texColor;
            }
            ENDCG
        }
    }
}
