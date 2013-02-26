using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Planet
{
  public class TerrainHeightmapSurface
  {
    GraphicsDevice device;
    RenderTarget2D renderTarget;
    Texture2D texture;
    VertexPositionTexture[] vertices;

    bool initialized;
    bool includeBorder;
    int totalWidth;
    int totalHeight;
    int width;
    int height;

    public bool Active { get; set; }
    public float[] HeightData;
    public int PatchWidth { get { return totalWidth; } }
    public int PatchHeight { get { return totalHeight; } }
    public int Width { get { return width; } }
    public int Height { get { return height; } }
    public Texture2D Texture { get { return texture; } }
    public VertexPositionTexture[] Vertices { get { return vertices; } }
    public bool IncludeBorder { get { return includeBorder; } }

    public TerrainHeightmapSurface()
    {
    }

    public void Initialize(GraphicsDevice device, int width, int height, bool includeBorder)
    {
      Active = true;

      if (initialized) return;
      initialized = true;

      this.includeBorder = includeBorder;
      this.device = device;
      this.width = width;
      this.height = height;

      totalWidth = width;
      totalHeight = height;

      if (includeBorder)
      {
        totalWidth += 2;
        totalHeight += 2;
      }

      renderTarget = new RenderTarget2D(device, totalWidth, totalHeight, false, SurfaceFormat.Single, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
      HeightData = new float[width * height];


      // create quad for executing the shader - vertices are defined in screen space
      // we need to modify the texture coordinates so they're interpolated 
      // across the quad in such a way to give us the proper cube space coordinates that
      // the shader uses to look up noise
      float ps = 1.0f / (width - 1);

      vertices = new VertexPositionTexture[4];
      vertices[0] = new VertexPositionTexture(new Vector3(-1, 1, 0f), new Vector2(0, 1));
      vertices[1] = new VertexPositionTexture(new Vector3(1, 1, 0f), new Vector2(1 + ps, 1));
      vertices[2] = new VertexPositionTexture(new Vector3(-1, -1, 0f), new Vector2(0, 0 - ps));
      vertices[3] = new VertexPositionTexture(new Vector3(1, -1, 0f), new Vector2(1 + ps, 0 - ps));


      // if we have a border then expand the texture coordinates out to account for it
      if (includeBorder)
      {
        vertices[0].TextureCoordinate.X -= ps;
        vertices[0].TextureCoordinate.Y += ps;

        vertices[1].TextureCoordinate.X += ps;
        vertices[1].TextureCoordinate.Y += ps;

        vertices[2].TextureCoordinate.X -= ps;
        vertices[2].TextureCoordinate.Y -= ps;

        vertices[3].TextureCoordinate.X += ps;
        vertices[3].TextureCoordinate.Y -= ps;
      }
    }


    public void Finished()
    {
      Active = false;
    }


    public void Clear()
    {
      device.Clear(ClearOptions.Target, Color.Black, 1, 0);
    }



    public void Begin()
    {
      // activate render target
      device.SetRenderTarget(renderTarget);
    }


    public void End()
    {
      device.SetRenderTarget(null);
      texture = renderTarget; // renderTarget.GetTexture();
    }


    public void ResolveHeightData()
    {
      texture.GetData<float>(HeightData);
    }

    public void GetHeightRange(out float lowest, out float highest)
    {
      lowest = 999999;
      highest = -9999999;

      // identify height range
      for (int i = 0; i < width * height; i++)
      {
        if (HeightData[i] > highest) highest = HeightData[i];
        if (HeightData[i] < lowest) lowest = HeightData[i];
      }
    }


    public void Save(TerrainNodeSplitItem item)
    {
      float lowest = 999999;
      float highest = -9999999;

      // normalize heights
      for (int i = 0; i < width * height; i++)
      {
        if (HeightData[i] > highest) highest = HeightData[i];
        if (HeightData[i] < lowest) lowest = HeightData[i];
      }

      float range = highest - lowest;

      highest = 100;

      Color[] data = new Color[width * height];

      for (int i = 0; i < width * height; i++)
      {
        // float c = (heightMap[i] - lowest) / range;     // map lowest value to 0
        float c = HeightData[i] / highest;                 // negative values will show as black, below sea level
        data[i] = new Color(c, c, c);
      }

//#if !XBOX
//      using (FileStream stream = new FileStream(String.Format(@"screenshots\height_{0}_{1}.png", Globals.NormalMapNumber++, item.ChildIndex), FileMode.Create))
//      {
//        Texture2D tex = new Texture2D(device, width, height, false, SurfaceFormat.Color);
//        tex.SetData<Color>(data);
//        tex.SaveAsPng(stream, tex.Width, tex.Height);
//      }
//#endif
    }
  }
}
