Shader "Wakoz/RotatingShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _RotationSpeed ("Rotation Speed", Float) = 1.0
        _MinRange ("Minimum Range", Float) = 0.8
        _MaxRange ("Maximum Range", Float) = 1.2
        _LerpSpeed ("Lerp Speed", Float) = 1.0
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        LOD 100

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

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
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _RotationSpeed;
            float _MinRange;
            float _MaxRange;
            float _LerpSpeed;
            float4 _Color;
            float4 _RendererColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color * _RendererColor;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate rotation angle based on time and speed
                float angle = _Time.y * _RotationSpeed;
                
                // Create rotation matrix
                float2x2 rotationMatrix = float2x2(
                    cos(angle), -sin(angle),
                    sin(angle), cos(angle)
                );
                
                // Center UVs around (0.5, 0.5) for rotation
                float2 centeredUV = i.uv - 0.5;
                
                // Apply rotation
                float2 rotatedUV = mul(rotationMatrix, centeredUV);
                
                // Scale UVs based on lerped range
                float t = (sin(_Time.y * _LerpSpeed) + 1.0) * 0.5; // Oscillate between 0 and 1
                float scale = lerp(_MinRange, _MaxRange, t);
                rotatedUV *= scale; // Multiply to scale UVs (larger scale zooms out)
                
                // Move UVs back to original position
                rotatedUV += 0.5;
                
                // Clamp UVs to prevent sampling outside texture
                rotatedUV = clamp(rotatedUV, 0.0, 1.0);
                
                // Sample texture
                fixed4 col = tex2D(_MainTex, rotatedUV) * i.color;
                col.rgb *= col.a; // Premultiply alpha for correct blending
                return col;
            }
            ENDCG
        }
    }
}
