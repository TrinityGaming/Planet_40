float4x4 WorldViewProj;


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
struct TransformedVertex
{
  float4 Position : POSITION0;
};


void PlanetMaskVertexShader(in InputVertex input, out TransformedVertex output)
{
  output.Position = mul(input.Position, WorldViewProj);
}



float4 PlanetMaskPixelShader(in TransformedVertex input) : COLOR0
{
  return float4(0, 0, 0, 1);
}



technique RenderPlanetMask
{
	pass p0
	{	
		VertexShader = compile vs_2_0 PlanetMaskVertexShader();
		PixelShader = compile ps_2_0 PlanetMaskPixelShader();
	}	
}



