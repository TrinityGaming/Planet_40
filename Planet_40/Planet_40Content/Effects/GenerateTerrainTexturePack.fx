float HeightScale;

texture HeightMapTexture;
texture NormalMapTexture;
texture SlopeMapTexture;

texture DiffuseTexture0;
texture DiffuseTexture1;
texture DiffuseTexture2;
texture DiffuseTexture3;
texture DiffuseTexture4;
texture DiffuseTexture5;
texture DiffuseTexture6;
texture DiffuseTexture7;
texture DiffuseTexture8;
texture DiffuseTexture9;
texture DiffuseTextureA;
texture DiffuseTextureB;


sampler heightmap = sampler_state
{
  texture=<HeightMapTexture>;
  magfilter = POINT;
  minfilter = POINT;
  mipfilter = NONE;
  AddressU = clamp; 
  AddressV = clamp; 
};

sampler normalmap = sampler_state
{
  texture=<NormalMapTexture>;
  magfilter = POINT;
  minfilter = POINT;
  mipfilter = NONE;
  AddressU = clamp; 
  AddressV = clamp; 
};

sampler slopemap = sampler_state
{
  texture=<SlopeMapTexture>;
  magfilter = POINT;
  minfilter = POINT;
  mipfilter = NONE;
  AddressU = clamp; 
  AddressV = clamp; 
};

sampler diffuse0 = sampler_state
{
  texture=<DiffuseTexture0>;
  magfilter = LINEAR;
  minfilter = LINEAR;
  mipfilter = LINEAR;
  AddressU = wrap; 
  AddressV = wrap; 
};

sampler diffuse1 = sampler_state
{
  texture=<DiffuseTexture1>;
  magfilter = LINEAR;
  minfilter = LINEAR;
  mipfilter = LINEAR;
  AddressU = wrap; 
  AddressV = wrap; 
};

sampler diffuse2 = sampler_state
{
  texture=<DiffuseTexture2>;
  magfilter = LINEAR;
  minfilter = LINEAR;
  mipfilter = LINEAR;
  AddressU = wrap; 
  AddressV = wrap; 
};

sampler diffuse3 = sampler_state
{
  texture=<DiffuseTexture3>;
  magfilter = LINEAR;
  minfilter = LINEAR;
  mipfilter = LINEAR;
  AddressU = wrap; 
  AddressV = wrap; 
};

sampler diffuse4 = sampler_state
{
  texture=<DiffuseTexture4>;
  magfilter = LINEAR;
  minfilter = LINEAR;
  mipfilter = LINEAR;
  AddressU = wrap; 
  AddressV = wrap; 
};

sampler diffuse5 = sampler_state
{
  texture=<DiffuseTexture5>;
  magfilter = LINEAR;
  minfilter = LINEAR;
  mipfilter = LINEAR;
  AddressU = wrap; 
  AddressV = wrap; 
};

sampler diffuse6 = sampler_state
{
  texture=<DiffuseTexture6>;
  magfilter = LINEAR;
  minfilter = LINEAR;
  mipfilter = LINEAR;
  AddressU = wrap; 
  AddressV = wrap; 
};

sampler diffuse7 = sampler_state
{
  texture=<DiffuseTexture7>;
  magfilter = LINEAR;
  minfilter = LINEAR;
  mipfilter = LINEAR;
  AddressU = wrap; 
  AddressV = wrap; 
};

sampler diffuse8 = sampler_state
{
  texture=<DiffuseTexture8>;
  magfilter = LINEAR;
  minfilter = LINEAR;
  mipfilter = LINEAR;
  AddressU = wrap; 
  AddressV = wrap; 
};

sampler diffuse9 = sampler_state
{
  texture=<DiffuseTexture9>;
  magfilter = LINEAR;
  minfilter = LINEAR;
  mipfilter = LINEAR;
  AddressU = wrap; 
  AddressV = wrap; 
};

sampler diffuseA = sampler_state
{
  texture=<DiffuseTextureA>;
  magfilter = LINEAR;
  minfilter = LINEAR;
  mipfilter = LINEAR;
  AddressU = wrap; 
  AddressV = wrap; 
};

sampler diffuseB = sampler_state
{
  texture=<DiffuseTextureB>;
  magfilter = LINEAR;
  minfilter = LINEAR;
  mipfilter = LINEAR;
  AddressU = wrap; 
  AddressV = wrap; 
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
  output.UV = input.UV;
}


float4 GetDiffuseColor(float2 uv, float slope, float altitude)
{
  // slope goes from 0 to 1, so we need to scale it over a single 1/4 section of the slope map

  float4 weights0 = tex2D(slopemap, float2(slope * 0.34 + 0.00, altitude));
  float4 weights1 = tex2D(slopemap, float2(slope * 0.33 + 0.34, altitude));
  float4 weights2 = tex2D(slopemap, float2(slope * 0.33 + 0.67, altitude));

  /*
  float4 weights0 = tex2D(slopemap, float2(slope * 0.25 + 0.00, altitude));
  float4 weights1 = tex2D(slopemap, float2(slope * 0.25 + 0.25, altitude));
  float4 weights2 = tex2D(slopemap, float2(slope * 0.25 + 0.50, altitude));
  float4 weights3 = tex2D(slopemap, float2(slope * 0.25 + 0.75, altitude));
  */

  uv *= 5;

  // get blending of the 16 diffuse textures based on slop map weights
  float4 diffuse = tex2D(diffuse0, uv) * weights0.x +
                   tex2D(diffuse1, uv) * weights0.y +
                   tex2D(diffuse2, uv) * weights0.z +
                   tex2D(diffuse3, uv) * weights0.w +
                   tex2D(diffuse4, uv) * weights1.x +
                   tex2D(diffuse5, uv) * weights1.y +
                   tex2D(diffuse6, uv) * weights1.z +
                   tex2D(diffuse7, uv) * weights1.w +
                   tex2D(diffuse8, uv) * weights2.x +
                   tex2D(diffuse9, uv) * weights2.y +
                   tex2D(diffuseA, uv) * weights2.z +
                   tex2D(diffuseB, uv) * weights2.w;
                   
                   
  return diffuse;
}


float4 TexturePixelShader(in VertexOutput input) : COLOR0
{
  // calculate the terrain altitude
  float2 uv = input.UV;
  uv.y = 1 - uv.y;
  float height = tex2D(heightmap, uv).r;
  
  // normalize height
  height *= HeightScale;
  
  // calculate slope
  float3 normal = tex2D(normalmap, uv).rgb * 2.0 - 1.0;
  float slope = normal.y;
 
  /*
  // add noise to slope and height for better transitions  
  vec4 noiseMap = texture2D(terrainLUT, uv0 * 128.0);
  vec4 noiseMap2 = texture2D(terrainLUT, uv0 * 64.0);
  vec4 noiseMap3 = texture2D(terrainLUT, uv0 * 32.0);
  vec4 noiseMap4 = texture2D(terrainLUT, uv0 * 16.0); 
  float n = noiseMap.y + noiseMap2.y + noiseMap3.y + noiseMap4.y;
  n *= 0.25;

  uv1.x = 0.0;
  uv1.y = uv1.y + ((n - 0.5) * 0.2);
  // this clamping might be wrong - because i'm scaling my LUT values by 16 below
  uv1.y = clamp(uv1.y, 0.1, 0.9);
  */
  
  // get diffuse color
  return GetDiffuseColor(uv, 1.0 - slope, 1.0 - height);
}



technique GenerateTexture
{
  pass Pass1
  {
    VertexShader = compile vs_3_0 TextureVertexShader();
    PixelShader = compile ps_3_0 TexturePixelShader();
  }
}


