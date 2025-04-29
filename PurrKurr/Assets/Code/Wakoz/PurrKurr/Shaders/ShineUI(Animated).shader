// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Wakoz/ShineUI(Animated)"
{
Properties
{
_ShineWidth("ShineWidth", Range(0, 1)) = 0.01
_LoopDuration("Loop Duration", Range(0.1, 10)) = 2

_Val1("Val1", Float) = 1
_Val2("Val2", Float) = 1
_Val3("Val3", Float) = 1

[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
_Color("Tint", Color) = (1,1,1,1)

_StencilComp("Stencil Comparison", Float) = 8
_Stencil("Stencil ID", Float) = 0
_StencilOp("Stencil Operation", Float) = 0
_StencilWriteMask("Stencil Write Mask", Float) = 255
_StencilReadMask("Stencil Read Mask", Float) = 255

_ColorMask("Color Mask", Float) = 15

[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
_SoftMask("Mask", 2D) = "white" {} // Soft Mask
}

SubShader
{
Tags
{
"Queue" = "Transparent"
"IgnoreProjector" = "True"
"RenderType" = "Transparent"
"PreviewType" = "Plane"
"CanUseSpriteAtlas" = "True"
}

Stencil
{
Ref[_Stencil]
Comp[_StencilComp]
Pass[_StencilOp]
ReadMask[_StencilReadMask]
WriteMask[_StencilWriteMask]
}

Cull Off
Lighting Off
ZWrite Off
ZTest[unity_GUIZTestMode]
Blend SrcAlpha OneMinusSrcAlpha
ColorMask[_ColorMask]

Pass
{
Name "Default"
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 2.0

#pragma multi_compile _ SOFTMASK_SIMPLE SOFTMASK_SLICED SOFTMASK_TILED // Soft Mask

#include "UnityCG.cginc"
#include "UnityUI.cginc"
#include "Assets/Dependencies/SoftMask/Shaders/Resources/SoftMask.cginc" // Soft Mask

#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
#pragma multi_compile_local _ UNITY_UI_ALPHACLIP

struct appdata_t
{
float4 vertex   : POSITION;
float4 color    : COLOR;
float2 texcoord : TEXCOORD0;
UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
float4 vertex   : SV_POSITION;
fixed4 color    : COLOR;
float2 texcoord : TEXCOORD0;
float4 worldPosition : TEXCOORD1;
float4 mask     : TEXCOORD2;
UNITY_VERTEX_OUTPUT_STEREO
SOFTMASK_COORDS(3) // Soft Mask
};

sampler2D _MainTex;
fixed4 _Color;
fixed4 _TextureSampleAdd;
float4 _ClipRect;
float4 _MainTex_ST;
float _UIMaskSoftnessX;
float _UIMaskSoftnessY;
float _ShineWidth;
float _LoopDuration;
float _Val1;
float _Val2;
float _Val3;

v2f vert(appdata_t v)
{
v2f OUT;
UNITY_SETUP_INSTANCE_ID(v);
UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
float4 vPosition = UnityObjectToClipPos(v.vertex);
OUT.worldPosition = v.vertex;
OUT.vertex = vPosition;

float2 pixelSize = vPosition.w;
pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
float2 maskUV = (v.vertex.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);
OUT.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
OUT.mask = float4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));

OUT.color = v.color * _Color;
#ifdef __SOFTMASK_ENABLE
OUT.maskPosition = float4(0, 0, 0, 0); // Soft Mask
SOFTMASK_CALCULATE_COORDS(OUT, v.vertex); // Soft Mask
#endif
return OUT;
}

fixed4 frag(v2f IN) : SV_Target
{
// Round up the alpha color coming from the interpolator (to 1.0/256.0 steps)
const half alphaPrecision = half(0xff);
const half invAlphaPrecision = half(1.0 / alphaPrecision);
IN.color.a = round(IN.color.a * alphaPrecision) * invAlphaPrecision;

half4 color = IN.color * (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd);

#ifdef UNITY_UI_CLIP_RECT
half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
color.a *= m.x * m.y;
#endif

#ifdef UNITY_UI_ALPHACLIP
clip(color.a - 0.001);
#endif

#ifdef __SOFTMASK_ENABLE
color *= SOFTMASK_GET_MASK(IN); // Soft Mask
#endif

// Calculate shine projection
fixed currentDistanceProjection = (((IN.texcoord.x * _Val1) + (IN.texcoord.y * _Val2)) / _Val3);

// Animate shine location to loop from -1 to 2 over _LoopDuration
float shineLocation = lerp(-1, 2, frac(_Time.y / _LoopDuration));

fixed lowLevel = shineLocation - _ShineWidth;
fixed highLevel = shineLocation + _ShineWidth;
fixed shinePower = smoothstep(_ShineWidth, 0, abs(abs(clamp(currentDistanceProjection, lowLevel, highLevel) - shineLocation)));

color.rgb += color.a * (shinePower / 2);
return color;
}
ENDCG
}
}
}