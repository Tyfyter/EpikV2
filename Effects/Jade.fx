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
/*float2 uMin;
float2 uMax;*/

/**Au demain, mon amie.
float4 Jade(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0{
	float4 color = tex2D(uImage0, coords);
	//color.a = 1;
	float brightness = (color.r+color.g+color.b)/3;
	//float frameY = (coords.y * uImageSize0.y - uSourceRect.y) / uSourceRect.w;
	//float framePos = 0;
	//if(uFrameCount>1)framePos = (uFrame/uFrameCount);
	//float posY = (coords.y-framePos)*uFrameCount;
	//float dist = sqrt(pow(abs(coords.x-uCenter.x),2)+pow(abs(posY-uCenter.y),2));//sqrt(2);//float2(coords.x,(coords.y/(uSourceRect.w/uImageSize0.y))%1)
	float dist = sqrt(pow(abs((coords.x*uImageSize0.x)-uCenter.x),2)+pow(abs((coords.y*uImageSize0.y)-uCenter.y),2))/sqrt(2);
	brightness = pow(brightness,1.5);
	float4 jadeColor = float4(0,brightness,brightness*0.5,color.a);
	//color.r = coords.x>uCenter.x;
	//color.g = frameY>uCenter.y;
	//if(dist<0.3)return float4(1,0,0,color.a);
	if(dist>uProgress)return color;
	return jadeColor;
}*/
float4 Jade(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0{
	float4 color = tex2D(uImage0, coords);
	float brightness = (color.r+color.g+color.b)/3;
	brightness = pow(brightness,1.5);
	float4 jadeColor = float4(0,brightness,brightness/2,color.a*sampleColor.a);
	//if(brightness>uProgress)return color;
	//return jadeColor;
	color.a = color.a*sampleColor.a;
	return lerp(color,jadeColor,clamp(uProgress/brightness, 0, 1));
}

technique Technique1{
	pass Jade{
		PixelShader = compile ps_2_0 Jade();
	}
}