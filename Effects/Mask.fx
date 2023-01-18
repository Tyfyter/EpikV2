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


float4 Mask(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float a = tex2D(uImage0, coords).a;
	return float4(tex2D(uImage1, coords).rgb * a, a);
}

float4 EmpressWings(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float a = tex2D(uImage0, coords).a;
	float2 colorCoords;
	if (coords.x > 0.5) {
		colorCoords = float2(1 - (coords.x - uSaturation), coords.y);
	} else {
		colorCoords = float2(coords.x + uSaturation, coords.y);
	}
	if (colorCoords.x > 1) {
		colorCoords.x -= 1;
	}
	if (colorCoords.x < 0) {
		colorCoords.x += 1;
	}
	return float4(tex2D(uImage1, colorCoords).rgb * a, a);
}

technique Technique1 {
	pass Mask {
		PixelShader = compile ps_2_0 Mask();
	}
	pass EmpressWings {
		PixelShader = compile ps_2_0 EmpressWings();
	}
}