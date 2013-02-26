float4x4 WorldViewProj;
float3 LightDirection;
float4x4 WorldMatrix;
texture DiffuseTexture;
texture NormalTexture;
float4 Tint;

///// scattering

float3 PatchPosition;
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


///// end scattering

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
  
  float4 RayleighColor  : TEXCOORD2;
  float4 MieColor       : TEXCOORD3;
};


float scale(float cos)
{
	float x = 1.0 - cos;
	return fScaleDepth * exp(-0.00287 + x*(0.459 + x*(3.83 + x*(-6.80 + x*5.25))));
}


void CalculateScattering(float3 position, out float4 rayleighColor, out float4 mieColor)
{
  // translate the vertex into planet space
	float3 v3Pos = position.xyz + PatchPosition;

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
	rayleighColor.rgb = v3FrontColor * (v3InvWavelength * fKrESun + fKmESun);
	rayleighColor.a = 1.0f;
	
	// calculate the attenuation factor for the ground
	mieColor.rgb = v3Attenuate;
	mieColor.a = 1.0f;
  }


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
  
  
  CalculateScattering(input.Position, output.RayleighColor, output.MieColor);
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

  // apply scattering
  f = input.RayleighColor + f * input.MieColor;
  f = (1 - exp(-fExposure * f));

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
