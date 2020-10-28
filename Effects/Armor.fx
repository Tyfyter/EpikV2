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
/*float2 uMin;
float2 uMax;*/

float4 JadeConst(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0{
	float4 color = tex2D(uImage0, coords);
	float brightness = (color.r+color.g+color.b)/3;
	brightness = pow(brightness,1.5);
	return float4(0,brightness,brightness/2,color.a*sampleColor.a);
}

technique Technique1{
	pass JadeConst{
		PixelShader = compile ps_2_0 JadeConst();
	}
}