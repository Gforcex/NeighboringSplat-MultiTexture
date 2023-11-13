#ifndef __NEIGHBORING_SPLAT_HLSL__
#define __NEIGHBORING_SPLAT_HLSL__

float2 GetHalfPixelUV(float2 uv, float texSize)
{
	//float texHalfSize = texSize * 0.5f;
	//
	//float2 halfUV = floor(uv * texHalfSize) / texHalfSize;
	//float2 halfUVLerp = frac(uv * texHalfSize) / texHalfSize;
	//return halfUV + halfUVLerp * 0.5f + 0.5 / texSize;

	uv -= 1.0 / texSize;
	float texHalfSize = texSize * 0.5f;

	float2 halfUV = floor(uv * texHalfSize) / texHalfSize;
	//float2 fullUV = floor(uv * texSize) / texSize;

	float2 halfUVLerp = frac(uv * texHalfSize) / texHalfSize;
	//float2 fullUVLerp = frac(uv * texSize) / texSize;

	//float2 subUV = clamp(fullUVLerp, 0, 0.5 / texSize);

	return halfUV + halfUVLerp * 0.5 + 0.5 / texSize;
}

#endif //__NEIGHBORING_SPLAT_HLSL__