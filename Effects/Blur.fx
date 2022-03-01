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
float2 uCenter;
float uProgress;
float uFrameCount;
float uFrame;

float gauss[3][3] = {
	0.070, 0.116, 0.070,
    0.116, 0, 0.116,
    0.070, 0.116, 0.070
};

float4 Gaussian(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 color = tex2D(uImage0, coords);
	float dx = 2 / uSourceRect.x;
	float dy = 2 / uSourceRect.y;
	for (int i = -1; i <= 1; i++) {
		for (int j = -1; j <= 1; j++) {
			color += gauss[i + 1][j + 1] * tex2D(uImage0, float2(coords.x + dx * i, coords.y + dy * j));
		}
	}
	return color;
}

technique Technique1 {
	pass Gaussian {
		PixelShader = compile ps_2_0 Gaussian();
	}
}