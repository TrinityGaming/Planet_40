using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Planet
{
  public static class Constants
  {
    // settings
    public const bool FullScreen = false;

    public const int FullScreenWidth = 1024;    // 1440
    public const int FullScreenHeight = 768;   // 900
    public const int WindowedWidth = 800;
    public const int WindowedHeight = 600;

    public const bool UseFrustumCamera = true;
    public const bool DisableFrustumCulling = false;
    public const bool DisableHorizonCulling = false;
    public const bool DisableDiffuseTextureGeneration = false;
    public const bool DisableNormalMapGeneration = false;
    public const bool ForceHeightmapGeneration = false;

    public const int TerrainNodeTextureSize = 128;
    public const int TerrainNodeNormalMapSize = 128;

    public static double SunRadius = 709100.0;
    public static double EarthRadius = 6378.137f;
    public static double EarthAtmosphereRadius = EarthRadius * 1.025;



    public const int PatchWidth = 33;
    public const int PatchHeight = 33;
    public const int ErrorFactor = 32;             // generally 128
    public const int MaxTerrainNodeLevel = 16;
    public const int MaxAtmosphereNodeLevel = 4;

  }
}
