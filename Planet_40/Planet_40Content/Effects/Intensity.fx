float Scale;

texture Source;

sampler MaskSampler = sampler_state {
  texture = <Source>; 
  magfilter = POINT;
  minfilter = POINT; 
  mipfilter = NONE; 
};


struct VertexInput
{
  float4 Position : Position;
  float2 Texture : TexCoord0;
};

struct VertexOutput
{
  float4 Position : Position;
  float2 Texture : TexCoord0;
};


VertexOutput IntensityVertexShader(VertexInput i)
{
  VertexOutput o = (VertexOutput)0;

  o.Position = float4(-1.0, 1.0, 0, 1);
  o.Texture = i.Texture;
  
  return o;
}


float4 IntensityPixelShader(float2 T : TEXCOORD0) : COLOR0
{
  float4 C = tex2D(MaskSampler, T) * Scale;
  C.a = 1;
  
  return C;
}


technique AllWhite
{
    pass Pass1
    {
        CullMode = none;
        VertexShader = compile vs_2_0 IntensityVertexShader();
        PixelShader = compile ps_2_0 IntensityPixelShader();
    }
}
