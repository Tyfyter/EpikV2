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
/*float2 uMin;
float2 uMax;*/

float4 Starlight(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(uImage0, coords);
	float b = ((color.r+color.g+color.b)/2);
	b = pow(b, 1.5);
	color.rgb = float3(b*0.64, b*0.7, b);
	color.a *= b;
	return color;
}

technique Technique1{
	pass Starlight{
		PixelShader = compile ps_2_0 Starlight();
	}
}