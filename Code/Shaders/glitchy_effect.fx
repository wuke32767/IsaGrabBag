// Pixel shader combines the bloom image with the original
// scene, using tweakable intensity levels and saturation.
// This is the final step in applying a bloom postprocess.

texture spriteTexture;
sampler2D spriteSampler = sampler_state {
	Texture = (spriteTexture);
	MagFilter = Linear;
	MinFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

float time, strength;

struct VertexInput {
	float4 Position : POSITION0;
	float2 UVMapping : TEXCOORD0;
};
struct VertexOutput {
	float4 Position : POSITION0;
	float2 UVMapping : TEXCOORD0;
};

float4 getframe(float2 uv){

	float4 color = tex2D(spriteSampler, uv);
	
	return color;
}

float rand1(float co){
    return frac(sin(dot(float2(1, co), float2(12.9898, 78.233))) * 43758.5453);
}

float4 PixelShaderF(VertexOutput input) : COLOR0
{
	float2 uv = input.UVMapping;
	
	float yPos = uv[1] * 64;
	
	float factor = yPos % 1;
	yPos = floor(yPos);
	factor = abs(factor * 2 - 1);
	
	uv[0] += (rand1(yPos + time) - 0.5f) * 0.03 * strength;
	
	yPos /= 64;
	
	float alpha = getframe(uv)[3];
	
	float4 red = getframe(uv + float2(0.025 * strength, 0.0));
	float4 green = getframe(uv + float2(0, 0.0));
	float4 blue = getframe(uv + float2(-0.025 * strength, 0.0));
	
	red = float4(red[0], green[1], blue[2], 0);
	
	red = lerp(red, 0, factor * 0.5 * strength);
	red = lerp(red, red * rand1(yPos + time), 0.4 * strength) * 1.3;
	
	red[3] = alpha;
	
	return red;
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