float4x4 WorldViewProj;
float3 LightDirection;
float4x4 WorldMatrix;
texture DiffuseTexture;
texture NormalTexture;
float4 Tint;


sampler DiffuseSampler = sampler_state
{
  texture=<DiffuseTexture>; 
  magfilter = LINEAR;
  minfilter = LINEAR;
  mipfilter = LINEAR;
  AddressU = clamp; 
  AddressV = clamp; 
};



sampler NormalSampler = sampler_state
{
  texture=<NormalTexture>; 
  magfilter = LINEAR;
  minfilter = LINEAR;
  mipfilter = LINEAR;
  AddressU = clamp; 
  AddressV = clamp; 
};



// application to vertex structure
struct InputVertex
{
  float4 Position : POSITION0;
  float3 Normal : NORMAL0;
  float2 UV : TEXCOORD0;
};


// vertex to pixel shader structure
struct OutputVertex
{
  float4 Position : POSITION0;
  float2 UV : TEXCOORD0;
};


void PlanetBumpVertexShader(in InputVertex input, out OutputVertex output)
{
  output.Position = mul(input.Position, WorldViewProj);
  output.UV = input.UV;
}


float4 PlanetBumpPixelShader(in OutputVertex input) : COLOR0
{
  float3 bump = tex2D(NormalSampler, input.UV).rgb;
  return float4(bump.rgb, 1) * Tint;
}


technique RenderPlanetBumpMapping
{
	pass p0
	{	
		VertexShader = compile vs_2_0 PlanetBumpVertexShader();
		PixelShader = compile ps_2_0 PlanetBumpPixelShader();
	}	
}



