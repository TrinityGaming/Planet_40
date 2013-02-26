using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Planet
{
  public static class Globals
  {
    public static PlanetGame Game;

    public static float uOffset;
    public static float vOffset;

    public static long FrameNumber;
    public static int DrawLevel;
    public static int DrawCount;
    public static int FrustumCullCount;
    public static int HorizonCullCount;
    public static int NodeCount;

    public static int GeometryQueueDepth;
    public static int HeightmapQueueDepth;
    public static int TextureQueueDepth;
    public static int MeshQueueDepth;

    public static int NormalMapNumber = 0;

    public static int TriangleIndex;

  }
}
