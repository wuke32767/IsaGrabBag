// Pixel shader combines the bloom image with the original
// scene, using tweakable intensity levels and saturation.
// This is the final step in applying a bloom postprocess.

sampler screen : register(s0);

float size;

float4 PixelShaderF(float2 texCoord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	
	float4 b = tex2D(screen, texCoord);
	
	if (b.a == 0){
	
		float2 offset = float2(1 / 362.0, 1 / 182.0);
		
		b = tex2D(screen, texCoord + float2(offset[0], 0));
		if (b.a == 1)
			return color;
		b = tex2D(screen, texCoord - float2(offset[0], 0));
		if (b.a == 1)
			return color;
		b = tex2D(screen, texCoord + float2(0, offset[0]));
		if (b.a == 1)
			return color;
		b = tex2D(screen, texCoord - float2(0, offset[0]));
		if (b.a == 1)
			return color;

		return 0;
	}
	
	return b;
}


technique BloomCombine
{
    pass Pass1
    {
#if SM4
        PixelShader = compile ps_4_0_level_9_1 PixelShaderF();
#elif SM3
        PixelShader = compile ps_3_0 PixelShaderF();
#else
        PixelShader = compile ps_2_0 PixelShaderF();
#endif
    }
}
