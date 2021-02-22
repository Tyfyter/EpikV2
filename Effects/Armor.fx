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

float4 JadeConst(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 color = tex2D(uImage0, coords);
	float brightness = (color.r+color.g+color.b)/3;
	brightness = pow(brightness,1.5);
	return float4(0,brightness,brightness/2,color.a*sampleColor.a);
}

float4 Starlight(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 color = tex2D(uImage0, coords);
	float b = ((color.r+color.g+color.b)/2);
	b = pow(b, 1.5);
	color.rgb = float3(b*0.64, b*0.7, b);
	return color;
}

float4 BrightStarlight(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 color = tex2D(uImage0, coords);
	float b = ((color.r+color.g+color.b)/2);
	color.rgb = float3(0.64*color.a, 0.7*color.a, 1*color.a);
	color.a*=pow(b, 1.5);
	return color;
}

technique Technique1 {
	pass JadeConst {
		PixelShader = compile ps_2_0 JadeConst();
	}
	pass Starlight {
		PixelShader = compile ps_2_0 Starlight();
	}
	pass BrightStarlight {
		PixelShader = compile ps_2_0 BrightStarlight();
	}
}