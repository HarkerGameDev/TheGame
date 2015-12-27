sampler TextureSampler : register(s0);


float4 main(float4 position : SV_Position, float4 texColor : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 tex = tex2D(TextureSampler, texCoord);

	float4 color = tex*texColor;
	float4 outputColor = color;
	//outputColor.r = (color.r * 0.393) + (color.g * 0.769) + (color.b * 0.189);
	//outputColor.g = (color.r * 0.349) + (color.g * 0.686) + (color.b * 0.168);    
	//outputColor.b = (color.r * 0.272) + (color.g * 0.534) + (color.b * 0.131);
	outputColor.g = color.g * 0.13;
	outputColor.b = max(color.b * color.b, (color.b * color.b * 0.7) + (color.r * 0.1) + (color.g * 0.2));
	outputColor.r = (color.r * color.r * color.r * 0.6) + (color.g * color.g * 0.2) - (outputColor.b * 0.3);
	//outputColor.b = (color.b * color.b);

    return outputColor;
}


technique Technique1
{
    pass Pass1
    {
#if SM4
        PixelShader = compile ps_5_0 main();
#else
		PixelShader = compile ps_3_0 main();
#endif
    }
}