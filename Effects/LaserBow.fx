﻿sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float2 uOffset;
float uScale;
float2 uTargetPosition;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;
/*float2 uMin;
float2 uMax;*/

float4 LaserBow(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 color = tex2D(uImage0, coords);
	float b = ((color.r+color.g+color.b)/3);
	float value = b - uSaturation;
	if (value <= 0) {
		value += 1;
	}
	value *= color.a;
	return float4(1, 0, 0.369, 1) * value;
}

technique Technique1 {
	pass LaserBow {
		PixelShader = compile ps_2_0 LaserBow();
	}
}