float4x4 WorldViewProj;
float3 LightPosition;


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



// application to vertex structure
struct InputVertex
{
  float4 Position : POSITION0;
};


// vertex to pixel shader structure
struct OutputVertex
{
  float4 Position : POSITION0;
  float4 RayleighColor  : TEXCOORD0;
  float4 MieColor       : TEXCOORD1;
  float3 Direction      : TEXCOORD2;
};



float scale(float cos)
{
	float x = 1.0 - cos;
	return fScaleDepth * exp(-0.00287 + x*(0.459 + x*(3.83 + x*(-6.80 + x*5.25))));
}


void CalculateScattering(float3 position, out float4 rayleighColor, out float4 mieColor, out float3 direction)
{
  // translate the vertex into planet space
	float3 v3Pos = position.xyz + PatchPosition;

	// Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
	float3 v3Ray = v3Pos - v3CameraPos;
	float fFar = length(v3Ray);
	v3Ray /= fFar;
	
	
	// Calculate the closest intersection of the ray with the outer atmosphere 
	//(which is the near point of the ray passing through the atmosphere)
	// float fNear = IntersectRaySphere(v3CameraPos, v3Pos);
	float B = 2.0 * dot(v3CameraPos, v3Ray);
	float C = fCameraHeight2 - fOuterRadius2;
	float fDet = max(0.0, B * B - 4.0 * C);
	float fNear = 0.5 * (-B - sqrt(fDet));
	
	// Calculate the ray's starting position, then calculate its scattering offset
	float3 v3Start = v3CameraPos + v3Ray * fNear;
	fFar -= fNear;
	float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fOuterRadius));
	float fStartAngle = dot(v3Ray, v3Start) / fOuterRadius;
	float fStartOffset = fDepth * scale(fStartAngle);
	
	
	// Initialize the scattering loop variables
	float fSampleLength = fFar / fSamples;
	float fScaledLength = fSampleLength * fScale;
	float3 v3SampleRay = v3Ray * fSampleLength;
	float3 v3SamplePoint = v3Start + v3SampleRay * 0.5;

	// loop through the sample rays
	float3 v3FrontColor = float3(0.0, 0.0, 0.0);
	for (int i = 0; i < nSamples; i++)
	{
		float fHeight = length(v3SamplePoint);
		float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fHeight));
		
		float fLightAngle = dot(v3LightPos, v3SamplePoint) / fHeight;
		float fCameraAngle = dot(v3Ray, v3SamplePoint) / fHeight;
		
		float fScatter = (fStartOffset + fDepth*(scale(fLightAngle) - scale(fCameraAngle)));
		float3 v3Attenuate = exp(-fScatter * (v3InvWavelength * fKr4PI + fKm4PI));
		
		v3FrontColor += v3Attenuate * (fDepth * fScaledLength);
		v3SamplePoint += v3SampleRay;
	}
	
	
	// Finally, scale the Mie and Rayleigh colors
	mieColor.xyz = v3FrontColor * fKmESun;
	mieColor.w = 1.0f;
	
    rayleighColor.xyz = v3FrontColor * (v3InvWavelength * fKrESun);
    rayleighColor.w = 1.0f;

    direction = v3CameraPos - v3Pos;
}





void AtmosphereBasicVertexShader(in InputVertex input, out OutputVertex output)
{
  output.Position = mul(input.Position, WorldViewProj);
  CalculateScattering(input.Position, output.RayleighColor, output.MieColor, output.Direction);
}



float4 AtmosphereBasicPixelShader(in OutputVertex input) : COLOR0
{
	float fCos = dot(v3LightPos, input.Direction) / length(input.Direction);
	float fCos2 = fCos * fCos;
	float fRayleighPhase = 0.75 * (1.0 + fCos2);
	float fMiePhase = 1.5 * ((1.0 - g2) / (2.0 + g2)) * (1.0 + fCos2) / pow(abs(1.0 + g2 - 2.0 * g * fCos), 1.5);
	
 	float4 f = fRayleighPhase * input.RayleighColor + fMiePhase * input.MieColor;

	f = 1 - exp(-fExposure * f);

	f.a = f.b;
	
	return f;
}



technique RenderAtmosphereBasic
{
	pass p0
	{	
		VertexShader = compile vs_3_0 AtmosphereBasicVertexShader();
		PixelShader = compile ps_3_0 AtmosphereBasicPixelShader();
	}	
}
