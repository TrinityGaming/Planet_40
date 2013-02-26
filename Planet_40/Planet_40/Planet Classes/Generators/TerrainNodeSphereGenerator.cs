using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Planet
{
  public class TerrainNodeSphereGenerator
  {
    Game game;
    GraphicsDevice device;
    Effect generateTerrainEffect;

    //Texture2D permutationTexture;
    //Texture2D gradientTexture;

    VertexDeclaration vertexPositionTexture;


    public TerrainNodeSphereGenerator(Game game)
    {
      this.game = game;
      this.device = game.GraphicsDevice;
    }


    public void LoadContent()
    {
      generateTerrainEffect = game.Content.Load<Effect>(@"effects\GenerateSphereNode");

      // texture declaration
      vertexPositionTexture = VertexPositionTexture.VertexDeclaration; //  new VertexDeclaration(device, VertexPositionTexture.VertexElements);
    }


    public void Execute(TerrainGeometrySurface surface, TerrainNodeBounds bounds)
    {
      surface.Begin();
      surface.Clear();

      // disable depth buffer and alpha blending
      device.DepthStencilState = DepthStencilState.None;
      device.BlendState = BlendState.Opaque;

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
