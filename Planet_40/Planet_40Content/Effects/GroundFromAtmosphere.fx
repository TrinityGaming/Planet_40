float4x4 WorldViewProj;
float4x4 ReflectionWorldViewProj;
float3 PatchPosition;
float MorphFactor;

float CullingTest;
float ClipBelowWater;

float3 EyePosition;
float3 LightPosition;

float3 v3LightPos;		// Light direction
float3 v3CameraPos;		// Camera's current position
float3 v3InvWavelength;	// 1 / pow(wavelength, 4) for RGB channels
float fCameraHeight;
float fCameraHeight2;
float fInnerRadius;
float fInnerRadius2;
float fOuterRadius;
float fOuterRadius2;

// Scattering parameters
float fKrESun;			// Kr * ESun
float fKmESun;			// Km * ESun
float fKr4PI;			// Kr * 4 * PI
float fKm4PI;			// Km * 4 * PI

// Phase function
float g;
float g2;

float fScale;			// 1 / (outerRadius - innerRadius) = 4 here
float fScaleDepth;		// Where the average atmosphere density is found
float fScaleOverScaleDepth;	// scale / scaleDepth
float fExposure;	// Exposure parameter for pixel shader

int nSamples;
float fSamples;

Texture GrassTexture;
Texture RockTexture;
Texture SandTexture;
Texture SnowTexture;
Texture Slopemap;


sampler GrassSampler = sampler_state {
  texture=<GrassTexture>; 
  magfilter = LINEAR;
  minfilter = LINEAR; 
  mipfilter = LINEAR; 
  AddressU = clamp; 
  AddressV = clamp; 
};

sampler RockSampler = sampler_state {
  texture=<RockTexture>;
  magfilter = LINEAR;
  minfilter = LINEAR; 
  mipfilter = LINEAR; 
  AddressU = clamp; 
  AddressV = clamp; 
};

sampler SandSampler = sampler_state {
  texture=<SandTexture>;
  magfilter = LINEAR;
  minfilter = LINEAR; 
  mipfilter = LINEAR; 
  AddressU = clamp; 
  AddressV = clamp; 
};

sampler SnowSampler = sampler_state {
  texture=<SnowTexture>;
  magfilter = LINEAR;
  minfilter = LINEAR; 
  mipfilter = LINEAR; 
  AddressU = clamp; 
  AddressV = clamp; 
};

sampler SlopemapSampler = sampler_state {
  texture=<Slopemap>;
  magfilter = POINT;
  minfilter = POINT; 
  mipfilter = POINT; 
  AddressU = clamp; 
  AddressV = clamp; 
};

/* TODO :

- Position becomes SourcePosition
- Add TargetPosition which will is the final desired position
- Add MorphFactor global which will range from 0 to 1
- Lerp between SourcePosition and TargetPosition

- SourcePosition is from the lower resolution patch
  - it will be calculated as the center of the 
  - 


*/

// Application to vertex structure
struct a2v
{
	float4 Position         : POSITION0;
	float4 SeaLevelPosition : POSITION1;
	float4 MorphPosition    : POSITION2;
  float3 Normal           : NORMAL0;
  float3 MorphNormal      : NORMAL1;
  float2 Texture          : TEXCOORD0;
  float4 Color            : COLOR0;
};

// Vertex to pixel shader structure
struct v2p
{
	float4 Position       : POSITION0;
	float4 RayleighColor  : TEXCOORD0;
	float4 MieColor       : TEXCOORD1;
  float2 T              : TEXCOORD2;
  float3 P              : TEXCOORD3;
  float3 N              : TEXCOORD4;
  float4 VertexColor    : COLOR0;
	float  Height         : COLOR1;
};



float scale(float cos)
{
	float x = 1.0 - cos;
	return fScaleDepth * exp(-0.00287 + x * (0.459 + x * (3.83 + x * (-6.80 + x * 5.25))));
}


void RenderSkyVS(in a2v IN, out v2p OUT)
{
  float4 InPosition = lerp(IN.MorphPosition, IN.Position, MorphFactor);
  float3 InNormal = lerp(IN.MorphNormal, IN.Normal, MorphFactor);

  // translate the vertex into planet space
	float3 v3Pos = InPosition.xyz + PatchPosition;
	float3 v3SeaLevelPos = IN.SeaLevelPosition.xyz + PatchPosition;

	// Get the ray from the camera to the vertex, and it's length (far point)
  float3 v3Ray = v3Pos - v3CameraPos;
	float fFar = length(v3Ray);
	v3Ray /= fFar;
	
  // Calculate the ray's starting position, then calculate its scattering offset			
	float3 v3Start = v3CameraPos;
	float fDepth = exp((fInnerRadius - fCameraHeight) / fScaleDepth);

  float fCameraAngle;
  if (length(v3CameraPos) >= length(v3Pos))
	  fCameraAngle = dot(-v3Ray, v3Pos) / length(v3Pos);
	else
	  fCameraAngle = dot(v3Ray, v3Pos) / length(v3Pos);
	
	float fLightAngle = dot(v3LightPos, v3Pos) / length(v3Pos);
	float fCameraScale = scale(fCameraAngle);
	float fLightScale = scale(fLightAngle);
	float fCameraOffset = fDepth*fCameraScale;
	float fTemp = (fLightScale + fCameraScale);

			
	// Initialize the scattering loop variables
	float fSampleLength = fFar / fSamples;
	float fScaledLength = fSampleLength * fScale;
	float3 v3SampleRay = v3Ray * fSampleLength;
	float3 v3SamplePoint = v3Start + v3SampleRay * 0.5f;

	
	// Now loop through the sample rays
	float3 v3FrontColor = float3(0.0, 0.0, 0.0);
	float3 v3Attenuate;
	for(int i=0; i<nSamples; i++)
	{
		float fHeight = length(v3SamplePoint);
		float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fHeight));
		float fScatter = fDepth*fTemp - fCameraOffset;
		v3Attenuate = exp(-fScatter * (v3InvWavelength * fKr4PI + fKm4PI));
		v3FrontColor += v3Attenuate * (fDepth * fScaledLength);
		v3SamplePoint += v3SampleRay;
	}


	// finally, scale the Mie and Rayleigh colors
	OUT.RayleighColor.rgb = v3FrontColor * (v3InvWavelength * fKrESun + fKmESun);
	OUT.RayleighColor.a = 1.0f;
	
	// calculate the attenuation factor for the ground
	OUT.MieColor.rgb = v3Attenuate;
	OUT.MieColor.a = 1.0f;
	
  // calculate final pixel shader parameters  
  OUT.Position = mul(InPosition, WorldViewProj);
	OUT.Height = saturate((length(v3Pos) - length(v3SeaLevelPos)) / 12.0);
  OUT.P = v3Pos;
  OUT.T = IN.Texture;
  OUT.N = normalize(InNormal);
  
  OUT.VertexColor = IN.Color;
}


float4 RenderSkyPS(in v2p IN) : COLOR0
{
  float4 AmbientColor = { 1.0f, 1.0f, 1.0f, 1.0f };
  float AmbientIntensity = 0.01f;
  
  float4 DiffuseColor;
  float DiffuseIntensity = 0.55f; 

  float4 F;
  float H = clamp(IN.Height.x, 0, 1.0);

  if (ClipBelowWater == 1 && H <= 0)
  {
    clip(-1);
    F = float4(0, 0, 0, 1);
  }
  else
  {
    float Slope = 0.4;
    float2 T = IN.T; // * 13.0;
    
    /*
    float4 Weights = tex2D(SlopemapSampler, float2(Slope, 1.0 - H));

    DiffuseColor = tex2D(GrassSampler, T) * Weights.a;
    DiffuseColor += tex2D(RockSampler, T) * Weights.r;
    DiffuseColor += tex2D(SandSampler, T) * Weights.g;
    DiffuseColor += tex2D(SnowSampler, T) * Weights.b;
    DiffuseColor = saturate(DiffuseColor);
    */
    
    DiffuseColor.rgb = float3(1.0, 1.0, 1.0);
    DiffuseColor.a = 1;
    
    float3 LightDir = normalize(IN.P - LightPosition);
    float Diffuse = saturate(dot(IN.N, LightDir));
    // float3 Reflect = normalize(2 * Diffuse * IN.N - LightDir);
    
    F = AmbientColor * AmbientIntensity + 
        DiffuseColor * DiffuseIntensity * Diffuse;
    
    //F = IN.RayleighColor + F * IN.MieColor;
    //F = (1 - exp(-fExposure * F));

    if (CullingTest > 0)
      F = DiffuseColor * DiffuseIntensity;
  }


  /*    
  DiffuseColor = WaterColor * WaterBlendFactor + WaterTerm * (1 - WaterBlendFactor);
  */
  
  /*
  DetailColor = tex2D(WaterSampler, IN.T * 13);
  
  float BlendDistance = 0.0001;
  float BlendWidth = 0.01;
  float BlendFactor = clamp((IN.Depth - BlendDistance) / BlendWidth, 0.7, 1);
  DiffuseColor = DiffuseColor * BlendFactor + DetailColor * (1 - BlendFactor);
  */
  
  

  return F;
}



technique RenderSky
{
	pass p0
	{	
		VertexShader = compile vs_3_0 RenderSkyVS();
		PixelShader = compile ps_3_0 RenderSkyPS();
	}	
}
