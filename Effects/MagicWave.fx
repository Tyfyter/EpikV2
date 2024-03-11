sampler uImage0 : register(s0);
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
float4 uShaderSpecificData;
float2 uTargetPosition;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;
/*float2 uMin;
float2 uMax;*/

float4 MagicWave(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	coords.x += (sin((coords.y * uImageSize0.y) + (uTime * 4) + (coords.x * uImageSize0.x)) / uImageSize0.x) * 0.5f;
	return tex2D(uImage0, coords) * sampleColor;
}

float4 MagicWave2(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float2 pixel = float2(1, 1) / uImageSize0;
	float4 color = (tex2D(uImage0, coords) + tex2D(uImage0, coords + pixel * float2(1, 0)) + tex2D(uImage0, coords + pixel * float2(-1, 0)) + tex2D(uImage0, coords + pixel * float2(0, 1)) + tex2D(uImage0, coords + pixel * float2(0, -1))) / 5;
	coords += sin(color.b * 32 + uTime * 4) * 0.5f * (color.rg / uImageSize0);
	return (tex2D(uImage0, coords).a + color.a) * 0.5f * sampleColor;
}

technique Technique1{
	pass MagicWave {
		PixelShader = compile ps_2_0 MagicWave();
	}
	pass MagicWave2 {
		PixelShader = compile ps_2_0 MagicWave2();
	}
}