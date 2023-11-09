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

float4 BloodstainedHairDye(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 baseColor = tex2D(uImage0, coords);
	float noise = tex2D(uImage1, (coords * uImageSize0 * 2) / uImageSize1).r;
	if (noise > uOpacity) {
		noise = 0;
	} else if (noise + 0.02 > uOpacity) {
		noise = (noise - uOpacity) / 0.02;
	} else {
		noise = 1;
	}
	noise *= baseColor.a;
	return (baseColor * (1 - noise) + float4(noise * ((baseColor.r + baseColor.g + baseColor.b + 1) / 4), 0, 0, noise)) * sampleColor;
}

technique Technique1 {
	pass BloodstainedHairDye {
		PixelShader = compile ps_3_0 BloodstainedHairDye();
	}
}