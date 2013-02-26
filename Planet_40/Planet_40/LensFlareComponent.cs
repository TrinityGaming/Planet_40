using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Willow;
using Willow.Camera;


 // TODO : PointList no longer exists in XNA 4!
  
 // this was relying on drawing a PointList to calculate an intensity map to look for occlusion and fade out the lens flair, unfortunately
 //   the pointlist renedering mode was removed in XNA 4, and it would require some effort to re-engineer this, so it's left as 
 //   exercise for the reader ;)



namespace Planet
{
    /// <summary>
    /// Reusable component for drawing a lensflare effect over the top of a 3D scene.
    /// </summary>
    public class LensFlareComponent
    {
        // How big is the circular glow effect?
        const float glowSize = 600;

        // How big a rectangle should we examine when issuing our occlusion queries?
        // Increasing this makes the flares fade out more gradually when the sun goes
        // behind scenery, while smaller query areas cause sudden on/off transitions.
        const float querySize = 100;



        // These are set by the main game to tell us the position of the camera and sun.
        private ICameraManager fCameraManager;
        public Position3 LightPosition;
        private Vector3 LightDirection;


        // Graphics objects.
        Texture2D glowSprite;
        SpriteBatch spriteBatch;
        BasicEffect basicEffect;
        VertexDeclaration vertexDeclaration;
        VertexPositionColor[] queryVertices;
        Effect fMaskCombine;
        public Texture2D fIntensityMask;
        VertexBuffer fOcclusionPoints;
        BlendState BlendStateAddOne;
        DepthStencilState DepthTest;
        BlendState NoColors;

      


        // An occlusion query is used to detect when the sun is hidden behind scenery.
        OcclusionQuery occlusionQuery;
        bool occlusionQueryActive;
        public float occlusionAlpha = 1.0f;

        public bool Display = true;
        public bool MaskMode = true;
        public bool fDrawFlare = true;
        private bool fIsValid = false;

        private int MaskSize = 32;

        Camera fSunCamera;
        RenderTarget2D fSunView;
        RenderTarget2D fIntensityMap;
        Effect fIntensityEffect;
        VertexDeclaration fIntensityMaskVertexDeclaration;
        Vector2 LightScreenPosition;
  
      Game game;

        // The lensflare effect is made up from several individual flare graphics,
        // which move across the screen depending on the position of the sun. This
        // helper class keeps track of the position, size, and color for each flare.
        class Flare
        {
            public Flare(float position, float scale, Color color, string textureName)
            {
                Position = position;
                Scale = scale;
                Color = color;
                TextureName = textureName;
            }

            public float Position;
            public float Scale;
            public Color Color;
            public string TextureName;
            public Texture2D Texture;
        }


        // Array describes the position, size, color, and texture for each individual
        // flare graphic. The position value lies on a line between the sun and the
        // center of the screen. Zero places a flare directly over the top of the sun,
        // one is exactly in the middle of the screen, fractional positions lie in
        // between these two points, while negative values or positions greater than
        // one will move the flares outward toward the edge of the screen. Changing
        // the number of flares, or tweaking their positions and colors, can produce
        // a wide range of different lensflare effects without altering any other code.
        Flare[] flares =
        {
            new Flare(-0.5f, 0.7f, new Color( 50,  25,  50), "flare1"),
            new Flare( 0.3f, 0.4f, new Color(100, 255, 200), "flare1"),
            new Flare( 1.2f, 1.0f, new Color(100,  50,  50), "flare1"),
            new Flare( 1.5f, 1.5f, new Color( 50, 100,  50), "flare1"),

            new Flare(-0.3f, 0.7f, new Color(200,  50,  50), "flare2"),
            new Flare( 0.6f, 0.9f, new Color( 50, 100,  50), "flare2"),
            new Flare( 0.7f, 0.4f, new Color( 50, 200, 200), "flare2"),

            new Flare(-0.7f, 0.7f, new Color( 50, 100,  25), "flare3"),
            new Flare( 0.0f, 0.6f, new Color( 25,  25,  25), "flare3"),
            new Flare( 2.0f, 1.4f, new Color( 25,  50, 100), "flare3"),
        };



        /// <summary>
        /// Constructs a new lensflare component.
        /// </summary>
        public LensFlareComponent()
        {

          BlendStateAddOne = new BlendState
          {
            AlphaBlendFunction = BlendFunction.Add,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.One
          };


          DepthTest = new DepthStencilState
          {
            DepthBufferEnable = true,
            DepthBufferWriteEnable = false
          };

          NoColors = new BlendState
          {
            ColorWriteChannels = ColorWriteChannels.None
          };
        }


        /// <summary>
        /// Loads the content used by the lensflare component.
        /// </summary>
        public void LoadContent(Game game)
        {
          this.game = game;

          fCameraManager = (ICameraManager)game.Services.GetService(typeof(ICameraManager));

            // Create a SpriteBatch for drawing the glow and flare sprites.
            spriteBatch = new SpriteBatch(game.GraphicsDevice);

            // Load the glow and flare textures.
            glowSprite = game.Content.Load<Texture2D>(@"textures\glow");

            foreach (Flare flare in flares)
            {
              flare.Texture = game.Content.Load<Texture2D>(@"textures\" + flare.TextureName);
            }

            // Effect and vertex declaration for drawing occlusion query polygons.
            basicEffect = new BasicEffect(game.GraphicsDevice);
            
            basicEffect.View = Matrix.Identity;
            basicEffect.VertexColorEnabled = true;

            fMaskCombine = game.Content.Load<Effect>(@"effects\maskcombine");

            vertexDeclaration = VertexPositionColor.VertexDeclaration; // new VertexDeclaration(game.GraphicsDevice, VertexPositionColor.VertexElements);

            // Create vertex data for the occlusion query polygons.
            queryVertices = new VertexPositionColor[4];

            queryVertices[0].Position = new Vector3(-querySize / 2, -querySize / 2, -1);
            queryVertices[1].Position = new Vector3( querySize / 2, -querySize / 2, -1);
            queryVertices[2].Position = new Vector3( querySize / 2,  querySize / 2, -1);
            queryVertices[3].Position = new Vector3(-querySize / 2,  querySize / 2, -1);

            // Create the occlusion query object.
            occlusionQuery = new OcclusionQuery(game.GraphicsDevice);

            fSunCamera = fCameraManager.GetCamera("SunCam");

            PresentationParameters pp = game.GraphicsDevice.PresentationParameters;

            fSunView = new RenderTarget2D(game.GraphicsDevice, MaskSize, MaskSize, false, pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
            fIntensityMap = new RenderTarget2D(game.GraphicsDevice, 1, 1, false, SurfaceFormat.Single, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);

            fIntensityEffect = game.Content.Load<Effect>(@"effects\intensity");
            fIntensityMaskVertexDeclaration = VertexPositionTexture.VertexDeclaration; // new VertexDeclaration(game.GraphicsDevice, VertexPositionTexture.VertexElements);

            fOcclusionPoints = GenerateOcclusionPoints();

        }

      private VertexBuffer GenerateOcclusionPoints()
      {
        VertexPositionTexture[] Vertices = new VertexPositionTexture[MaskSize * MaskSize];
        int C = 0;

        for (float y = 0.0f; y < 1.0f; y += (1.0f / (float)MaskSize))
        {
          for (float x = 0.0f; x < 1.0f; x += (1.0f / (float)MaskSize))
          {
            Vertices[C].Position = Vector3.Zero;
            Vertices[C].TextureCoordinate = new Vector2(x, y);
            C++;
          }
        }

        VertexBuffer R = new VertexBuffer(game.GraphicsDevice, VertexPositionTexture.VertexDeclaration, Vertices.Length, BufferUsage.WriteOnly);
        R.SetData<VertexPositionTexture>(Vertices);
        return R;
      }



      public bool IsValid { get { return fIsValid; } }


        #region Draw


      public void Update(GameTime gameTime)
      {
        // set lens flare info
        Position3 P = LightPosition - fSunCamera.Position;
        P.Normalize();
        LightDirection = -(Vector3)P;


        // see if we even need to draw the flare
        Matrix infiniteView = fCameraManager.ActiveCamera.ViewMatrix;
        infiniteView.Translation = Vector3.Zero;

        // Project the light position into 2D screen space.
        Viewport viewport = game.GraphicsDevice.Viewport;

        Vector3 projectedPosition = viewport.Project(-LightDirection, fCameraManager.ActiveCamera.ProjectionMatrix,
                                                     infiniteView, Matrix.Identity);

        // Don't draw any flares if the light is behind the camera.
        if ((projectedPosition.Z < 0) || (projectedPosition.Z > 1))
        {
          fDrawFlare = false;
          return;
        }
        else
          fDrawFlare = true;


        LightScreenPosition = new Vector2(projectedPosition.X, projectedPosition.Y);


        // calculate the projection angle
        Position3 SunCenter = LightPosition - fSunCamera.Position; //  fSolarSystem.fStarPosition - fSunCamera.fPosition;
        Position3 SunOuter = LightPosition; //  fSolarSystem.fStarPosition;
        Position3 V2 = (Position3)fSunCamera.fRight;
        V2.Normalize();
        V2 *= 7000000.0f;  // this is bigger than the actual radius 'cuz the coloring doesn't use up the entire texture surface
        SunOuter += V2;
        SunOuter -= fSunCamera.Position;

        SunCenter.Normalize();
        SunOuter.Normalize();
        double Angle = 2.0 * Math.Acos(Position3.Dot(SunCenter, SunOuter));

        fSunCamera.SetProjectionMatrix(Matrix.CreatePerspectiveFieldOfView((float)Angle, 1.0f, (float)(1.0 / Constants.EarthRadius), 2000000.0f));

      }

      public void IntensityMapBegin()
      {
        game.GraphicsDevice.SetRenderTarget(fSunView);
        game.GraphicsDevice.Clear(Color.White);
        fCameraManager.ActivateCamera("SunCam");
        occlusionAlpha = 1.0f;

        fIsValid = false;
      }


      public void IntensityMapEnd()
      {
        // now we want to filter fSunView to fItensityMask - we're basically adding up
        // the white pixels in the 256 pixel sun view with rgb=010101 to get a total 
        // value of between 0 and 255 for the intensity
        game.GraphicsDevice.SetRenderTarget(fIntensityMap);
        game.GraphicsDevice.Clear(Color.Black);

        game.GraphicsDevice.DepthStencilState = DepthStencilState.None;
        game.GraphicsDevice.BlendState = BlendStateAddOne;

        //game.GraphicsDevice.VertexDeclaration = fIntensityMaskVertexDeclaration;
        //game.GraphicsDevice.Vertices[0].SetSource(fOcclusionPoints, 0, VertexPositionTexture.SizeInBytes);
        game.GraphicsDevice.SetVertexBuffer(fOcclusionPoints);

        fIntensityEffect.Parameters["Source"].SetValue(fSunView); //.GetTexture());
        fIntensityEffect.Parameters["Scale"].SetValue(1.0f / (float)(MaskSize * MaskSize));

        fIntensityEffect.CurrentTechnique.Passes[0].Apply();

        // TODO : PointList no longer exists!
        // TODO : game.GraphicsDevice.DrawPrimitives(PrimitiveType.PointList, 0, MaskSize * MaskSize);

        game.GraphicsDevice.SetRenderTarget(null);
        fIntensityMask = fIntensityMap; // fIntensityMap.GetTexture();

        game.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
        fIsValid = true;
      }

      public void DrawIntensityMap()
      {
        if (!fIsValid) return;

        // draw camera view
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
        spriteBatch.Draw(fSunView,
                         new Vector2(764, 4),
                         new Rectangle(0, 0, MaskSize, MaskSize),
                         Color.White,
                         0.0f,
                         new Vector2(0, 0),
                         256.0f / (float)MaskSize,
                         SpriteEffects.None,
                         0);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
        spriteBatch.Draw(fIntensityMap,
                         new Vector2(700, 4),
                         new Rectangle(0, 0, 1, 1),
                         Color.White,
                         0.0f,
                         new Vector2(0, 0),
                         32.0f,
                         SpriteEffects.None,
                         0);
        spriteBatch.End();
      }

        public void Draw(GameTime gameTime)
        {
          if (! Display) return;

          game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
          //game.GraphicsDevice.RenderState.FillMode = FillMode.Solid;
          //game.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

          // Check whether the light is hidden behind the scenery.
          if (! MaskMode)
            UpdateOcclusion(LightScreenPosition);

          // If it is visible, draw the flare effect.
          if (MaskMode || occlusionAlpha > 0)
          {
            DrawGlow(LightScreenPosition);
            DrawFlares(LightScreenPosition);
          }

          RestoreRenderStates();
        }


        /// <summary>
        /// Mesures how much of the sun is visible, by drawing a small rectangle,
        /// centered on the sun, but with the depth set to as far away as possible,
        /// and using an occlusion query to measure how many of these very-far-away
        /// pixels are not hidden behind the terrain.
        /// 
        /// The problem with occlusion queries is that the graphics card runs in
        /// parallel with the CPU. When you issue drawing commands, they are just
        /// stored in a buffer, and the graphics card can be as much as a frame delayed
        /// in getting around to processing the commands from that buffer. This means
        /// that even after we issue our occlusion query, the occlusion results will
        /// not be available until later, after the graphics card finishes processing
        /// these commands.
        /// 
        /// It would slow our game down too much if we waited for the graphics card,
        /// so instead we delay our occlusion processing by one frame. Each time
        /// around the game loop, we read back the occlusion results from the previous
        /// frame, then issue a new occlusion query ready for the next frame to read
        /// its result. This keeps the data flowing smoothly between the CPU and GPU,
        /// but also causes our data to be a frame out of date: we are deciding
        /// whether or not to draw our lensflare effect based on whether it was
        /// visible in the previous frame, as opposed to the current one! Fortunately,
        /// the camera tends to move slowly, and the lensflare fades in and out
        /// smoothly as it goes behind the scenery, so this out-by-one-frame error
        /// is not too noticeable in practice.
        /// </summary>
        void UpdateOcclusion(Vector2 lightPosition)
        {
            if (occlusionQueryActive)
            {
                // If the previous query has not yet completed, wait until it does.
                if (!occlusionQuery.IsComplete)
                    return;

                // Use the occlusion query pixel count to work
                // out what percentage of the sun is visible.
                const float queryArea = querySize * querySize;

                occlusionAlpha = Math.Min(occlusionQuery.PixelCount / queryArea, 0.5f);
            }

            // Set renderstates for drawing the occlusion query geometry. We want depth
            // tests enabled, but depth writes disabled, and we set ColorWriteChannels
            // to None to prevent this query polygon actually showing up on the screen.
            game.GraphicsDevice.DepthStencilState = DepthTest;
            game.GraphicsDevice.BlendState = NoColors;


            // Set up our BasicEffect to center on the current 2D light position.
            Viewport viewport = game.GraphicsDevice.Viewport;

            basicEffect.World = Matrix.CreateTranslation(lightPosition.X,
                                                         lightPosition.Y, 0);

            basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0,
                                                                        viewport.Width,
                                                                        viewport.Height,
                                                                        0, 0, 1);

            basicEffect.CurrentTechnique.Passes[0].Apply();

            // Issue the occlusion query.
            occlusionQuery.Begin();

            game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, queryVertices, 0, 2, vertexDeclaration);

            occlusionQuery.End();


            // Reset renderstates
            game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            game.GraphicsDevice.BlendState = BlendState.Opaque;

            occlusionQueryActive = true;
        }


        /// <summary>
        /// Draws a large circular glow sprite, centered on the sun.
        /// </summary>
        void DrawGlow(Vector2 lightPosition)
        {
          if (!fDrawFlare) return;

            Vector4 color = new Vector4(1, 1, 1, occlusionAlpha);
            Vector2 origin = new Vector2(glowSprite.Width, glowSprite.Height) / 2;
            float scale = glowSize * 2 / glowSprite.Width;

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            if (MaskMode)
            {
              game.GraphicsDevice.Textures[1] = fIntensityMask;
              game.GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
              fMaskCombine.Parameters["Color"].SetValue(color);
              fMaskCombine.CurrentTechnique.Passes[0].Apply();
            }

            spriteBatch.Draw(glowSprite, lightPosition, null, new Color(color), 0,
                             origin, scale, SpriteEffects.None, 0);

            spriteBatch.End();
        }


        /// <summary>
        /// Draws the lensflare sprites, computing the position
        /// of each one based on the current angle of the sun.
        /// </summary>
        void DrawFlares(Vector2 lightPosition)
        {
          if (!fDrawFlare) return;

          Viewport viewport = game.GraphicsDevice.Viewport;

            // Lensflare sprites are positioned at intervals along a line that
            // runs from the 2D light position toward the center of the screen.
            Vector2 screenCenter = new Vector2(viewport.Width, viewport.Height) / 2;
            
            Vector2 flareVector = screenCenter - lightPosition;

            foreach (Flare flare in flares)
            {
                // Compute the position of this flare sprite.
                Vector2 flarePosition = lightPosition + flareVector * flare.Position;

                // Set the flare alpha based on the previous occlusion query result.
                Vector4 flareColor = flare.Color.ToVector4();

                flareColor.W *= occlusionAlpha;

                // Center the sprite texture.
                Vector2 flareOrigin = new Vector2(flare.Texture.Width,
                                                  flare.Texture.Height) / 2;


                // Draw the flare sprites using additive blending.
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);


                if (MaskMode)
                {
                  game.GraphicsDevice.Textures[1] = fIntensityMask;
                  fMaskCombine.Parameters["Color"].SetValue(flareColor);
                  fMaskCombine.CurrentTechnique.Passes[0].Apply();
                }


                // Draw the flare.
                spriteBatch.Draw(flare.Texture, flarePosition, null,
                                 new Color(flareColor), 1, flareOrigin,
                                 flare.Scale, SpriteEffects.None, 0);


                spriteBatch.End();

            }

          }


        /// <summary>
        /// Sets renderstates back to their default values after we finish drawing
        /// the lensflare, to avoid messing up the 3D terrain rendering.
        /// </summary>
        void RestoreRenderStates()
        {
          /*
          RenderState renderState = game.GraphicsDevice.RenderState;

            renderState.DepthBufferEnable = true;
            renderState.AlphaTestEnable = false;
            renderState.AlphaBlendEnable = false;

            SamplerState samplerState = game.GraphicsDevice.SamplerStates[0];

            samplerState.AddressU = TextureAddressMode.Wrap;
            samplerState.AddressV = TextureAddressMode.Wrap;
          */
        }


        #endregion
    }
}
