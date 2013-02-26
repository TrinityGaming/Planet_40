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
  mipfilter = NONE;
  AddressU = clamp; 
  AddressV = clamp; 
};



sampler NormalSampler = sampler_state
{
  texture=<NormalTexture>; 
  magfilter = LINEAR;
  minfilter = LINEAR;
  mipfilter = NONE;
  AddressU = clamp; 
  AddressV = clamp; 
};



// application to vertex structure
struct InputVertex
{
  float4 Position : POSITION0;
  float3 Normal : NORMAL0;
  float2 UV : TEXCOORD0;
  float Height : TEXCOORD1;
  float4 Tangent : TANGENT0;
};


// vertex to pixel shader structure
struct OutputVertex
{
  float4 Position : POSITION0;
  float2 UV : TEXCOORD0;
  float3 LightDirection : TEXCOORD1;
};


void PlanetBumpVertexShader(in InputVertex input, out OutputVertex output)
{
  output.Position = mul(input.Position, WorldViewProj);
  output.UV = input.UV;
  
  // create matrix for transforming from tangent space to object space
  float3x3 tangentToObject;
  tangentToObject[0] = normalize(cross(input.Tangent, input.Normal) * input.Tangent.w);
  tangentToObject[1] = normalize(input.Tangent);
  tangentToObject[2] = normalize(input.Normal);
  
  float3x3 tangentToWorld = mul(tangentToObject, WorldMatrix);
  output.LightDirection = mul(tangentToWorld, LightDirection);
}


float4 PlanetBumpPixelShader(in OutputVertex input) : COLOR0
{
  float4 AmbientColor = { 1, 1, 1, 1 };
  float AmbientIntensity = 0.01;

  float4 DiffuseColor = { 1, 1, 1, 1 };
  float DiffuseIntensity = 0.75f;
  
  float2 uv = input.UV;
  DiffuseColor = tex2D(DiffuseSampler, uv);
  
  float3 bump = tex2D(NormalSampler, uv).rgb;
  float3 normal = bump * 2.0 - 1.0;
  normal = normal.rbg;  // note that we're reversing b and g
  float Diffuse = dot(normalize(normal), normalize(input.LightDirection));
  DiffuseColor *= Tint;
  
  float4 f = AmbientColor * AmbientIntensity + 
             DiffuseColor * DiffuseIntensity * Diffuse;

  return float4(f.rgb, 1);
}




technique RenderPlanetBumpMapping
{
	pass p0
	{	
		VertexShader = compile vs_3_0 PlanetBumpVertexShader();
		PixelShader = compile ps_3_0 PlanetBumpPixelShader();
	}	
}
