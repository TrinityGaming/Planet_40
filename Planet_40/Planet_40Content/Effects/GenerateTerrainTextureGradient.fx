texture HeightMapTexture;
texture gradient;


sampler heightmapSampler = sampler_state {
  texture = <HeightMapTexture>; 
  magfilter = POINT;
  minfilter = POINT;
  mipfilter = NONE;
  AddressU = clamp; 
  AddressV = clamp; 
};

sampler gradientSampler = sampler_state {
  texture = <gradient>; 
  AddressU = clamp;
  AddressV = clamp;
  magfilter = POINT;
  minfilter = POINT; 
  mipfilter = POINT; 
};


struct VertexInput
{
  float4 Position : Position;
  float2 UV : TexCoord0;
};

struct VertexOutput
{
  float4 Position : Position;
  float2 UV : TexCoord0;
};




void TextureVertexShader(in VertexInput input, out VertexOutput output)
{
  output.Position = input.Position;
  output.UV = input.UV; //  HalfPixel;
}


float4 TexturePixelShader(in VertexOutput input) : COLOR0
{
  // calculate the terrain altitude
  float2 uv = input.UV;
  
  uv.y = 1 - uv.y;
  
  float height = tex2D(heightmapSampler, uv);
  
  
  // normalize height
  height /= 80.0;

  height = saturate(height);
  
  // get diffuse color
  float4 diffuse = tex1D(gradientSampler, height);
  

  return diffuse;  
}



technique GenerateTexture
{
  pass Pass1
  {
    VertexShader = compile vs_3_0 TextureVertexShader();
    PixelShader = compile ps_3_0 TexturePixelShader();
  }
}


