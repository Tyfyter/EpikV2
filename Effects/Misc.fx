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
float2 uLoopData;

float4 Identity(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0 {
	return color * tex2D(uImage0, uv);
}

technique Technique1 {
	pass Identity {
		PixelShader = compile ps_2_0 Identity();
	}
}
