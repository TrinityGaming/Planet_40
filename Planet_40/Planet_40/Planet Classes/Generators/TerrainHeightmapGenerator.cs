using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Planet
{
  public class TerrainHeightmapGenerator
  {
    Game game;
    GraphicsDevice device;
    Effect generateTerrainEffect;

    Texture2D permutationTexture;
    Texture2D gradientTexture;

    VertexDeclaration vertexPositionTexture;


    public TerrainHeightmapGenerator(Game game)
    {
      this.game = game;
      this.device = game.GraphicsDevice;
    }


    public void LoadContent()
    {
      generateTerrainEffect = game.Content.Load<Effect>(@"effects\GenerateTerrainNode");
      CreateGradientMap();
      CreatePermutationMap();


      // texture declaration
      vertexPositionTexture = VertexPositionTexture.VertexDeclaration; //  new VertexDeclaration(device, VertexPositionTexture.VertexElements);
    }

    private void CreateGradientMap()
    {
      // gradients for 3D noise
      Color[] gradients = {
                          new Color(1, 1, 0),
                          new Color(-1, 1, 0),
                          new Color(1, -1, 0),
                          new Color(-1, -1, 0),
                          new Color(1, 0, 1),
                          new Color(-1, 0, 1),
                          new Color(1, 0, -1),
                          new Color(-1, 0, -1),
                          new Color(0, 1, 1),
                          new Color(0, -1, 1),
                          new Color(0, 1, -1),
                          new Color(0, -1, -1),
                          new Color(1, 1, 0),
                          new Color(0, -1, 1),
                          new Color(-1, 1, 0),
                          new Color(0, -1, -1)
                        };


      // permutation table
      int[] permutation = { 
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

      Color[] colors = new Color[256];

      int i = 0;

      for (int x = 0; x < 256; x++)
      {
        int A = permutation[x] % 16;
        colors[i] = gradients[A];
        i++;
      }

      gradientTexture = new Texture2D(game.GraphicsDevice, 256, 1, false, SurfaceFormat.Color);
      gradientTexture.SetData(colors);
    }

    private void CreatePermutationMap()
    {
      // permutation table
      int[] permutation = { 
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

      Color[] colors = new Color[permutation.Length * permutation.Length];

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

          colors[i] = new Color((float)AA / 256.0f, (float)AB / 256.0f, (float)BA / 256.0f, (float)BB / 256.0f);
          i++;
        }

      permutationTexture = new Texture2D(game.GraphicsDevice, 256, 256, false, SurfaceFormat.Color);
      permutationTexture.SetData(colors);
    }


    public void Execute(TerrainHeightmapSurface surface, TerrainNodeBounds bounds)
    {
      surface.Begin();
      surface.Clear();

      //device.VertexDeclaration = vertexPositionTexture;

      // disable depth buffer and alpha blending
      device.DepthStencilState = DepthStencilState.None;
      device.BlendState = BlendState.Opaque;

      generateTerrainEffect.Parameters["PermutationMap"].SetValue(permutationTexture);
      generateTerrainEffect.Parameters["GradientMap"].SetValue(gradientTexture);
      generateTerrainEffect.Parameters["PatchLeft"].SetValue((float)bounds.Left);
      generateTerrainEffect.Parameters["PatchTop"].SetValue((float)bounds.Top);
      generateTerrainEffect.Parameters["PatchWidth"].SetValue((float)bounds.Width);
      generateTerrainEffect.Parameters["PatchHeight"].SetValue((float)bounds.Height);
      generateTerrainEffect.Parameters["FaceMatrix"].SetValue(CubeFaces.GetFace(bounds.Face).FaceMatrix);


      for (int i = 0; i < generateTerrainEffect.CurrentTechnique.Passes.Count; i++)
      {
        EffectPass pass = generateTerrainEffect.CurrentTechnique.Passes[i];
        pass.Apply();
        device.DrawUserPrimitives(PrimitiveType.TriangleStrip, surface.Vertices, 0, 2);
      }

      surface.End();
    }
  }
}
