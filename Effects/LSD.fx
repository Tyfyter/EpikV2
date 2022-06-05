sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;

float GetFiniteOffset(float factor, float mult) {
	float realFactor = (factor % 3.14159265359) - 1.57079632679;
	const float exponent = 8;
	if (realFactor > 0) {
		return pow(realFactor, exponent) * mult;
	} else {
		return pow(-realFactor, exponent) * -mult;
	}
}

float4 LSD(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 underlying = tex2D(uImage0, coords);
	float2 pixelFactor = float2(4, 4) / uScreenResolution;
	float rx = coords.x + sin(uTime) * pixelFactor.x;
	float gx = coords.x + sin(uTime * 0.9) * pixelFactor.x;
	float bx = coords.x + sin(uTime * 1.1) * pixelFactor.x;
	float ry = (coords.y + GetFiniteOffset(uTime, 1) / 38) % 1;
	float gy = (coords.y + GetFiniteOffset(uTime * 0.9, 1) / 38) % 1; // + GetFiniteOffset(uTime * 0.9, 1) / 80; // + GetFiniteOffset(uTime * 0.9, pixelFactor.y, 4);
	float by = (coords.y + GetFiniteOffset(uTime * 1.1, 1) / 38) % 1; // + GetFiniteOffset(uTime * 1.1, 1) / 80; // + GetFiniteOffset(uTime * 1.1, pixelFactor.y, 4);
	float4 overlying = (tex2D(uImage0, float2(rx, ry)) * float4(1, 0, 0, 1)) +
	(tex2D(uImage0, float2(gx, gy)) * float4(0, 1, 0, 1)) +
	(tex2D(uImage0, float2(bx, by)) * float4(0, 0, 1, 1));
	if (overlying.a > 1) {
		overlying.a = 1;
	}
	return (1 - overlying.a) * underlying + overlying.a * overlying;
}
float4 LessD (float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0{
	float4 underlying = tex2D(uImage0, coords);
	float2 pixelFactor = float2(4, 4) / uScreenResolution;
	float rx = coords.x + sin(uTime) * pixelFactor.x;
	float gx = coords.x + sin(uTime * 0.9) * pixelFactor.x;
	float bx = coords.x + sin(uTime * 1.1) * pixelFactor.x;
	float4 overlying = (tex2D(uImage0, float2(rx, coords.y)) * float4(1, 0, 0, 1)) +
	(tex2D(uImage0, float2(gx, coords.y)) * float4(0, 1, 0, 1)) +
	(tex2D(uImage0, float2(bx, coords.y)) * float4(0, 0, 1, 1));
	if (overlying.a > 1) {
		overlying.a = 1;
	}
	return (1 - overlying.a) * underlying + overlying.a * overlying;
}

technique Technique1 {
	pass LSD {
		PixelShader = compile ps_2_0 LSD();
	}
	pass LessD {
		PixelShader = compile ps_2_0 LessD();
	}
}