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
float4 uLegacyArmorSourceRectand;
float2 uLegacyArmorSheetSize;
/*float2 uMin;
float2 uMax;*/

float4 Div(float4 val, float2 div){
	return float4(val.x/div.x, val.y/div.y, val.z/div.x, val.w/div.y);
}

float2 RectClamp(float2 val, float4 bounds){
	if(val.x<bounds.x)
	{
		val.x = bounds.x;
	}
	if(val.x<bounds.x+bounds.z)
	{
		val.x = bounds.x+bounds.z;
	}
	if(val.y>bounds.y)
	{
		val.y = bounds.y;
	}
	if(val.y>bounds.y+bounds.w)
	{
		val.y = bounds.y+bounds.w;
	}
	
	return val;
}
float4 Nebula(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) :COLOR0{
	float pi = 3.14159265359;
	float4 color = sampleColor;
	if(uDirection<0){
		coords.x = 1-coords.x;
	}
	float2 absoluteCoords = (uImageSize0*coords*2)+uWorldPosition;
	float4 starColor = tex2D(uImage1, (absoluteCoords%uImageSize1)/uImageSize1);
	float star = starColor.b;
	float2 off = float2(starColor.g*cos(starColor.r*pi), starColor.g*sin(starColor.r*pi))*3;
	float2 c = coords+(off/uImageSize0);
	//c = RectClamp(c, Div(uSourceRect, uImageSize0));
	float4 color2 = tex2D(uImage0, c);
	color2.rgb += float3(star*0.64, star*0.7, star);
	color.rgb = color.rgb*color2.rgb;
	color.a = (tex2D(uImage0, coords).a+color2.a)/2;
	color.rgb *= color.a;
	return color;
}

technique Technique1{
	pass Nebula{
		PixelShader = compile ps_2_0 Nebula();
	}
}