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

float4 Jade(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0{
	float4 color = tex2D(uImage0, coords);
	float brightness = (color.x+color.y+color.z)/3;
	return float4(0,1*brightness,0.40*brightness,color.w);
}

technique Technique1{
	pass Jade{
		PixelShader = compile ps_2_0 Jade();
	}
}