float4x4 WorldViewProjectionMatrix : WorldViewProjection;
texture Billboard;
float MaskColor;

sampler BillboardSampler = sampler_state {
  texture=<Billboard>; 
  magfilter = LINEAR;
  minfilter = LINEAR; 
  mipfilter = LINEAR; 
  AddressU = clamp;
  AddressV = clamp; 
};

struct InputVertex
{
  float4 Position : Position;
  float2 UV : TexCoord0;
};

struct TransformedVertex
{
  float4 Position : Position;
  float2 UV : TexCoord0;
};


void BillboardVertexShader(in InputVertex input, out TransformedVertex output)
{
  output.Position = mul(input.Position, WorldViewProjectionMatrix);
  output.UV = input.UV;
}


float4 BillboardPixelShader(in TransformedVertex input) : Color0
{
  float4 c;

  c = tex2D(BillboardSampler, input.UV);

  if (MaskColor != 0)
    if (c.a > 0.5)
      c = float4(MaskColor, MaskColor, MaskColor, 1);
    else
      c = float4(0, 0, 0, 0);
      
  return c;
}


technique TransformTechnique
{
  pass
  { 
    CullMode = none;
    VertexShader = compile vs_2_0 BillboardVertexShader();
    PixelShader = compile ps_2_0 BillboardPixelShader();
  }
}
