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
float2 uTargetPosition;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;
float2 pointA;
float2 pointB;
float2 pointC;
/*float2 uMin;
float2 uMax;*/

float4 HydraNeck(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float pi = 3.14159265359;
	float2 absoluteCoords = (uImageSize0 * coords * 2 / uScale) + uWorldPosition + uOffset;
	float4 starColor = tex2D(uImage1, (absoluteCoords % uImageSize1) / uImageSize1);
	float star = starColor.b;
	float2 off = float2(starColor.g * cos(starColor.r * pi), starColor.g * sin(starColor.r * pi)) * 3;
	float2 c = (off / uImageSize0);
	float2 pixelPos = float2(coords.x * uImageSize0.x, coords.y * uImageSize0.y) + uWorldPosition;
	float4 color = float4(sampleColor.rgb, 0);
	for (float f = 0; f < 1; f += 0.018867925) {//53 nodes, because I can't get away with any more
		float2 currentPos = (pointC * f * f * f + pointB * (1 - f * f * f)) * f + pointA * (1 - f) + c;
		float distFactor = length(currentPos - pixelPos) / 3;
		if (distFactor < 1) {
			color.a = 1;
		} else {
			color.a = max(color.a, 1 - pow(distFactor - 1, 2));
		}
	}
	//color2.rgb += float3(star * 0.64, star * 0.7, star);
	color.rgb += float3(star * 0.64, star * 0.7, star);
	color.rgb = color.rgb;
	color.rgb *= color.a;
	return color;
}

technique Technique1{
	pass HydraNeck {
		PixelShader = compile ps_3_0 HydraNeck();
	}
}