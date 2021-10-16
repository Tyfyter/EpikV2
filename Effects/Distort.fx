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

float4 Distort(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0{
	//return float4(uColor.r, uColor.g, uColor.b, 0.5);
	const float borderPoint = 0.3;
	const float borderThickness = 0.1;
	float2 diff = coords - uOffset;
	//diff = normalize(diff) / uImageSize0;
	float4 distortion = tex2D(uImage1, coords);
	float r = max(distortion.r - uColor.r, 0);
	float g = max(distortion.g - uColor.g, 0);
	float b = max(distortion.b - uColor.b, 0);
	float sum = (r + g + b)*1;
	if (sum > borderPoint && sum < borderPoint+borderThickness) {
		return float4(0,0,0,1);
	}
	float4 color = tex2D(uImage0, coords - (diff * sum));
	//color.rgb *= uColor;
	return color;
}

technique Technique1{
	pass Distort {
		PixelShader = compile ps_2_0 Distort();
	}
}