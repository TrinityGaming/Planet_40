using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Planet
{
  public class TerrainNormalSurface
  {
    GraphicsDevice device;
    RenderTarget2D renderTarget;
    Texture2D texture;
    VertexPositionTexture[] vertices;
    bool initialized;

    public bool Active { get; set; }
    public int PatchWidth { get { return renderTarget.Width; } }
    public int PatchHeight { get { return renderTarget.Height; } }
    public VertexPositionTexture[] Vertices { get { return vertices; } }
    public Texture2D Texture { get { return texture; } }

    //Color[] normalData;


    public TerrainNormalSurface()
    {
    }


    public void Initialize(GraphicsDevice device, int width, int height)
    {
      Active = true;

      if (initialized) return;
      initialized = true;


      this.device = device;
      renderTarget = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);


      // create quad for executing the shader - vertices are defined in screen space
      // the texture coordinates here are used to look up height values in the height map, which
      // has a border around it so the edge normals can be calculated correctly
      // we need to define the texture coordinates so they are looking at only the pixels inside the border
      float ps = 1.0f / (width - 1);

      vertices = new VertexPositionTexture[4];
      vertices[0] = new VertexPositionTexture(new Vector3(-1, 1, 0f), new Vector2(0, 1));
      vertices[1] = new VertexPositionTexture(new Vector3(1, 1, 0f), new Vector2(1, 1));
      vertices[2] = new VertexPositionTexture(new Vector3(-1, -1, 0f), new Vector2(0, 0));
      vertices[3] = new VertexPositionTexture(new Vector3(1, -1, 0f), new Vector2(1, 0));


      // adjust coordinates to point to the pixels inside the border

      // top left
      vertices[0].TextureCoordinate.X += ps;
      vertices[0].TextureCoordinate.Y -= ps;

      // top right
      vertices[1].TextureCoordinate.X -= ps;
      vertices[1].TextureCoordinate.Y -= ps;

      //// lower left
      vertices[2].TextureCoordinate.X += ps;
      vertices[2].TextureCoordinate.Y += ps;
      
      //// lower right
      vertices[3].TextureCoordinate.X -= ps;
      vertices[3].TextureCoordinate.Y += ps;

    }


    public void Finished()
    {
      Active = false;
    }

    public void Clear()
    {
      device.Clear(ClearOptions.Target /*| ClearOptions.DepthBuffer*/, Color.Black, 1, 0);
    }



    public void Begin()
    {
      device.SetRenderTarget(renderTarget);
    }


    public void End()
    {
      device.SetRenderTarget(null);
      texture = renderTarget;
    }


    public void Save()
    {
//#if !XBOX
//      using (FileStream stream = new FileStream(String.Format(@"screenshots\terrain_texture_{0}.png", Globals.NormalMapNumber++), FileMode.Create))
//      {
//        texture.SaveAsPng(stream, texture.Width, texture.Height);
//      }
//#endif
    }
  }


}
