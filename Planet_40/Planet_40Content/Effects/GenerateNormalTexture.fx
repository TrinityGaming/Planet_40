texture heightmap;
float PatchWidth;
float PatchHeight;
float NormalScale;

sampler heightmapSampler = sampler_state {
  texture = <heightmap>; 
  AddressU = clamp;
  AddressV = clamp;
  magfilter = POINT;
  minfilter = POINT; 
  mipfilter = NONE; 
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




void NormalVertexShader(in VertexInput input, out VertexOutput output)
{
  output.Position = input.Position;
  output.UV = input.UV;
}



/*
float4 NormalPixelShader(in VertexOutput input) : COLOR0
{

  float textureSize = 256.0f;
  float texelSize =  1.0f / textureSize ; //size of one texel;
  float normalStrength = 0.05f;
  
    float tl = abs(tex2D (heightmapSampler, uv + texelSize * float2(-1, -1)).x);   // top left
    float  l = abs(tex2D (heightmapSampler, uv + texelSize * float2(-1,  0)).x);   // left
    float bl = abs(tex2D (heightmapSampler, uv + texelSize * float2(-1,  1)).x);   // bottom left
    float  t = abs(tex2D (heightmapSampler, uv + texelSize * float2( 0, -1)).x);   // top
    float  b = abs(tex2D (heightmapSampler, uv + texelSize * float2( 0,  1)).x);   // bottom
    float tr = abs(tex2D (heightmapSampler, uv + texelSize * float2( 1, -1)).x);   // top right
    float  r = abs(tex2D (heightmapSampler, uv + texelSize * float2( 1,  0)).x);   // right
    float br = abs(tex2D (heightmapSampler, uv + texelSize * float2( 1,  1)).x);   // bottom right
 
    // Compute dx using Sobel:
    //           -1 0 1 
    //           -2 0 2
    //           -1 0 1
    float dX = tr + 2*r + br -tl - 2*l - bl;
 
    // Compute dy using Sobel:
    //           -1 -2 -1 
    //            0  0  0
    //            1  2  1
    float dY = bl + 2*b + br -tl - 2*t - tr;
 
    // Build the normalized normal
    float4 N = float4(normalize(float3(dX, 1.0f / normalStrength, dY)), 1.0f);
 
    //convert (-1.0 , 1.0) to (0.0 , 1.0), if needed
    return N * 0.5f + 0.5f;
 }
*/ 



float4 NormalPixelShader(in VertexOutput input) : COLOR0
{
  float2 uv = input.UV;
  
  uv.y = 1 - uv.y;
  
  float du = 1.0 / PatchWidth;
  float dv = 1.0 / PatchHeight;

  float l = tex2D(heightmapSampler, uv + float2(-du, 0));
  float r = tex2D(heightmapSampler, uv + float2(du, 0));
  float t = tex2D(heightmapSampler, uv + float2(0, dv));
  float b = tex2D(heightmapSampler, uv + float2(0, -dv));
  
  float3 normal = normalize(float3((l - r) * NormalScale, 1.0, (b - t) * NormalScale));
  return float4(normal * 0.5 + 0.5, 1.0);
};







technique GenerateTexture
{
  pass Pass1
  {
    VertexShader = compile vs_3_0 NormalVertexShader();
    PixelShader = compile ps_3_0 NormalPixelShader();
  }
}


