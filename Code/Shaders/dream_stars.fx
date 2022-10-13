// Pixel shader combines the bloom image with the original
// scene, using tweakable intensity levels and saturation.
// This is the final step in applying a bloom postprocess.

sampler screen : register(s0);

struct VertexInput {
	float4 Color		: COLOR0;
	float2 UVMapping	: TEXCOORD0;
	float4 Position		: SV_Position;
};
struct PixelInput
{
	float4 Color		: COLOR0;
	float2 UVMapping	: TEXCOORD0;
	float2 Position		: TEXCOORD1;
};
struct PixelOutput
{
	float4 Color		: COLOR0;
	float Depth			: SV_Depth;
};

float2 pos;

PixelInput VertexShaderF(VertexInput input)
{
	PixelInput output;
	
	output.Color = input.Color;
	output.UVMapping = input.UVMapping;
	output.Position = float2(1, 1);
	
	return output;
}
PixelOutput PixelShaderF(PixelInput input)
{
	PixelOutput output;
	
	output.Color = tex2D(screen, input.UVMapping) * input.Color;
	output.Depth = 1;
	
	return output;
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
