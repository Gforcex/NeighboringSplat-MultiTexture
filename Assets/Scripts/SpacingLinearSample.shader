Shader"Unlit/SpacingLinearSample"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float2 GetHalfPixelUV(float2 uv, float texSize)
            {
	            //float texHalfSize = texSize * 0.5f;

	            //float2 halfUV = floor(uv * texHalfSize) / texHalfSize + 0.5 / texSize;
	            //float2 halfUVLerp = frac(uv * texHalfSize) / texHalfSize;
	            //halfUV += halfUVLerp * 0.5f;
	            //return halfUV;


	            float texHalfSize = texSize * 0.5f;
	            //uv += 0.5 / texHalfSize;
	            //uv += 0.5 / texSize;
            	uv -= 1.0 / texSize;

	            float2 halfUV = floor(uv * texHalfSize) / texHalfSize;
	            float2 fullUV = floor(uv * texSize) / texSize;

	            float2 halfUVLerp = frac(uv * texHalfSize) / texHalfSize;
	            float2 fullUVLerp = frac(uv * texSize) / texSize;

	            float2 subUV = clamp(fullUVLerp, 0, 0.5 / texSize);

	            //return halfUV + halfUVLerp * 0.5;
	            return halfUV + halfUVLerp * 0.5 + 0.5 / texSize;
	            return halfUV * 0.5 + halfUVLerp * 0.5 + 0.5 / texSize;
            }

            half4 frag (v2f i) : SV_Target
            {
	            float2 uv = GetHalfPixelUV(i.uv, _MainTex_TexelSize.zw);

	            //return uv.x;

	            return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
}
