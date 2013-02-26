float4x4 WorldViewProj;
float3 PatchPosition;
float3 LightPosition;


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
  float4 Color : COLOR0;
};


void SunBasicVertexShader(in InputVertex input, out OutputVertex output)
{
  output.Position = mul(input.Position, WorldViewProj);
  output.Color = float4(1, 1, 0, 1);
}



float4 SunBasicPixelShader(in OutputVertex input) : COLOR0
{
  float4 AmbientColor = { 1, 1, 1, 1 };
  float AmbientIntensity = 0.01;

  float4 DiffuseColor = input.Color;
  float DiffuseIntensity = 0.95f; 

  float4 f = AmbientColor * AmbientIntensity + 
             DiffuseColor * DiffuseIntensity;

  return float4(f.rgb, 1);
}



technique RenderSunVertexNormal
{
	pass p0
	{	
		VertexShader = compile vs_2_0 SunBasicVertexShader();
		PixelShader = compile ps_2_0 SunBasicPixelShader();
	}	
}
