Shader"Unlit/TextureArrayBlend"
{
    Properties
    {
        [Toggle(_BILINEAR_WEIGHT)] _BilinearWeight("Bilinear Weight", Float) = 0.0
        _IdMap("Id Map", 2D) = "" {}
        _WeightMap("Weight Map", 2D) = "" {}
        _TexArr ("Tex Array", 2DArray) = "" {}

        [Toggle(_HARD_WEIGHT)] _HardWeight("Hard Weight", Float) = 0.0
        _Splat1Map("Splat 1 Map", 2D) = "" {}
        _Splat2Map("Splat 2 Map", 2D) = "" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature_local_fragment _BILINEAR_WEIGHT
            #pragma shader_feature_local_fragment _HARD_WEIGHT

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "NeighboringSplat/NeighboringSplat.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            TEXTURE2D(_IdMap);
            TEXTURE2D(_WeightMap);  SAMPLER(sampler_WeightMap);
            TEXTURE2D_ARRAY(_TexArr); SAMPLER(sampler_TexArr);

            TEXTURE2D(_Splat1Map);  SAMPLER(sampler_Splat1Map);
            TEXTURE2D(_Splat2Map);  SAMPLER(sampler_Splat2Map);

            float4 _WeightMap_TexelSize;
            float4 _IdMap_TexelSize;

            v2f vert (appdata v)
            {
                v2f o;
	            o.vertex = TransformObjectToHClip(v.vertex.xyz);

	            float3 positionWS = TransformObjectToWorld(v.vertex.xyz);

	            float2 worldSize = 1000;
	            o.uv = (positionWS.xz - unity_ObjectToWorld._14_24_34.xz + worldSize * 0.5) / worldSize;

	            //o.uv = 1.0 - float2(v.uv.y, v.uv.x);
	            //o.uv = 1.0 - v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {	
	            float2 tilingUV = i.uv * 2.0f;

	            half4 col = 0;
        #ifdef _HARD_WEIGHT

	            half4 weight1 = SAMPLE_TEXTURE2D(_Splat1Map, sampler_Splat1Map, i.uv);
	            half4 weight2 = SAMPLE_TEXTURE2D(_Splat2Map, sampler_Splat2Map, i.uv);

	            col += SAMPLE_TEXTURE2D_ARRAY(_TexArr, sampler_TexArr, tilingUV, 0) * weight1.r;
	            col += SAMPLE_TEXTURE2D_ARRAY(_TexArr, sampler_TexArr, tilingUV, 1) * weight1.g;
	            col += SAMPLE_TEXTURE2D_ARRAY(_TexArr, sampler_TexArr, tilingUV, 2) * weight1.b;
	            col += SAMPLE_TEXTURE2D_ARRAY(_TexArr, sampler_TexArr, tilingUV, 3) * weight1.a;
	            col += SAMPLE_TEXTURE2D_ARRAY(_TexArr, sampler_TexArr, tilingUV, 4) * weight2.r;
	            col += SAMPLE_TEXTURE2D_ARRAY(_TexArr, sampler_TexArr, tilingUV, 5) * weight2.g;
        #else

	            float3 id = LOAD_TEXTURE2D(_IdMap, (i.uv - (0.5 / _IdMap_TexelSize.zw)) * _IdMap_TexelSize.zw).rgb * 255;

            #ifdef _BILINEAR_WEIGHT
                half3 weight = SAMPLE_TEXTURE2D(_WeightMap, sampler_WeightMap, GetHalfPixelUV(i.uv, _WeightMap_TexelSize.zw)).rgb;
            #else
	            float2 uv = floor(i.uv * _WeightMap_TexelSize.zw) / _WeightMap_TexelSize.zw;
                half3 weight = SAMPLE_TEXTURE2D(_WeightMap, sampler_WeightMap, uv).rgb;
            #endif

          	    //weight.b = 1.0 - weight.r - weight.g;

	            col += SAMPLE_TEXTURE2D_ARRAY(_TexArr, sampler_TexArr, tilingUV, id.r) * weight.r;
	            col += SAMPLE_TEXTURE2D_ARRAY(_TexArr, sampler_TexArr, tilingUV, id.g) * weight.g;
	            col += SAMPLE_TEXTURE2D_ARRAY(_TexArr, sampler_TexArr, tilingUV, id.b) * weight.b;
        #endif
	            return col;

            }
            ENDHLSL
        }
    }
}
