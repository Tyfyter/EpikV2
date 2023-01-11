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


float4 DST_LSD(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float2 normalCoordsR = (coords * uScreenResolution + uScreenPosition) / float2(512 * 2, 512 * 2);
	float4 baseColorR = tex2D(uImage1, fmod(normalCoordsR, float2(1, 1)));
	float pixelR = 2 / float(512);
	
	float4 X0R = tex2D(uImage1, fmod(normalCoordsR - float2(pixelR, 0), float2(1, 1)));
	float4 X1R = tex2D(uImage1, fmod(normalCoordsR + float2(pixelR, 0), float2(1, 1)));
	
	float4 Y0R = tex2D(uImage1, fmod(normalCoordsR - float2(0, pixelR), float2(1, 1)));
	float4 Y1R = tex2D(uImage1, fmod(normalCoordsR + float2(0, pixelR), float2(1, 1)));
	
	float diffXR = (X0R.r - baseColorR.r) - (X1R.r - baseColorR.r);
	float diffYR = (Y0R.r - baseColorR.r) - (Y1R.r - baseColorR.r);
	
	float2 coordsR = coords + float2(diffXR * 4 / uScreenResolution.x, diffYR * 4 / uScreenResolution.y);
	
	float2 normalCoordsG = (coords * uScreenResolution + uScreenPosition) / float2(512 * 4, 512 * 4);
	float4 baseColorG = tex2D(uImage1, fmod(normalCoordsG, float2(1, 1)));
	float pixelG = 2 / float(512);
	
	float4 X0G = tex2D(uImage1, fmod(normalCoordsG - float2(pixelG, 0), float2(1, 1)));
	float4 X1G = tex2D(uImage1, fmod(normalCoordsG + float2(pixelG, 0), float2(1, 1)));
	
	float4 Y0G = tex2D(uImage1, fmod(normalCoordsG - float2(0, pixelG), float2(1, 1)));
	float4 Y1G = tex2D(uImage1, fmod(normalCoordsG + float2(0, pixelG), float2(1, 1)));
	
	float diffXG = (X0G.r - baseColorG.r) - (X1G.r - baseColorG.r);
	float diffYG = (Y0G.r - baseColorG.r) - (Y1G.r - baseColorG.r);
	float2 coordsG = coords + float2(diffXG * 4 / uScreenResolution.x, diffYG * 4 / uScreenResolution.y);
	
	float2 normalCoordsB = (coords * uScreenResolution + uScreenPosition) / float2(512, 512);
	float4 baseColorB = tex2D(uImage1, fmod(normalCoordsB, float2(1, 1)));
	float pixelB = 2 / float(512);
	
	float4 X0B = tex2D(uImage1, fmod(normalCoordsB - float2(pixelB, 0), float2(1, 1)));
	float4 X1B = tex2D(uImage1, fmod(normalCoordsB + float2(pixelB, 0), float2(1, 1)));
	
	float4 Y0B = tex2D(uImage1, fmod(normalCoordsB - float2(0, pixelB), float2(1, 1)));
	float4 Y1B = tex2D(uImage1, fmod(normalCoordsB + float2(0, pixelB), float2(1, 1)));
	
	float diffXB = (X0B.r - baseColorB.r) - (X1B.r - baseColorB.r);
	float diffYB = (Y0B.r - baseColorB.r) - (Y1B.r - baseColorB.r);
	float2 coordsB = coords + float2(diffXB * 4 / uScreenResolution.x, diffYB * 4 / uScreenResolution.y);
	//float4 offsetB = tex2D(uImage1, fmod((coords * uScreenResolution + uScreenPosition) / float2(512, 512), float2(1, 1)));
	return float4(tex2D(uImage0, coordsR).r, tex2D(uImage0, coordsG).g, tex2D(uImage0, coordsB).b, 1);
}
/*float4 LessD (float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0{
	float4 underlying = tex2D(uImage0, coords);
	float2 pixelFactor = float2(4, 4) / uScreenResolution;
	float rx = coords.x + sin(uTime) * pixelFactor.x;
	float gx = coords.x + sin(uTime * 0.9) * pixelFactor.x;
	float bx = coords.x + sin(uTime * 1.1) * pixelFactor.x;
	float4 overlying = (tex2D(uImage0, float2(rx, coords.y)) * float4(1, 0, 0, 1)) +
	(tex2D(uImage0, float2(gx, coords.y)) * float4(0, 1, 0, 1)) +
	(tex2D(uImage0, float2(bx, coords.y)) * float4(0, 0, 1, 1));
	if (overlying.a > 1) {
		overlying.a = 1;
	}
	return (1 - overlying.a) * underlying + overlying.a * overlying;
}*/

technique Technique1 {
	pass DST_LSD {
		PixelShader = compile ps_3_0 DST_LSD();
	}
}