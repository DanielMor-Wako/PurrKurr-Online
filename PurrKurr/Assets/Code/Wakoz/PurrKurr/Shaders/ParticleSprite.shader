// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)


Shader "Sprites/ParticleSprite" {

    Properties {
        _MainTex ("Sprite Texture", 2D) = "white" {}
		[HDR]_Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
		[HideInInspector][PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		[HideInInspector][PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0

		[HideInInspector] _StencilComp("Stencil Comparison", Float) = 8
		[HideInInspector] _Stencil("Stencil ID", Float) = 0
		[HideInInspector] _StencilOp("Stencil Operation", Float) = 0
		[HideInInspector] _StencilWriteMask("Stencil Write Mask", Float) = 255
		[HideInInspector] _StencilReadMask("Stencil Read Mask", Float) = 255

		[HideInInspector] _ColorMask("Color Mask", Float) = 15
		[HideInInspector] _ClipRect("Clip Rect", Vector) = (-32767, -32767, 32767, 32767)
		_MaskCoord			("Mask Coordinates", vector) = (0, 0, 32767, 32767)
		_MaskSoftnessX		("Mask SoftnessX", float) = 0
		_MaskSoftnessY		("Mask SoftnessY", float) = 0
		_SoftMask("Mask", 2D) = "white" {} // Soft Mask
    }

    SubShader {

        Tags {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="False"
        }

		Stencil {
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}


        Cull Off
        Lighting Off
        ZWrite Off

		Pass {
			Blend SrcAlpha One

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile __ UNITY_UI_CLIP_RECT
			#pragma multi_compile __ UNITY_UI_ALPHACLIP

			#pragma multi_compile __ SOFTMASK_SIMPLE SOFTMASK_SLICED SOFTMASK_TILED
			// #define SOFTMASK_SIMPLE

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"
			#include "Assets/Dependencies/SoftMask/Shaders/Resources/SoftMask.cginc" // Soft Mask

			struct appdata {
				float4 vertex : POSITION;
				float4 color    : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv				: TEXCOORD0;
				fixed4 color			: COLOR;
				float4 vertex			: SV_POSITION;
				float4 worldpos			: TEXCOORD1;
				float4	mask			: TEXCOORD2;
				SOFTMASK_COORDS(3) // Soft Mask
			};

			sampler2D _MainTex;float4 _MainTex_ST;
			fixed4 _Color;
			fixed4 _RendererColor;
			float4 _ClipRect;
			float _Intensity;
			float _MaskSoftnessX;
			float _MaskSoftnessY;

			v2f vert (appdata v) {

				float2 uv = TRANSFORM_TEX(v.uv, _MainTex);
				fixed4 color = _Color * v.color * _RendererColor;
				float4 vertex = UnityObjectToClipPos(v.vertex);
				float4 worldPos = v.vertex;
				float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);


				v2f o = {
					uv,
					color,
					vertex,
					worldPos,
					half4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_MaskSoftnessX, _MaskSoftnessY) + vertex.xy)),
					#ifdef __SOFTMASK_ENABLE
						float4(0,0,0,0)
					#endif
				};
				//o.worldpos = v.vertex;
				//o.vertex = UnityObjectToClipPos(v.vertex);
				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				//o.color = _Color * v.color * _RendererColor;
				//o.maskPosition = float4(0,0,0,0);
				SOFTMASK_CALCULATE_COORDS(o, v.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target {
				fixed4 col = tex2D(_MainTex, i.uv) * i.color;
				col.r += _Intensity;
				col.g += _Intensity;
				col.b += _Intensity;
				#if UNITY_UI_CLIP_RECT
					col.a *= UnityGet2DClipping(i.worldpos.xy, _ClipRect);
				#endif

				fixed4 result_SoftMask = col;
  				result_SoftMask *= SOFTMASK_GET_MASK(i);
  				return result_SoftMask;
			}
			ENDCG
		}
    }

	Fallback "Transparent/VertexLit"
}
