using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Willow;
using Willow.Noise;


namespace Planet
{
  public delegate double CreatePositionDelegate(ref Position3 spherePosition, ref Position3 cubePosition, ref Vector2 patchPosition, 
                                                double radius, out Position3 outPosition);
  public delegate TerrainNodeVertexBuffer CreateTerrainNodeVertexBufferDelegate(int count);


  public static class TerrainNodeDelegates
  {
    static GraphicsDevice graphicsDevice;
    static Pool<TerrainNodeVertexBuffer> terrainNodeVertexBufferPool;

    public static void InitializeTerrainNodeDelegates(GraphicsDevice graphicsDevice)
    {
      TerrainNodeDelegates.graphicsDevice = graphicsDevice;

      terrainNodeVertexBufferPool = new Pool<TerrainNodeVertexBuffer>(512, t => t.Active)
      {
        Initialize = t =>
        {
          // TODO : don't hard code border usage
          t.Initialize(graphicsDevice, (Constants.PatchWidth + 2) * (Constants.PatchHeight + 2));
        },

        Deinitialize = t =>
          {
          }
      };


      // pre-allocate vertex buffers
      object[] items = new object[512];

      for (int i = 0; i < 512; i++)
        items[i] = (object)terrainNodeVertexBufferPool.New();

      // release items
      for (int i = 0; i < 50; i++)
        ((TerrainNodeVertexBuffer)items[i]).Finished();


    }


    public static TerrainNodeVertexBuffer CreateTerrainNodeVertexBuffer(int count)
    {
      return terrainNodeVertexBufferPool.New();
    }



    public static double CreatePositionSphere(ref Position3 spherePosition, ref Position3 cubePosition, ref Vector2 patchPosition,
                                              double radius, out Position3 outPosition)
    {
      // the input position is a point on the surface of a unit sphere
      // by default we just want to move it out to the sphere radius
      // descendant classes will do more interesting things, like add some perlin noise
      // to it for terrain
      outPosition = spherePosition * radius;
      return 0;
    }


    public static double CreatePositionPlanet(ref Position3 spherePosition, ref Position3 cubePosition, ref Vector2 patchPosition,
                                              double radius, out Position3 outPosition)
    {
      Position3 p = cubePosition * 1.2;

      double h0 = Math.Abs(Noise.fBm(p.X, p.Y, p.Z, 3)) / 10.0;
      double h1 = Noise.Turbulence(p.X, p.Y, p.Z, 3, 2.0, 0.5);
      double h2 = Noise.HybridMultiFractal(p.X, p.Y, p.Z, 14, 1.854143, h0, 0.60);
      double h = h1 * h2;

      h *= 60;

      outPosition = spherePosition * (radius + h);
      return h;
    }

    //public static double CreatePositionPlanetGpu(ref Position3 spherePosition, ref Position3 cubePosition, ref Vector2 patchPosition,
    //                                             double radius, out Position3 outPosition)
    //{
    //  double h = terrainNodeGenerator.HeightData[(int)(patchPosition.Y * terrainNodeGenerator.PatchWidth + patchPosition.X)];
    //  outPosition = spherePosition * (radius + h);
    //  return h;
    //}

  }
}
