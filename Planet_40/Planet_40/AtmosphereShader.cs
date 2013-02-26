using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Planet
{
  public class AtmosphereShader
  {
    float[] waveLength4 = new float[3];
    float Kr4PI;
    float Km4PI;
    float scale;
    float scaleOverScaleDepth;

    float nSamples = 3;
    float fSamples = 3.0f;
    float rayleighScaleDepth = 0.25f;
    // float fMieScaleDepth = 0.1f;

    public float[] WaveLength = new float[3];
    public float InnerRadius;
    public float OuterRadius;
    public float Exposure;
    public float Kr;
    public float Km;
    public float Esun;
    public float g;



    public AtmosphereShader()
    {
      Initialize();
    }

    public void Initialize()
    {
      SetDefaultParameters();
    }

    public void SetDefaultParameters()
    {
      WaveLength[0] = 0.650f;		// 650 nm for red
      WaveLength[1] = 0.570f;		// 570 nm for green
      WaveLength[2] = 0.475f;		// 475 nm for blue
      Exposure = 2.0f;
      Kr = 0.0025f;
      Km = 0.0015f;
      Esun = 35.0f;
      g = -0.994f; // -0.95f;

      InnerRadius = (float)Constants.EarthRadius;
      OuterRadius = (float)Constants.EarthAtmosphereRadius;

      CalculateParameters();
    }

    public void CalculateParameters()
    {
      waveLength4[0] = (float)Math.Pow(WaveLength[0], 4.0f);
      waveLength4[1] = (float)Math.Pow(WaveLength[1], 4.0f);
      waveLength4[2] = (float)Math.Pow(WaveLength[2], 4.0f);

      Kr4PI = Kr * 4.0f * (float)Math.PI;
      Km4PI = Km * 4.0f * (float)Math.PI;

      scale = 1.0f / (OuterRadius - InnerRadius);
      scaleOverScaleDepth = scale / rayleighScaleDepth;
    }

    public void ResolveParameters(Effect effect)
    {
      /*
      fEffect.Parameters["WorldMatrix"].SetValue(World);
      fEffect.Parameters["WorldViewProj"].SetValue(W * C.ViewMatrix * C.ProjectionMatrix);
      fEffect.Parameters["v3CameraPos"].SetValue(fCameraPosition);
      fEffect.Parameters["v3LightPos"].SetValue(vLightDirection);
      fEffect.Parameters["fCameraHeight"].SetValue(fCameraHeight);
      fEffect.Parameters["fCameraHeight2"].SetValue(fCameraHeight * fCameraHeight);
      */

      effect.Parameters["v3InvWavelength"].SetValue(new Vector3(1.0f / waveLength4[0], 1.0f / waveLength4[1], 1.0f / waveLength4[2]));
      effect.Parameters["fInnerRadius"].SetValue(InnerRadius);
      effect.Parameters["fInnerRadius2"].SetValue(InnerRadius * InnerRadius);
      effect.Parameters["fOuterRadius"].SetValue(OuterRadius);
      effect.Parameters["fOuterRadius2"].SetValue(OuterRadius * OuterRadius);
      effect.Parameters["fKrESun"].SetValue(Kr * Esun);
      effect.Parameters["fKmESun"].SetValue(Km * Esun);
      effect.Parameters["fKr4PI"].SetValue(Kr4PI);
      effect.Parameters["fKm4PI"].SetValue(Km4PI);
      effect.Parameters["g"].SetValue(g);
      effect.Parameters["g2"].SetValue(g * g);
      effect.Parameters["fScale"].SetValue(scale);
      effect.Parameters["fScaleDepth"].SetValue(rayleighScaleDepth);
      effect.Parameters["fScaleOverScaleDepth"].SetValue(scaleOverScaleDepth);
      effect.Parameters["nSamples"].SetValue(nSamples);
      effect.Parameters["fSamples"].SetValue(fSamples);
      effect.Parameters["fExposure"].SetValue(Exposure);
    }
  }
}
