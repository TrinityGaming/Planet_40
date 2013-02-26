float4x4 WorldViewProjectionMatrix : WorldViewProjection;

struct InputVertex
{
  float4 Position : Position;
  float4 Color : Color0;
};

struct TransformedVertex
{
  float4 Position : Position;
  float4 Color : Color0;
  float PointSize : psize;
};


void DomeVertexShader(in InputVertex input, out TransformedVertex output)
{
  output.Position = mul(input.Position, WorldViewProjectionMatrix);
  output.Color = input.Color;
  
  if (input.Color.r == 1.0)
    output.PointSize = 2.0;
  else
    output.PointSize = 1.0;
}


float4 DomePixelShader(float4 Color : Color0) : Color0
{
  return Color;
}

technique TransformTechnique
{
  pass
  {
    VertexShader = compile vs_2_0 DomeVertexShader();
    PixelShader = compile ps_2_0 DomePixelShader();
  }
}
