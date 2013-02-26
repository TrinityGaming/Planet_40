#define gradient

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Planet
{
  public class TerrainTextureGenerator
  {
    Game game;
    GraphicsDevice device;
    Effect generateTextureEffect;
#if gradient
    Texture2D gradientTexture;
#else
    Texture2D slopemapTexture;
    Texture2D[] textures;
#endif

    VertexDeclaration vertexPositionTexture;

    public TerrainTextureGenerator(Game game)
    {
      this.game = game;
      this.device = game.GraphicsDevice;
    }


    public void LoadContent()
    {
#if gradient
      generateTextureEffect = game.Content.Load<Effect>(@"effects\GenerateTerrainTextureGradient");
      gradientTexture = game.Content.Load<Texture2D>(@"textures\gradient_01");

      generateTextureEffect.Parameters["gradient"].SetValue(gradientTexture);

#else
      string[] textureNames = { 
                                "dirt_01", "dirt_03",
                                "sand_02", "sand_03",
                                "grass_01", "grass_02", "grass_03",
                                "water_01", "stone_02", "stone_03",
                                "snow_01", "snow_03"
                              };

      generateTextureEffect = game.Content.Load<Effect>(@"effects\GenerateTerrainTexturePack");
      slopemapTexture = game.Content.Load<Texture2D>(@"textures\slopemap");

      // load diffuse textures
      textures = new Texture2D[textureNames.Length];
      for (int i = 0; i < textures.Length; i++)
        textures[i] = game.Content.Load<Texture2D>(@"textures\" + textureNames[i]);


#endif

      // texture declaration
      vertexPositionTexture = VertexPositionTexture.VertexDeclaration; // new VertexDeclaration(device, VertexPositionTexture.VertexElements);
    }


    public void Execute(TerrainHeightmapSurface heightmapSurface, TerrainNormalSurface normalmapSurface, TerrainTextureSurface textureSurface, float HeightScale)
    {
      textureSurface.Begin();
      textureSurface.Clear();

      //device.VertexDeclaration = vertexPositionTexture;

      // disable depth buffer and alpha blending
      device.DepthStencilState = DepthStencilState.None;
      device.BlendState = BlendState.Opaque;

      generateTextureEffect.Parameters["HeightMapTexture"].SetValue(heightmapSurface.Texture);

#if !gradient
      generateTextureEffect.Parameters["HeightScale"].SetValue(HeightScale);
      generateTextureEffect.Parameters["NormalMapTexture"].SetValue(normalmapSurface.Texture);

      generateTextureEffect.Parameters["SlopeMapTexture"].SetValue(slopemapTexture);
      generateTextureEffect.Parameters["DiffuseTexture0"].SetValue(textures[0]);
      generateTextureEffect.Parameters["DiffuseTexture1"].SetValue(textures[1]);
      generateTextureEffect.Parameters["DiffuseTexture2"].SetValue(textures[2]);
      generateTextureEffect.Parameters["DiffuseTexture3"].SetValue(textures[3]);
      generateTextureEffect.Parameters["DiffuseTexture4"].SetValue(textures[4]);
      generateTextureEffect.Parameters["DiffuseTexture5"].SetValue(textures[5]);
      generateTextureEffect.Parameters["DiffuseTexture6"].SetValue(textures[6]);
      generateTextureEffect.Parameters["DiffuseTexture7"].SetValue(textures[7]);
      generateTextureEffect.Parameters["DiffuseTexture8"].SetValue(textures[8]);
      generateTextureEffect.Parameters["DiffuseTexture9"].SetValue(textures[9]);
      generateTextureEffect.Parameters["DiffuseTextureA"].SetValue(textures[10]);
      generateTextureEffect.Parameters["DiffuseTextureB"].SetValue(textures[11]);

#endif

      //Vector2 halfPixel = new Vector2(0.5f / heightmapSurface.Width, 0.5f / heightmapSurface.Height);
      //generateTextureEffect.Parameters["HalfPixel"].SetValue(halfPixel);

      for (int i = 0; i < generateTextureEffect.CurrentTechnique.Passes.Count; i++)
      {
        EffectPass pass = generateTextureEffect.CurrentTechnique.Passes[i];
        pass.Apply();
        device.DrawUserPrimitives(PrimitiveType.TriangleStrip, textureSurface.Vertices, 0, 2);
      }

      textureSurface.End();
    }
  }
}
