float PatchLeft;
float PatchTop;
float PatchWidth;
float PatchHeight;
float3x3 FaceMatrix;


texture PermutationMap;
texture GradientMap;

sampler permSampler = sampler_state {
  texture = <PermutationMap>; 
  AddressU = Wrap;
  AddressV = Wrap;
  magfilter = POINT;
  minfilter = POINT; 
  mipfilter = NONE; 
};

sampler gradSampler = sampler_state
{
  texture = <GradientMap>;
  AddressU  = Wrap;
  AddressV  = Clamp;
  MAGFILTER = POINT;
  MINFILTER = POINT;
  MIPFILTER = NONE;
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




float3 fade(float3 t)
{
  return t * t * t * (t * (t * 6 - 15) + 10); // new curve
}

float4 perm(float2 p)
{
  return tex2D(permSampler, p);
}

float grad(float x, float3 p)
{
  return dot(tex1D(gradSampler, x), p);
}



float inoise(float3 p)
{
  float3 pos = fmod(floor(p), 256.0);	// FIND UNIT CUBE THAT CONTAINS POINT
  p -= floor(p);                             // FIND RELATIVE X,Y,Z OF POINT IN CUBE.
  float3 f = fade(p);                   // COMPUTE FADE CURVES FOR EACH OF X,Y,Z.
  
  pos /= 256.0;
  const float one = 1.0 / 256.0;
	
    // HASH COORDINATES OF THE 8 CUBE CORNERS
	float4 AA = perm(pos.xy) + pos.z;
 
	// AND ADD BLENDED RESULTS FROM 8 CORNERS OF CUBE
  float r = lerp( lerp( lerp( grad(AA.x, p ),  
                             grad(AA.z, p + float3(-1, 0, 0) ), f.x),
                       lerp( grad(AA.y, p + float3(0, -1, 0) ),
                             grad(AA.w, p + float3(-1, -1, 0) ), f.x), f.y),
                             
                 lerp( lerp( grad(AA.x+one, p + float3(0, 0, -1) ),
                             grad(AA.z+one, p + float3(-1, 0, -1) ), f.x),
                       lerp( grad(AA.y+one, p + float3(0, -1, -1) ),
                             grad(AA.w+one, p + float3(-1, -1, -1) ), f.x), f.y), f.z);


  // have to multiply this by 255 after converting to XNA 4.  Why???
  return r * 255.0;
}



float cloud(float3 p, float persistence, float frequency, float volume, int octaves)
{
  int i;
  float amplitude;
  float result;

  amplitude = 1;
  result = 0;

  [unroll]
  for (i = 1; i <= octaves; i++)
  {
    result += inoise(p * frequency) * amplitude;
    amplitude *= persistence;
    frequency *= 2;
  }

  result += volume;
  result = result * 0.5 + 0.5;  // return value between 0 and 1

  return clamp(result, 0, 1);
}  

 
      
// utility functions

// calculate gradient of noise (expensive!)
float3 inoiseGradient(float3 p, float d)
{
	float f0 = inoise(p);
	float fx = inoise(p + float3(d, 0, 0));	
	float fy = inoise(p + float3(0, d, 0));
	float fz = inoise(p + float3(0, 0, d));
	return float3(fx - f0, fy - f0, fz - f0) / d;
}

float fBm(float3 p, int octaves, float lacunarity = 2.0, float gain = 0.5)
{
	float freq = 1.0, amp = 0.5;
	float sum = 0;	
	for(int i=0; i<octaves; i++) {
		sum += inoise(p*freq)*amp;
		freq *= lacunarity;
		amp *= gain;
	}
	return sum;
}


float turbulence(float3 p, int octaves, float lacunarity = 2.0, float gain = 0.5)
{
	float sum = 0;
	float freq = 1.0, amp = 1.0;
	for(int i=0; i<octaves; i++) {
		sum += abs(inoise(p*freq))*amp;
		freq *= lacunarity;
		amp *= gain;
	}
	return sum;
}

// Ridged multifractal
// See "Texturing & Modeling, A Procedural Approach", Chapter 12
float ridge(float h, float offset)
{
    h = abs(h);
    h = offset - h;
    h = h * h;
    return h;
}

float ridgedmf(float3 p, int octaves, float lacunarity = 2.0, float gain = 0.5, float offset = 1.0)
{
	float sum = 0;
	float freq = 1.0, amp = 0.5;
	float prev = 1.0;
	for(int i=0; i<octaves; i++) {
		float n = ridge(inoise(p*freq), offset);
		sum += n*amp*prev;
		prev = n;
		freq *= lacunarity;
		amp *= gain;
	}
	return sum;
}
      


float fBm2(float3 p, int octaves, float lacunarity = 2.0, float gain = 0.5)
{
  float result = 0;
  float amplitude = 1.0;
  float amplitudeSum = 0.0;
  
  for (int i = 0; i < octaves; i++)
  {
    amplitudeSum += amplitude;

    result += amplitude * inoise(p);
    amplitude *= gain;
    
    p *= lacunarity;
  }

  // return result, scaled to -1 to 1
  return result / amplitudeSum;
}


float turbulence2(float3 p, int octaves, float lacunarity = 2.0, float gain = 0.5)
{
  float result = 0;
  float amplitude = 1.0;
  float amplitudeSum = 0.0;
  
  for (int i = 0; i < octaves; i++)
  {
    amplitudeSum += amplitude;

    result += amplitude * inoise(p);
    amplitude *= gain;
    p *= lacunarity;
  }

  // return result, scaled to -1 to 1
  return result / amplitudeSum;
}

      

float hybrid(float3 p, int octaves, float lacunarity, float h, float offset)
{
  float result = 0;
  float frequency = 1.0;
  float weight = 1.0;
  float signal;
  
  for (int i = 0; i < octaves; i++)
  {
    signal = pow(frequency, -h) * (inoise(p) + offset);
    result += weight * signal;
    
    frequency *= lacunarity;
    p *= lacunarity;
    
    weight = saturate(weight * signal);
  }
  
  return result;
}

      
      
float4 TerrainNoise(float3 position)
{
  /*
  position *= 1.2;
  float h0 = abs(fBm(position, 3)) * 0.1;
  float h1 = turbulence(position, 3, 2.0, 0.5);
  
  position *= 1.2;
  float h2 = hybrid(position, 16, 1.854143, 0.05, 0.80);
  float h = h1 * h2;
  h *= 60;
  */


  position *= 1.2;
  float h0 = abs(fBm(position, 3)) * 0.1;
  float h1 = turbulence(position, 3, 2.0, 0.5);
  
  position *= 1.2;
  float h2 = hybrid(position, 20, 1.854143, 0.05, 0.80);
  float h = h1 * h2;
  
  // this scales the height value up to increase roughness
  h *= 90;

  // move the entire surface down so the sphere radius is more or less sea level
  h -= 60;
  
  h = max(0, h);
  
  return float4(h, 0, 0, 1);
}


void TerrainVertexShader(in VertexInput input, out VertexOutput output)
{
  output.Position = input.Position;
  output.UV = input.UV;
}


float4 TerrainPixelShader(in VertexOutput input) : COLOR0
{
  float2 uv = input.UV;

  float x = PatchWidth * uv.x;
  float y = PatchHeight * uv.y;

  float3 position = float3(PatchLeft + x, PatchTop - y, 1);

  position = mul(position, FaceMatrix);
  position = normalize(position);        // !!!!!
  
  return TerrainNoise(position);
}


technique GenerateTerrain
{
  pass Pass1
  {
    VertexShader = compile vs_3_0 TerrainVertexShader();
    PixelShader = compile ps_3_0 TerrainPixelShader();
  }
}


