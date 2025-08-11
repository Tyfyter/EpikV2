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
float4 uShaderSpecificData;
float4 uSourceRect0;
float4 uSourceRect1;
float4 uAlphaMatrix0;
float4 uAlphaMatrix1;


float4 ApplyAlphaMatrix(float4 color, float4 _matrix) {
	return float4(color.r, color.g, color.b, min(color.r * _matrix.r + color.g * _matrix.g + color.b * _matrix.b + color.a * _matrix.a, 1));
}

float2 ApplySourceRect(float2 uv, float4 sourceRect) {
	return uSourceRect0.xy + uv * uSourceRect0.zw;
}

float4 Identity(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0 {
	return color * ApplyAlphaMatrix(tex2D(uImage0, ApplySourceRect(uv, uSourceRect0)), uAlphaMatrix0) * uOpacity;
}

technique Technique1 {
	pass Identity {
		PixelShader = compile ps_2_0 Identity();
	}
}
