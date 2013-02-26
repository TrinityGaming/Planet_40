float4x4 WorldViewProj;
float3 LightDirection;
texture DiffuseTexture;
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
  float2 UV :TEXCOORD0;
  float3 Normal : TEXCOORD1;
};


void PlanetBasicVertexShader(in InputVertex input, out OutputVertex output)
{
  output.Position = mul(input.Position, WorldViewProj);
  output.UV = input.UV;
  output.Normal = input.Normal;
}



float4 PlanetBasicPixelShader(in OutputVertex input) : COLOR0
{
  float4 AmbientColor = { 1, 1, 1, 1 };
  float AmbientIntensity = 0.01;

  float4 DiffuseColor = tex2D(DiffuseSampler, input.UV);
  float DiffuseIntensity = 0.75f; 
  float Diffuse = saturate(dot(input.Normal, LightDirection));
  
  DiffuseColor *= Tint;
  
  float4 f = AmbientColor * AmbientIntensity + 
             DiffuseColor * DiffuseIntensity * Diffuse;

  return float4(f.rgb, 1);
}



technique RenderPlanetBasic
{
	pass p0
	{	
		VertexShader = compile vs_2_0 PlanetBasicVertexShader();
		PixelShader = compile ps_2_0 PlanetBasicPixelShader();
	}	
}



