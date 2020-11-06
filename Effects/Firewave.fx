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

float4 Firewave(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0{
	//float4 offset = tex2D(uImage1, (((coords.x+uTime)/30)%1,coords.y));
	//float2 coord = coords+float2(0, (offset.r/0.65)*4-2/uImageSize0.y);
	coords.y+=sin((coords.x*uImageSize0.x)+(uTime*4)+(coords.y*uImageSize0.y))/uImageSize0.y;
	return tex2D(uImage0, coords);
}

technique Technique1{
	pass Firewave{
		PixelShader = compile ps_2_0 Firewave();
	}
}