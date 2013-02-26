using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Willow.VertexDefinition;

namespace Planet
{
  public class TerrainNodeVertexBuffer
  {
    GraphicsDevice graphicsDevice;
    int vertexCount;
    VertexBuffer vertexBuffer;
    bool initialized;

    public VertexPositionNormalTextureHeight[] Vertices;    // patch vertex data
    public VertexBuffer VertexBuffer { get { return vertexBuffer; } }
    public int VertexCount { get { return vertexCount; } }
    public bool Active { get; set; }


    public TerrainNodeVertexBuffer()
    {
    }

    public void Initialize(GraphicsDevice graphicsDevice, int count)
    {
      Active = true;

      if (initialized) return;
      initialized = true;


      this.graphicsDevice = graphicsDevice;
      vertexCount = count;
      Vertices = new VertexPositionNormalTextureHeight[vertexCount];
    }

    public void SaveHeightMap(ref double[,] heightMap, int w, int h)
    {
      double highest = -999999999;

      // normalize heights
      for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
          if (heightMap[y, x] > highest)
            highest = heightMap[y, x];

      Color[] data = new Color[w * h];

      for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
          float c = (float)(heightMap[y, x] / highest);
          data[y * w + x] = new Color(c, c, c);
        }

#if !XBOX
      Texture2D tex = new Texture2D(graphicsDevice, w, h, false, SurfaceFormat.Color);
      tex.SetData<Color>(data);
      using (FileStream stream = new FileStream(String.Format(@"screenshots\heightmap_{0}.png", ++Globals.NormalMapNumber), FileMode.Create))
      {
        tex.SaveAsPng(stream, tex.Width, tex.Height);
      }
#endif
    }

    public void CommitChanges(int width, int height)
    {
      int count = width * height;

      VertexPositionNormalTextureHeight[] vertices = new VertexPositionNormalTextureHeight[count];

      // see if we have a border
      int left = 0;
      int top = 0;
      int columns = width;

      if (count != this.Vertices.Length)
      {
        left++;
        top++;
        columns += 2;
      }


      // transfer vertices from full, bordered vertex data
      int vertexIndex = 0;

      for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
          vertices[vertexIndex++] = this.Vertices[(top + y) * columns + (left + x)];

      vertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionNormalTextureHeight.VertexDeclaration, count, BufferUsage.WriteOnly);
      vertexBuffer.SetData(vertices);
    }


    public void Finished()
    {
      Active = false;
    }

    //private void Clear()
    //{
    //  if (vertexBuffer != null)
    //  {
    //    vertexBuffer.Dispose();
    //    vertexBuffer = null;
    //  }
    //}
  }
}
