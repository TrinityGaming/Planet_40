float4 Color;

sampler SpriteSampler : register(s0);
sampler IntensitySampler : register(s1);


float4 CombinePixelShader(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 SpriteColor = tex2D(SpriteSampler, texCoord);
    
    // NOTE: the 2.0 here is the width/height of the intensity texture - make sure it's updated if that texture changes
    float4 Intensity = tex2D(IntensitySampler, float2(0.5 / 1.0, 0.5 / 1.0)); 
    
    SpriteColor.a *= (Intensity * 0.5f);
    SpriteColor *= Color;
    
    return SpriteColor;
}


technique BloomCombine
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 CombinePixelShader();
    }
}
