//--------------------------------------------------------------------------------------
// Author: Gforcex@163.com
//
// https://github.com/Gforcex/NeighboringSplat-MultiTexture
//
//--------------------------------------------------------------------------------------

#ifndef __NEIGHBORING_SPLAT_HLSL__
#define __NEIGHBORING_SPLAT_HLSL__

float2 GetHalfPixelUV(float2 uv, float texSize)
{
	uv -= 1.0 / texSize;
	float texHalfSize = texSize * 0.5f;

	float2 halfUV = floor(uv * texHalfSize) / texHalfSize;
	float2 halfUVLerp = frac(uv * texHalfSize) / texHalfSize;

	return halfUV + halfUVLerp * 0.5 + 0.5 / texSize;
}

#endif //__NEIGHBORING_SPLAT_HLSL__