float4x4 WorldViewProj;
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
float fPlanetRadius;

float fExposure;	// Exposure parameter for pixel shader

int nSamples;
float fSamples;

// Application to vertex structure
struct a2v
{
	float4 Position : POSITION0;
};

// Vertex to pixel shader structure
struct v2p
{
	float4 Position      : POSITION0;
	float3 Direction     : TEXCOORD0;
	float4 RayleighColor : TEXCOORD1;
	float4 MieColor      : TEXCOORD2;
};


float scale(float fCos)
{
	float x = 1.0 - fCos;
	return fScaleDepth * exp(-0.00287 + x*(0.459 + x*(3.83 + x*(-6.80 + x*5.25))));
}


void RenderSkyVS(in a2v IN, out v2p OUT)
{
  // translate the vertex into planet space
	float3 v3Pos = IN.Position + PatchPosition;

	// Get the ray from the camera to the vertex, and it's length (far point)
	float3 v3Ray = v3Pos - v3CameraPos;
	float fFar = length(v3Ray);
	v3Ray /= fFar;
	
	// Calculate the ray's starting position, then calculate its scattering offset
	float3 v3Start = v3CameraPos;
	float fHeight = length(v3Start);
	float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fCameraHeight));
	float fStartAngle = dot(v3Ray, v3Start) / fHeight;
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
	OUT.Position = mul(IN.Position, WorldViewProj);
	OUT.MieColor.xyz = v3FrontColor * fKmESun;
	
	OUT.MieColor.w = 1.0f;
  OUT.RayleighColor.xyz = v3FrontColor * (v3InvWavelength * fKrESun);
	OUT.RayleighColor.w = 1.0f;
	
	OUT.Direction = v3CameraPos - v3Pos;
}


float4 RenderSkyPS(in v2p IN) : COLOR0
{
	float fCos = dot(v3LightPos, IN.Direction) / length(IN.Direction);
	float fRayleighPhase = 0.75 * (1.0 + fCos*fCos);
	
  //float fRayleighPhase = 0.75 * (2.0 + 0.5 * fCos * fCos);
	
	float fMiePhase = 1.5 * ((1.0 - g2) / (2.0 + g2)) * (1.0 + fCos*fCos) / pow(abs(1.0 + g2 - 2.0 * g * fCos), 1.5);
	float4 C = fRayleighPhase * IN.RayleighColor + fMiePhase * IN.MieColor;
	
	C = 1 - exp(-fExposure * C);
	
	C.a = C.b;
	
	return C;
}


technique RenderSky
{
	pass p0
	{	
		VertexShader = compile vs_3_0 RenderSkyVS();
		PixelShader = compile ps_3_0 RenderSkyPS();
		// ZWriteEnable = 0;
	}	
}



