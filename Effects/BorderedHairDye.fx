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
float4x4 zoom;
/*float2 uMin;
float2 uMax;*/
bool EmptyAdj(float2 coords, float2 unit) {
	return tex2D(uImage0, coords - unit * float2(1, 0)).a == 0 
	|| tex2D(uImage0, coords - unit * float2(0, 1)).a == 0
	|| tex2D(uImage0, coords + unit * float2(1, 0)).a == 0
	|| tex2D(uImage0, coords + unit * float2(0, 1)).a == 0;
}

float4 DashingDye(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 baseColor = tex2D(uImage0, coords);
	float4 color = float4(0.149, 0.616, 0.808, 1);

	if (baseColor.r > 0.36 || !EmptyAdj(coords, float2(2, 2) / uImageSize0)) {
		float adjX = (1 - ((coords.x - 0.05) / 0.7)) * 7;
		if (adjX < 1) {
			color = float4(0.859, 0.176, 0.263, 1);
		} else if (adjX < 2) {
			color = float4(0.906, 0.408, 0.22, 1);
		} else if (adjX < 3) {
			color = float4(0.984, 0.976, 0.686, 1);
		} else if (adjX < 4) {
			color = float4(0.404, 0.792, 0.314, 1);
		} else if (adjX >= 5) {
			color = float4(0.365, 0.125, 0.502, 1);
		}
	}
	
	baseColor.r = pow(baseColor.r, 0.75);
	baseColor.g = pow(baseColor.g, 0.75);
	baseColor.b = pow(baseColor.b, 0.75);
	baseColor *= sampleColor * 1.5;
	if (baseColor.r > 1) baseColor.r = 1;
	if (baseColor.g > 1) baseColor.g = 1;
	if (baseColor.b > 1) baseColor.b = 1;
	return color * baseColor;
}

float4 LunarDye(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float2 baseCoords = float2(0.5, (uSourceRect.y + 16) / uImageSize0.y);
	float2 offsetCoords = (coords - baseCoords) * uImageSize0;
	float len = length(offsetCoords) / 32;
	float angle = atan2(offsetCoords.y, offsetCoords.x);
	float cosine = cos(angle);
	float sine = sin(angle);
	//offsetCoords += float2(offsetCoords.x * cosine - offsetCoords.y * sine, offsetCoords.x * sine + offsetCoords.y * cosine) * len * 0.5 * sin(uTime + len);
	//offsetCoords += float2(sin(uTime + offsetCoords.x * 0.25) * max(abs(offsetCoords.x) - 8, 0) * 0.25, cos(-uTime + offsetCoords.y * 0.25) * max(abs(offsetCoords.y) - 8, 0) * 0.25);
	
	coords = (offsetCoords / uImageSize0) + baseCoords;
	float4 baseColor = tex2D(uImage0, coords);
	float4 color = float4(0.557 * 0.5, 0.541 * 0.5, 0.769 * 0.5, 0.769) * 0.5;

	if (baseColor.r > 0.36 || !EmptyAdj(coords, float2(2, 2) / uImageSize0)) {
		color = float4(0.141, 0.286, 0.745, 1);
	}
	
	baseColor.r = pow(baseColor.r, 0.5);
	baseColor.g = pow(baseColor.g, 0.5);
	baseColor.b = pow(baseColor.b, 0.5);
	baseColor = ((baseColor * 1.5) + (baseColor * sampleColor * 1.5)) * 0.5;
	if (baseColor.r > 1) baseColor.r = 1;
	if (baseColor.g > 1) baseColor.g = 1;
	if (baseColor.b > 1) baseColor.b = 1;
	
	float frameY = (coords.y * uImageSize0.y - uSourceRect.y);
	float frameX = 0;
	if (uDirection < 0) {
		frameX = uImageSize0.x * (1 - coords.x);
	} else {
		frameX = uImageSize0.x * coords.x;
	}
	float2 absoluteCoords = (float2(frameX, frameY)) + float2(uTime, -uTime) * 3 + uTargetPosition * 0.05;
	float4 starColor = tex2D(uImage1, fmod((absoluteCoords % uImageSize1) / uImageSize1, float2(1, 1)));
	float star = (pow(max(starColor.b, 0), 4) - 0.1) * 4;
	if (star < 0) star = 0;
	if (star > 0.7) star = 0.7;
	//return float4(star * 0.64, star * 0.7, star, 1) * color.a * baseColor.a;
	
	//
	return color * baseColor + float4(star * 0.64, star * 0.7, star, 0) * baseColor.a; // * baseColor
}

float4 Base(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 baseColor = tex2D(uImage0, coords);
	float4 color = float4(0.149, 0.616, 0.808, 1);

	if (baseColor.r > 0.36 || !EmptyAdj(coords, float2(2, 2) / uImageSize0)) {
		
	}
	return color * baseColor;
}

technique Technique1 {
	pass DashingDye {
		PixelShader = compile ps_2_0 DashingDye();
	}
	pass LunarDye {
		PixelShader = compile ps_3_0 LunarDye();
	}
}