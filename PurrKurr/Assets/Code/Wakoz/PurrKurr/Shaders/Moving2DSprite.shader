Shader "Wakoz/2DMovingSprite"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {} // Main texture
        _TintColor("Tint Color", Color) = (1.0, 1.0, 1.0, 1.0) // Tint color for the sprites
        _SpriteCount("Number of Sprites", Range(1, 100)) = 10 // Number of sprites
        _SpriteSize("Sprite Size", Float) = 1.0 // Size of the sprites
        _Speed("Movement Speed", Float) = 1.0 // Speed of movement
        _Direction("Movement Direction", Vector) = (1, 0, 0, 0) // Direction of movement
        _Distortion("Distortion Strength", Float) = 0.1 // Distortion offset strength
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

            float4 _TintColor; // Tint color property
            float _SpriteCount;
            float _SpriteSize;
            float _Speed;
            float4 _Direction;
            float _Distortion;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Randomize sprite positions based on UV coordinates
                float2 spritePosition = frac(i.uv * _SpriteCount);

                // Apply movement direction and speed
                float2 movement = _Direction.xy * _Speed * _Time.y;

                // Add distortion to movement
                float2 distortion = float2(
                    sin(i.uv.y * 10.0 + _Time.y * _Speed) * _Distortion,
                    cos(i.uv.x * 10.0 + _Time.y * _Speed) * _Distortion
                );

                // Combine movement and distortion
                spritePosition += movement + distortion;

                // Wrap around to keep sprites within bounds
                spritePosition = frac(spritePosition);

                // Sample the texture
                fixed4 texColor = tex2D(_MainTex, spritePosition);

                // Scale the sprite size
                float distanceToCenter = length(spritePosition - 0.5);
                float sizeMask = smoothstep(0.5, 0.5 - _SpriteSize, distanceToCenter);

                // Apply size mask to the sprite
                texColor.a *= sizeMask;

                // Apply tint color
                texColor *= _TintColor;

                return texColor;
            }
            ENDCG
        }
    }
}
