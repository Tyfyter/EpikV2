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
float2 uWorldSize;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float2 uOffset;
float uScale;
float2 uTargetPosition;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;

float gauss[3][3] = {
	0.070, 0.116, 0.070,
    0.116, 0,     0.116,
    0.070, 0.116, 0.070
};

float DoFlip(float a, int b) {
	if (b == 1) {
		return a;
	}
	if (b == -1) {
		return 1 - a;
	}
	return 1;
}

float4 Glow(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 color = tex2D(uImage0, coords);
	if (color.a > 0) {
		return color;
	}
	float dx = 2 / uImageSize0.x;
	float dy = 2 / uImageSize0.y;
	float subX = fmod(coords.x, dx) / dx;
	float subY = fmod(coords.y, dy) / dy;
	for (int i = -1; i <= 1; i++) {
		for (int j = -1; j <= 1; j++) {
			float glowFactor = DoFlip(subX, i) * DoFlip(subY, j);
			float2 realCoords = float2(coords.x + dx * i, coords.y + dy * j);
			color.rbg += tex2D(uImage0, realCoords).rbg * glowFactor;
		}
	}
	return color;
}

technique Technique1 {
	pass Glow {
		PixelShader = compile ps_3_0 Glow();
	}
}