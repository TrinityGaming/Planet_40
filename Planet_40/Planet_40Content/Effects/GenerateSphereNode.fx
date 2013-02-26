struct InputVertex
{
  float4 Position : Position;
};

struct TransformedVertex
{
  float4 Position : Position;
};


void TerrainVertexShader(in InputVertex input, out TransformedVertex output)
{
  output.Position = input.Position;
}


float4 TerrainPixelShader(in TransformedVertex input) : COLOR0
{
  return float4(0, 0, 0, 1);  // return height above sea level (radius), which for a sphere is 0
}


technique GenerateTerrain
{
  pass Pass1
  {
    VertexShader = compile vs_3_0 TerrainVertexShader();
    PixelShader = compile ps_3_0 TerrainPixelShader();
  }
}


