sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;

float4 MappedShade(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0{
	float4 map = tex2D(uImage3, coords);
	float2 c = coords;
	float v = map.r*4;
	c.x += cos(uTime*-2)*v/uScreenResolution.x;
	c.y += sin(uTime)*v/uScreenResolution.y;
	
	float vx = (map.g*32)-16;
	float vy = (map.b*32)-16;
	c.x += vx/uScreenResolution.x;
	c.y += vy/uScreenResolution.y;
	
	float4 color = tex2D(uImage0, c);
	return color;
}

technique Technique1{
	pass MappedShade{
		PixelShader = compile ps_2_0 MappedShade();
	}
}