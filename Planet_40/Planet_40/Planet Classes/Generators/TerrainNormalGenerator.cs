using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Planet
{
  public class TerrainNormalGenerator
  {
    Game game;
    GraphicsDevice device;
    Effect generateNormalEffect;
    VertexDeclaration vertexPositionTexture;


    public TerrainNormalGenerator(Game game)
    {
      this.game = game;
      this.device = game.GraphicsDevice;
    }


    public void LoadContent()
    {
      generateNormalEffect = game.Content.Load<Effect>(@"effects\GenerateNormalTexture");

      // texture declaration
      vertexPositionTexture = VertexPositionTexture.VertexDeclaration; // new VertexDeclaration(device, VertexPositionTexture.VertexElements);
    }


    /// <summary>
    /// Create high resolution normal map
    /// </summary>
    /// <param name="heightmapSurface">High resolution height map</param>
    /// <param name="normalSurface">Render target surface for outputting the normal map</param>
    public void Execute(TerrainHeightmapSurface heightmapSurface, TerrainNormalSurface normalSurface, int nodeLevel)
    {
      normalSurface.Begin();
      normalSurface.Clear();

      // disable depth buffer and alpha blending
      device.DepthStencilState = DepthStencilState.None;
      device.BlendState = BlendState.Opaque;

      float normalScale = (float)Math.Pow(2.0, -(Constants.MaxTerrainNodeLevel - nodeLevel));
      normalScale *= 1000;

      generateNormalEffect.Parameters["NormalScale"].SetValue(normalScale);
      generateNormalEffect.Parameters["PatchWidth"].SetValue(heightmapSurface.Width);
      generateNormalEffect.Parameters["PatchHeight"].SetValue(heightmapSurface.Height);
      generateNormalEffect.Parameters["heightmap"].SetValue(heightmapSurface.Texture);


      for (int i = 0; i < generateNormalEffect.CurrentTechnique.Passes.Count; i++)
      {
        EffectPass pass = generateNormalEffect.CurrentTechnique.Passes[i];

        pass.Apply();
        device.DrawUserPrimitives(PrimitiveType.TriangleStrip, normalSurface.Vertices, 0, 2, vertexPositionTexture);
      }

      normalSurface.End();
    }
  }
}
