using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Willow.Noise
{
  public static class Noise
  {
    // gradients for 3D noise
    static Vector3[] gradients = {
                          new Vector3(1, 1, 0),
                          new Vector3(-1, 1, 0),
                          new Vector3(1, -1, 0),
                          new Vector3(-1, -1, 0),
                          new Vector3(1, 0, 1),
                          new Vector3(-1, 0, 1),
                          new Vector3(1, 0, -1),
                          new Vector3(-1, 0, -1),
                          new Vector3(0, 1, 1),
                          new Vector3(0, -1, 1),
                          new Vector3(0, 1, -1),
                          new Vector3(0, -1, -1),
                          new Vector3(1, 1, 0),
                          new Vector3(0, -1, 1),
                          new Vector3(-1, 1, 0),
                          new Vector3(0, -1, -1)
                        };

    // permutation table
    static int[] permutation = { 
         151,160,137,91,90,15,
         131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
         190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
         88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
         77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
         102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
         135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
         5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
         223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
         129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
         251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
         49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
         138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
      };


    private static Vector4[] perms;
    private static Vector3[] grads;

    static Noise()
    {
      CreateGradientMap();
      CreatePermutationMap();
    }

    private static void CreateGradientMap()
    {
      grads = new Vector3[256];
      int i = 0;

      for (int x = 0; x < 256; x++)
      {
        int A = permutation[x] % 16;
        grads[i] = gradients[A];
        i++;
      }
    }


    private static void CreatePermutationMap()
    {
      perms = new Vector4[permutation.Length * permutation.Length];

      int i = 0;
      for (int y = 0; y < 256; y++)
        for (int x = 0; x < 256; x++)
        {
          int A = permutation[x % 256] + y;
          int AA = permutation[A % 256];
          int AB = permutation[(A + 1) % 256];
          int B = permutation[(x + 1) % 256] + y;
          int BA = permutation[B % 256];
          int BB = permutation[(B + 1) % 256];

          perms[i] = new Vector4((float)AA, (float)AB, (float)BA, (float)BB);
          i++;
        }
    }


    private static float fade(float t)
    {
      return t * t * t * (t * (t * 6 - 15) + 10); // new curve
    }

    private static Vector3 fade(Vector3 t)
    {
      return new Vector3(fade(t.X), fade(t.Y), fade(t.Z));
    }

    private static Vector4 perm(Vector2 p)
    {
      int i = ((int)p.Y & 255) * 256 + ((int)p.X & 255);
      return perms[i];
    }

    private static float grad(float x, Vector3 p)
    {
      return Vector3.Dot(grads[(int)x & 255], p);
    }

    private static Vector3 floor(Vector3 p)
    {
      return new Vector3((float)Math.Floor(p.X), (float)Math.Floor(p.Y), (float)Math.Floor(p.Z));
    }

    private static Vector3 fmod(Vector3 p, float value)
    {
      return new Vector3(p.X % value, p.Y % value, p.Z % value);
    }

    private static float lerp(float t, float a, float b)
    {
      return a + t * (b - a);
    }


    public static float inoise(Vector3 p)
    {
      Vector3 P = fmod(floor(p), 256.0f);
      p -= floor(p);
      Vector3 f = fade(p);

      // get hash coordinates of the 8 cube corners
      Vector4 AA = perm(new Vector2(P.X, P.Y));
      AA.X += P.Z;   // = AA
      AA.Y += P.Z;   // = AB
      AA.Z += P.Z;   // = BA
      AA.W += P.Z;   // = BB


      return lerp(f.Z, lerp(f.Y, lerp(f.X, grad(AA.X, p),
                                           grad(AA.Z, p + new Vector3(-1, 0, 0))),
                                 lerp(f.X, grad(AA.Y, p + new Vector3(0, -1, 0)),
                                           grad(AA.W, p + new Vector3(-1, -1, 0)))),

                       lerp(f.Y, lerp(f.X, grad(AA.X + 1, p + new Vector3(0, 0, -1)),
                                           grad(AA.Z + 1, p + new Vector3(-1, 0, -1))),
                                 lerp(f.X, grad(AA.Y + 1, p + new Vector3(0, -1, -1)),
                                           grad(AA.W + 1, p + new Vector3(-1, -1, -1)))));


    }




    public static double GetNoise(double x, double y, double z)
    {
      return inoise(new Position3(x, y, z).AsVector3);
    }

    private static double GetNoiseAbs(double x, double y, double z)
    {
      return Math.Abs(GetNoise(x, y, z));
    }


    public static double fBm(double x, double y, double z, double octaves)
    {
      return fBm(x, y, z, octaves, 2.0, 0.5);
    }

    public static double fBm(double x, double y, double z, double octaves, double lacunarity, double gain)
    {
      double result = 0;
      double amplitude = 1.0;
      double amplitudeSum = 0.0;

      for (int i = 0; i < octaves; i++)
      {
        amplitudeSum += amplitude;

        result += amplitude * GetNoise(x, y, z);
        amplitude *= gain;

        x *= lacunarity;
        y *= lacunarity;
        z *= lacunarity;

      }

      // return result, scaled to -1 to 1
      return result / amplitudeSum;
    }


    public static double Turbulence(double x, double y, double z, double octaves, double lacunarity, double gain)
    {
      double result = 0;
      double amplitude = 1.0;
      double amplitudeSum = 0.0;

      for (int i = 0; i < octaves; i++)
      {
        amplitudeSum += amplitude;

        result += amplitude * GetNoiseAbs(x, y, z);
        amplitude *= gain;

        x *= lacunarity;
        y *= lacunarity;
        z *= lacunarity;

      }

      // return result, scaled to -1 to 1
      return result / amplitudeSum;
    }



    public static double HeterogeneousTerrain(double x, double y, double z, double octaves, double lacunarity, double h, double offset)
    {
      double result = 0;
      double frequency = 1.0;

      for (int i = 0; i < octaves; i++)
      {
        result += Math.Pow(frequency, -h) * (GetNoise(x, y, z) + offset);

        // scale increment by current "altitude" of function
        // increment *= result;

        frequency *= lacunarity;
        x *= lacunarity;
        y *= lacunarity;
        z *= lacunarity;

      }

      return result;
    }

    public static double RidgedMultiFractal(double x, double y, double z, double octaves, double lacunarity, double gain, double h, double offset)
    {
      double result = 0.0;
      double frequency = 1.0;
      double weight = 1.0;
      double signal;


      for (int i = 0; i < octaves; i++)
      {
        signal = GetNoise(x, y, z);

        // get absolute value of signal - this creates the ridges
        if (signal < 0.0) signal = -signal;

        // invert and translate - offset should be close to 1.0
        signal = offset - signal;

        // square the signal to increase sharpness of ridges
        signal *= signal;

        // weight successive contributions by previous signal
        signal *= weight;

        // add to result
        result += Math.Pow(frequency, -h) * signal;


        // update values
        frequency *= lacunarity;

        x *= lacunarity;
        y *= lacunarity;
        z *= lacunarity;

        weight = signal * gain;
        if (weight > 1.0) weight = 1.0;
        if (weight < 0.0) weight = 0.0;
      }

      return result;
    }


    public static double HybridMultiFractal(double x, double y, double z, double octaves, double lacunarity, double h, double offset)
    {
      double result = 0.0;
      double frequency = 1.0;
      double weight = 1.0;
      double signal;


      for (int i = 0; i < octaves; i++)
      {
        signal = Math.Pow(frequency, -h) * (GetNoise(x, y, z) + offset);
        result += weight * signal;

        frequency *= lacunarity;
        x *= lacunarity;
        y *= lacunarity;
        z *= lacunarity;

        weight *= signal;
        if (weight > 1.0) weight = 1.0;
      }

      return result;
    }


    public static double PerlinNoiseRidged(double x, double y, double z, double octaves, double lambda, double amplitude, double g)
    {
      double result = 0.0;

      for (int i = 1; i <= octaves; i++)
      {
        double p = Math.Pow(2, i);
        double a = amplitude / p;
        double l = lambda / p;

        double r = GetNoise(x / l, y / l, z / l);
        if (r > 0)
          r = -r + 1;
        else
          r++;

        r *= a;

        if (i > 1)
          r *= g * result;

        result += r;
      }

      return result;
    }

  }
}
