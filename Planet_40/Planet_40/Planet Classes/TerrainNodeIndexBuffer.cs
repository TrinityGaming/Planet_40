using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

namespace Planet
{
  class TerrainNodeIndexBuffer
  {
    static short[] indexData;

    public static IndexBuffer Indices;
    public static int IndexCount;
    public static short[] IndexData { get { return indexData; } }

    public static void CreateIndices(GraphicsDevice graphicsDevice, int patchWidth, int patchHeight)
    {
      IndexCount = (patchWidth - 1) * (patchHeight - 1) * 6;

      indexData = new short[IndexCount];

      for (int y = 0; y < patchHeight - 1; y++)
        for (int x = 0; x < patchWidth - 1; x++)
        {
          int i = (y * (patchWidth - 1) + x) * 6;

          // lower left triangle
          indexData[i + 0] = (short)((y + 1) * patchWidth + (x + 0));  // top left vertex
          indexData[i + 1] = (short)((y + 1) * patchWidth + (x + 1));  // top right vertex
          indexData[i + 2] = (short)((y + 0) * patchWidth + (x + 0));  // lower left vertex

          // top right triangle
          indexData[i + 3] = (short)((y + 1) * patchWidth + (x + 1));  // top right vertex
          indexData[i + 4] = (short)((y + 0) * patchWidth + (x + 1));  // lower right vertex
          indexData[i + 5] = (short)((y + 0) * patchWidth + (x + 0));  // lower left vertex
        }

      Indices = new IndexBuffer(graphicsDevice, typeof(short), IndexCount, BufferUsage.WriteOnly);
      Indices.SetData<short>(indexData);
    }

  }
}
