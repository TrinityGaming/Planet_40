// compiler directives
#define sun
//#define atmosphere
//#define lensflare      // occlusion fade doesn't work in XNA 4
#define planet

//#define perfhud        // deals with creating graphics device for perfhud - probably doesn't work currently


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Willow;
using Willow.Camera;
using Willow.Input;
using Willow.VertexDefinition;


namespace Planet
{
  public class PlanetGame : Microsoft.Xna.Framework.Game
  {
    GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;
    RasterizerState WireFrame;
    RasterizerState CullClockwiseSolid;
    RasterizerState CullClockwiseWireFrame;
    DepthStencilState DepthNoWrite;

    IInputManager inputManager;
    ICameraManager cameraManager;
    Camera mainCamera;
    Camera frustumCamera;
    Camera sunCamera;
    FrameRate frameRate;

#if planet
    Sphere sphere;
#endif


#if atmosphere
    Sphere atmosphere;
    AtmosphereShader groundFromSpace;
#endif

#if sun
    Sphere sun;
    Effect sunBasicEffect;
#endif

    Effect planetEffectBasic;
    Effect planetEffectTexture;
    Effect planetEffectBump;
    Effect planetEffectBumpSpace;
    Effect planetEffectBumpAtmosphere;
    Effect planetEffectBumpMaps;
    Effect planetEffect;  // current planet effect
    Effect planetEffectMask;
    Effect sunGlowEffect;

    Effect planetEffectAtmosphereSpace;
    Effect planetEffectAtmosphereAtmosphere;

    FillMode fillMode = FillMode.Solid;
    RasterizerState currentRasterizerState;
    bool showHud = true;

    VertexDeclaration positionNormalTextureHeight;
    VertexDeclaration positionColor;
    VertexDeclaration billboardVertexDeclaration;
    BasicEffect testEffect;
    VertexBuffer testSquare;
    SpriteFont debugTextFont;
    Texture2D dirtTexture;
    Texture2D grassTexture;
    Texture2D sunGlowTexture;
    VertexBuffer sunVertexBuffer;

    SpaceDome spaceDome;
    Tank tank;

#if lensflare
    LensFlareComponent lensFlare;
#endif


    VertexPositionColor[] tankTriangle = new VertexPositionColor[14];

    public GraphicsDeviceManager Graphics { get { return graphics; } }

    public PlanetGame()
    {
      graphics = new GraphicsDeviceManager(this);
      Content.RootDirectory = "Content";
      graphics.PreferMultiSampling = true;

//#if perfhud
//      graphics.PreparingDeviceSettings += this.PreparingDeviceSettings;
//#endif


      if (Constants.FullScreen)
      {
        graphics.PreferredBackBufferWidth = Constants.FullScreenWidth;
        graphics.PreferredBackBufferHeight = Constants.FullScreenHeight;
        graphics.ToggleFullScreen();
      }
      else
      {
        graphics.PreferredBackBufferWidth = Constants.WindowedWidth;
        graphics.PreferredBackBufferHeight = Constants.WindowedHeight;
      }

      Globals.Game = this;



#if PROFILE
      graphics.SynchronizeWithVerticalRetrace = false;
      this.IsFixedTimeStep = false;
#endif

      inputManager = InputManager.CreateInputManager(this);
      cameraManager = CameraManager.CreateCameraManager(this);
    }

//#if perfhud
//    private void PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e) 
//    { 
//      foreach (GraphicsAdapter adapter in GraphicsAdapter.Adapters) 
//      { 
//        if (adapter.Description.Contains("PerfHUD")) 
//        { 
//          e.GraphicsDeviceInformation.Adapter = adapter; 
//          //e.GraphicsDeviceInformation.DeviceType = DeviceType.Reference; 
//          break; 
//        } 
//      } 

//      return; 
//    }
//#endif


    protected override void Initialize()
    {
      // main camera
      mainCamera = cameraManager.CreateCamera("Main");
      mainCamera.fViewport = GraphicsDevice.Viewport;
      mainCamera.fLocalPosition = new Position3(0, 0, 10000);
      mainCamera.AcceptInput = true;


      // frustum camera
      if (Constants.UseFrustumCamera)
      {
        frustumCamera = cameraManager.CreateCamera("Frustum");
        frustumCamera.fViewport = GraphicsDevice.Viewport;
        frustumCamera.fLocalPosition = new Position3(0, 0, 10000);
        frustumCamera.AcceptInput = false;

        //cameraManager.ActivateFrustumCamera("Frustum");
      }

      // sun camera
      sunCamera = cameraManager.CreateCamera("SunCam");
      sunCamera.fViewport = new Viewport();
      sunCamera.fViewport.Width = 32;
      sunCamera.fViewport.Height = 32;


      // frame rate calculation
      frameRate = new FrameRate(this);
      frameRate.UpdateOrder = 1;
      Components.Add(frameRate);


      Mouse.SetPosition(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);

      base.Initialize();

      ResetCameraPosition();
    }


    protected override void LoadContent()
    {
      WireFrame = new RasterizerState {
        CullMode = CullMode.CullCounterClockwiseFace,
        FillMode = FillMode.WireFrame,
        MultiSampleAntiAlias = true
      };


      DepthNoWrite = new DepthStencilState
      {
        DepthBufferEnable = true,
        DepthBufferWriteEnable = false
      };


      CullClockwiseSolid = new RasterizerState
      {
        CullMode = CullMode.CullClockwiseFace,
        FillMode = FillMode.Solid,
        MultiSampleAntiAlias = true
      };


      CullClockwiseWireFrame = new RasterizerState
      {
        CullMode = CullMode.CullClockwiseFace,
        FillMode = FillMode.WireFrame,
        MultiSampleAntiAlias = true
      };



      currentRasterizerState = RasterizerState.CullCounterClockwise;

      TerrainNodeDelegates.InitializeTerrainNodeDelegates(GraphicsDevice);
      TerrainNodeSplitManager.Initialize(this);

      spriteBatch = new SpriteBatch(GraphicsDevice);
      positionNormalTextureHeight = VertexPositionNormalTextureHeight.VertexDeclaration; // new VertexDeclaration(GraphicsDevice, VertexPositionNormalTextureHeight.VertexElements);
      positionColor = VertexPositionColor.VertexDeclaration; // new VertexDeclaration(GraphicsDevice, VertexPositionColor.VertexElements);

      dirtTexture = Content.Load<Texture2D>(@"Textures\dirt_01");
      grassTexture = Content.Load<Texture2D>(@"Textures\grass_01");
      sunGlowTexture = Content.Load<Texture2D>(@"textures\sun_glow");
      planetEffectMask = Content.Load<Effect>(@"Effects\PlanetMask");
      planetEffectBasic = Content.Load<Effect>(@"Effects\PlanetBasic");
      planetEffectTexture = Content.Load<Effect>(@"Effects\PlanetBasicTexture");
      planetEffectBump = Content.Load<Effect>(@"Effects\PlanetBump");
      planetEffectBumpSpace = Content.Load<Effect>(@"Effects\PlanetBumpFromSpace");
      planetEffectBumpAtmosphere = Content.Load<Effect>(@"Effects\PlanetBumpFromAtmosphere");
      planetEffectBumpMaps = Content.Load<Effect>(@"Effects\PlanetBumpMaps");

      planetEffectAtmosphereSpace = Content.Load<Effect>(@"Effects\AtmosphereBasicSpace");
      planetEffectAtmosphereAtmosphere = Content.Load<Effect>(@"Effects\AtmosphereBasicAtmosphere");

      // select initial render effect
      planetEffect = null;
      SelectNextRenderMode();


#if sun
      sunBasicEffect = Content.Load<Effect>(@"Effects\SunBasic");
#endif

      debugTextFont = Content.Load<SpriteFont>(@"Fonts\DebugTextFont");


      // initialize terrain node index buffer
      TerrainNodeIndexBuffer.CreateIndices(GraphicsDevice, Constants.PatchWidth, Constants.PatchHeight);


      // create test sphere
      Position3 p = new Position3(1, 1, 1);
      p.Normalize();
      p *= 149597870.691;     // earth -> sun distance

#if planet
      sphere = new Sphere(p, Constants.EarthRadius, true, TerrainNodeDelegates.CreatePositionPlanet, TerrainNodeDelegates.CreateTerrainNodeVertexBuffer, false);
#endif

#if atmosphere
      // create test atmosphere
      atmosphere = new Sphere(p, Constants.EarthAtmosphereRadius, true, TerrainNodeDelegates.CreatePositionSphere, TerrainNodeDelegates.CreateTerrainNodeVertexBuffer, true);

      // create atmosphere shaders
      groundFromSpace = new AtmosphereShader();
#endif

#if sun
      // create sun
      sun = new Sphere(Position3.Zero, Constants.SunRadius, false, TerrainNodeDelegates.CreatePositionSphere, TerrainNodeDelegates.CreateTerrainNodeVertexBuffer, true);
#endif


      testEffect = new BasicEffect(GraphicsDevice);
      testSquare = GenerateSquare(25, 0, Color.LawnGreen);

      mainCamera.AttachedPosition = p;  // camera is attached to the sphere
      mainCamera.UpdateCamera(0);
      mainCamera.LookAt(Position3.Zero);

      if (frustumCamera != null)
      {
        frustumCamera.AttachedPosition = p;  // camera is attached to the sphere
        frustumCamera.UpdateCamera(0);
        frustumCamera.LookAt(Position3.Zero);
      }

      // space dome
      spaceDome = new SpaceDome(Constants.EarthRadius * 3.0, Constants.EarthRadius * 100.0);
      spaceDome.LoadContent(this);

      tank = new Tank();
      tank.LoadContent(this);
      tank.Movement.AttachedPosition = p;
      tank.Movement.UpdateMovement(0);


      sunGlowEffect = Content.Load<Effect>(@"effects\billboard");
      billboardVertexDeclaration = VertexPositionTexture.VertexDeclaration; // new VertexDeclaration(this.GraphicsDevice, VertexPositionTexture.VertexElements);
      GenerateSunVertices();


#if lensflare
      // lens flare
      lensFlare = new LensFlareComponent();
      lensFlare.LoadContent(this);
      lensFlare.LightPosition = Position3.Zero;
      lensFlare.MaskMode = true;
#endif


    }


    protected override void UnloadContent()
    {
    }


    public void GenerateSunVertices()
    {
      VertexPositionTexture[] Vertices = new VertexPositionTexture[6];

      float S = 69550000.0f;
      float Z = 0.0f;

      VertexPositionTexture V = new VertexPositionTexture();
      V.Position = new Vector3(-S, -S, Z);
      V.Position.Normalize();
      V.Position *= S;
      V.TextureCoordinate = new Vector2(0, 0);
      Vertices[0] = V;

      V = new VertexPositionTexture();
      V.Position = new Vector3(-S, S, Z);
      V.Position.Normalize();
      V.Position *= S;
      V.TextureCoordinate = new Vector2(0, 1);
      Vertices[1] = V;

      V = new VertexPositionTexture();
      V.Position = new Vector3(S, S, Z);
      V.Position.Normalize();
      V.Position *= S;
      V.TextureCoordinate = new Vector2(1, 1);
      Vertices[2] = V;

      V = new VertexPositionTexture();
      V.Position = new Vector3(S, S, Z);
      V.Position.Normalize();
      V.Position *= S;
      V.TextureCoordinate = new Vector2(1, 1);
      Vertices[3] = V;

      V = new VertexPositionTexture();
      V.Position = new Vector3(S, -S, Z);
      V.Position.Normalize();
      V.Position *= S;
      V.TextureCoordinate = new Vector2(1, 0);
      Vertices[4] = V;

      V = new VertexPositionTexture();
      V.Position = new Vector3(-S, -S, Z);
      V.Position.Normalize();
      V.Position *= S;
      V.TextureCoordinate = new Vector2(0, 0);
      Vertices[5] = V;

      VertexBuffer R = new VertexBuffer(this.GraphicsDevice, VertexPositionTexture.VertexDeclaration, Vertices.Length, BufferUsage.WriteOnly);

      R.SetData<VertexPositionTexture>(Vertices);

      sunVertexBuffer = R;
    }

    public VertexBuffer GenerateSquare(float S, float Z, Color C)
    {
      VertexPositionColor[] Vertices = new VertexPositionColor[6];

      VertexPositionColor V = new VertexPositionColor();
      V.Position = new Vector3(-S, -S, Z);
      V.Color = C;
      Vertices[0] = V;

      V = new VertexPositionColor();
      V.Position = new Vector3(-S, S, Z);
      V.Color = C;
      Vertices[1] = V;

      V = new VertexPositionColor();
      V.Position = new Vector3(S, S, Z);
      V.Color = C;
      Vertices[2] = V;

      V = new VertexPositionColor();
      V.Position = new Vector3(S, S, Z);
      V.Color = C;
      Vertices[3] = V;

      V = new VertexPositionColor();
      V.Position = new Vector3(S, -S, Z);
      V.Color = C;
      Vertices[4] = V;

      V = new VertexPositionColor();
      V.Position = new Vector3(-S, -S, Z);
      V.Color = C;
      Vertices[5] = V;

      VertexBuffer R = new VertexBuffer(graphics.GraphicsDevice, VertexPositionColor.VertexDeclaration, Vertices.Length, BufferUsage.WriteOnly);
      R.SetData<VertexPositionColor>(Vertices);

      return R;
    }



    private void ResetCameraPosition()
    {
      // start at local planet space
      Position3 P = Position3.Zero;

      // move out to planet radius
      P -= new Position3(0, 0, Constants.EarthRadius);

      // add some altitude above planet surface
#if XBOX
      P -= new Position3(0, 0, 6000.0);
#else
      P -= new Position3(0, 0, 6000.0);
#endif

      // rotate a bit to the sunward side
      Matrix M = Matrix.CreateRotationY(MathHelper.ToRadians(90.0f));
      P = (Position3)Vector3.Transform((Vector3)P, M);

      // orient right side up
      mainCamera.Orientation = Quaternion.CreateFromYawPitchRoll(-MathHelper.PiOver2, 0, MathHelper.PiOver2);

      // set camera positions
      mainCamera.fLocalPosition = P;

      if (frustumCamera != null)
      {
        frustumCamera.Orientation = mainCamera.Orientation;
        frustumCamera.fLocalPosition = P;
      }
    }

    private void ExitGame()
    {
      // stop terrain node split queue processing thread
      SplitTerrainNodeQueue.Quit();

      // exit application
      this.Exit();
    }

    private void SelectNextRenderMode()
    {
#if atmosphere
      if (planetEffect == null)
        planetEffect = planetEffectBumpSpace;
      else
#endif
      if (planetEffect == null || planetEffect == planetEffectBumpSpace)
        planetEffect = planetEffectBump;
      else if (planetEffect == planetEffectBump)
        planetEffect = planetEffectTexture;
      else if (planetEffect == planetEffectTexture)
        planetEffect = planetEffectBasic;
      else if (planetEffect == planetEffectBasic)
        planetEffect = planetEffectBumpMaps;
      else
        planetEffect = planetEffectBump;


      ValidateRenderMode();
    }


    private void ValidateRenderMode()
    {
      // eliminate effects that aren't currently available due to settings
      if (Constants.DisableNormalMapGeneration)
        if (planetEffect == planetEffectBumpSpace || planetEffect == planetEffectBumpMaps)
          SelectNextRenderMode();

      if (Constants.DisableDiffuseTextureGeneration)
        if (planetEffect == planetEffectTexture || planetEffect == planetEffectBumpSpace || planetEffect == planetEffectBumpMaps)
          SelectNextRenderMode();
    }


    protected override void Update(GameTime gameTime)
    {
      if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) ExitGame();
      if (Keyboard.GetState().IsKeyDown(Keys.Escape)) ExitGame();

      if (inputManager.KeyPressed(Keys.H))
        showHud = !showHud;


      if (inputManager.KeyPressed(Keys.T))
      {
        if (mainCamera.AcceptInput)
        {
          mainCamera.AcceptInput = false;
          tank.Movement.AcceptInput = true;
        }
        else
        {
          mainCamera.AcceptInput = true;
          tank.Movement.AcceptInput = false;
        }
      }

      // toggle frustum camera
      if (inputManager.KeyPressed(Keys.O))
      {
        if (cameraManager.ActiveFrustumCamera != frustumCamera)
        {
          frustumCamera.Clone(mainCamera);
          frustumCamera.AcceptInput = false;
          cameraManager.ActivateFrustumCamera("Frustum");
        }
        else
          cameraManager.ActivateFrustumCamera("Main");


      }

      //if (inputManager.KeyPressed(Keys.Right))
      //{
      //  Globals.uOffset += 1.0f / 256.0f;
      //}

      //if (inputManager.KeyPressed(Keys.Left))
      //{
      //  Globals.uOffset -= (1.0f / 256.0f);
      //}

      if (inputManager.KeyPressed(Keys.R))
        ResetCameraPosition();


      if (inputManager.KeyPressed(Keys.M))
      {
        if (fillMode == FillMode.Solid)
        {
          fillMode = FillMode.WireFrame;
          currentRasterizerState = WireFrame;
        }
        else if (fillMode == FillMode.WireFrame)
        {
          fillMode = FillMode.Solid;
          currentRasterizerState = RasterizerState.CullCounterClockwise;
        }
      }

      if (inputManager.KeyPressed(Keys.P))
      {
        SelectNextRenderMode();
      }

      // update frustum camera
      if (frustumCamera != null)
      {
        if (inputManager.KeyDown(Keys.Right))
        {
          frustumCamera.YawRight();
          frustumCamera.Update(gameTime);
          frustumCamera.UpdateProjectionMatrix();
        }

        if (inputManager.KeyDown(Keys.Left))
        {
          frustumCamera.YawLeft();
          frustumCamera.Update(gameTime);
          frustumCamera.UpdateProjectionMatrix();
        }
      }


      base.Update(gameTime);


      // count total terrain nodes
      Globals.NodeCount = 0;

#if planet
      CountTerrainNodes(sphere.Front);
      CountTerrainNodes(sphere.Back);
      CountTerrainNodes(sphere.Left);
      CountTerrainNodes(sphere.Right);
      CountTerrainNodes(sphere.Top);
      CountTerrainNodes(sphere.Bottom);
#endif


      // point SunCam at sun - runs after base.Update so camera Up vector has been set
      sunCamera.Position = mainCamera.Position;
      sunCamera.fUp = mainCamera.fUp;
      sunCamera.LookAt(Position3.Zero);

#if lensflare
      lensFlare.Update(gameTime);
#endif

      UpdateTank(gameTime);
    }


    private void UpdateTank(GameTime gameTime)
    {
      // move the tank to the current camera position
      if (inputManager.KeyPressed(Keys.N))
      {
        tank.Movement.fLocalPosition = mainCamera.fLocalPosition;
        tank.Movement.UpdateMovement(0);
      }

#if planet
      tank.HandleInput(gameTime, sphere, inputManager);
#endif
    }


    private void CountTerrainNodes(TerrainNode node)
    {
      Globals.NodeCount++;

      if (node.HasChildren)
      {
        if (node.Children[0] != null) CountTerrainNodes(node.Children[0]);
        if (node.Children[1] != null) CountTerrainNodes(node.Children[1]);
        if (node.Children[2] != null) CountTerrainNodes(node.Children[2]);
        if (node.Children[3] != null) CountTerrainNodes(node.Children[3]);
      }
    }


    private void RenderTestSquares()
    {
      GraphicsDevice device = graphics.GraphicsDevice;
      // TODO : device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

      testEffect.Projection = mainCamera.ProjectionMatrix;
      testEffect.World = Matrix.CreateTranslation(-(Vector3)mainCamera.Position);
      testEffect.View = mainCamera.ViewMatrix;
      testEffect.EnableDefaultLighting();
      testEffect.VertexColorEnabled = true;

      testEffect.CurrentTechnique.Passes[0].Apply();
      device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
    }


    private void CalculateProjection(Sphere sphere)
    {
      Camera C = cameraManager.ActiveCamera;

      double H = (sphere.Position - C.Position).Length();
      double L = H; //  H - 9.0;

      if (L < sphere.Radius + 10.0)  // TODO : the distance here is important - might need to scale to planet size
      {
        C.NearClip = (float)(0.001 / sphere.Radius);
        C.FarClip = 20000.0f;
        C.UpdateProjectionMatrix();
      }
      else if (L < sphere.Radius + 20.0)  // TODO : the distance here is important - might need to scale to planet size
      {
        C.NearClip = (float)(0.01 / sphere.Radius);
        C.FarClip = 20000.0f;
        C.UpdateProjectionMatrix();
      }
      else if (L < sphere.Radius + 50.0)  // TODO : the distance here is important - might need to scale to planet size
      {
        C.NearClip = (float)(0.1 / sphere.Radius);
        C.FarClip = 20000.0f;
        C.UpdateProjectionMatrix();
      }
      else if (L < sphere.Radius + 6000.0)
      {
        C.NearClip = 0.001f; //  0.000001f; //  (float)(1.0 /* 30.0 */ / sphere.Radius);
        C.FarClip = 20000.0f;
        C.UpdateProjectionMatrix();
      }
      else
      {
        C.NearClip = 0.001f; //  (float)(0.1 /* 150.0 */ / sphere.Radius);
        C.FarClip = 20000.0f;
        C.UpdateProjectionMatrix();
      }
    }


    /// <summary>
    /// Determine if this node should be culled, either horizon or view frustum
    /// </summary>
    /// <param name="cameraPosition">Camera position in planet space</param>
    /// <param name="horizonAngle">Horizon angle in radians</param>
    /// <returns>Boolean indicating whether or not the node should be culled</returns>
    private bool CullTerrainNode(TerrainNode terrainNode, Position3 cameraPosition, float horizonAngle, Sphere sphere, Camera camera)
    {
      // don't cull away top level nodes if they're splitting
      if (terrainNode.Splitting && terrainNode.Level <= 2) return false;

      Position3 nodePosition;


      ///// horizon culling /////
      if (!Constants.DisableHorizonCulling && horizonAngle != 0)
      {
        if (terrainNode.Level >= 2)
        {
          // get the camera position on the unit sphere
          Position3 unitCameraPosition = cameraPosition;
          unitCameraPosition.Normalize();

          // get the planet node position on the unit sphere
          nodePosition = terrainNode.ClosestPosition;
          nodePosition.Normalize();

          // get the node's angle relative to the camera
          float angle = (float)Math.Acos(Position3.Dot(unitCameraPosition, nodePosition));

          // if it's over the horizon then cull the node
          if (angle > horizonAngle)
          {
            Globals.HorizonCullCount++;
            return true;
          }
        }
      }



      ///// view frustum culling /////

      if (!Constants.DisableFrustumCulling)
      {
        // get node's camera space position
        nodePosition = (sphere.Position + terrainNode.Position) - camera.Position;


        // create a matrix for frustum culling
        Matrix M = sphere.ScaleMatrix * Matrix.CreateTranslation(nodePosition.AsVector3 * (float)sphere.Scale);

        // create bounding box transformed to camera space
        Vector3 MinV;
        Vector3 MaxV;

        MinV = Vector3.Transform(terrainNode.MinVertex, M);
        MaxV = Vector3.Transform(terrainNode.MaxVertex, M);

        BoundingBox B = new BoundingBox(MinV, MaxV);
        ContainmentType CT;

        // if it's not contained within the view frustum then cull the node
        camera.Frustum.Contains(ref B, out CT);
        if (CT == ContainmentType.Disjoint)
        {
          Globals.FrustumCullCount++;
          return true;
        }
      }


      // otherwise it's visible
      return false;
    }



    private void DrawTerrainNode(TerrainNode terrainNode, Position3 cameraPosition, float horizonAngle, Sphere sphere, Camera camera)
    {
      // this can happen if a node was partially split
      if (terrainNode == null) return;


      // no need to draw or recurse any deeper if the node isn't visible
      if (CullTerrainNode(terrainNode, cameraPosition, horizonAngle, sphere, cameraManager.ActiveFrustumCamera))
      {
        // if the node is being split then we can cancel the split if it's not visible
        terrainNode.CancelSplitting = true;
        return;
      }


      // we only draw leaf nodes, so recurse down until we find them, then draw them
      // a node that's splitting is considered a leaf node
      if (!terrainNode.Splitting && terrainNode.HasChildren)
      {
        DrawTerrainNode(terrainNode.Children[0], cameraPosition, horizonAngle, sphere, camera);
        DrawTerrainNode(terrainNode.Children[1], cameraPosition, horizonAngle, sphere, camera);
        DrawTerrainNode(terrainNode.Children[2], cameraPosition, horizonAngle, sphere, camera);
        DrawTerrainNode(terrainNode.Children[3], cameraPosition, horizonAngle, sphere, camera);
      }
      else
      {
        Globals.DrawCount++;

        // track max draw level
        if (terrainNode.Level > Globals.DrawLevel)
          Globals.DrawLevel = terrainNode.Level;


        // get world space position by adding terrain node position to planet position
        Position3 worldSpacePosition = sphere.Position + terrainNode.Position;

        // translate to camera space by subtracting the camera position, scale by planet scale
        Position3 cameraSpacePosition = (worldSpacePosition - camera.Position) * sphere.Scale;

        Matrix cameraSpaceMatrix = sphere.ScaleMatrix * Matrix.CreateTranslation(cameraSpacePosition.AsVector3);
        Matrix worldViewProjectionMatrix = cameraSpaceMatrix * camera.ViewProjectionMatrix;

        // get light position, translated to camera space, TODO : light position is currently in the solar system center
        Vector3 lightPosition = (Vector3)(camera.Position - new Position3(0, 0, 0));

        // get light direction - "space" doesn't matter here since it's just a direction
        Vector3 lightDirection = (Vector3)(worldSpacePosition - new Position3(0, 0, 0));
        lightDirection.Normalize();


        // determine which shader to use
        double altitude = mainCamera.fLocalPosition.Length();
        Effect effect;

#if atmosphere
        if (altitude <= Constants.EarthAtmosphereRadius)
          effect = planetEffectBumpAtmosphere;
        else
          effect = planetEffectBumpSpace;
#else
        effect = planetEffect;
#endif


        effect.Parameters["WorldViewProj"].SetValue(worldViewProjectionMatrix);
        effect.Parameters["LightDirection"].SetValue(-lightDirection);

        if (effect == planetEffectTexture)
        {
          //effect.Parameters["Tint"].SetValue(new Vector4(1, 1, 1, 1));
          effect.Parameters["DiffuseTexture"].SetValue(terrainNode.DiffuseTexture);
        }
        else if (effect != planetEffectBasic)
        {
          effect.Parameters["WorldMatrix"].SetValue(cameraSpaceMatrix);
          effect.Parameters["DiffuseTexture"].SetValue(terrainNode.DiffuseTexture);
          effect.Parameters["NormalTexture"].SetValue(terrainNode.NormalTexture);
        }

        /*
        if (terrainNode == tank.NodeUnderTank)
          effect.Parameters["Tint"].SetValue(new Vector4(1.0f, 0.5f, 0.5f, 1f));
        else*/
        effect.Parameters["Tint"].SetValue(new Vector4(1f, 1f, 1f, 1f));

        // set up atmosphere parameters
#if atmosphere
        groundFromSpace.ResolveParameters(effect);

        float cameraHeight = (float)(camera.Position - sphere.Position).Length();
        Vector3 cameraPositionNode = (camera.Position - sphere.Position).AsVector3;

        Vector3 vLightDirection = (Vector3)(Position3.Zero - camera.Position);
        vLightDirection.Normalize();

        effect.Parameters["PatchPosition"].SetValue(terrainNode.Position.AsVector3);
        effect.Parameters["v3CameraPos"].SetValue(cameraPositionNode);
        effect.Parameters["v3LightPos"].SetValue(vLightDirection);
        effect.Parameters["fCameraHeight"].SetValue(cameraHeight);
        effect.Parameters["fCameraHeight2"].SetValue(cameraHeight * cameraHeight);
#endif


        effect.CurrentTechnique.Passes[0].Apply();

        GraphicsDevice.SetVertexBuffer(terrainNode.VertexBuffer.VertexBuffer);
        GraphicsDevice.Indices = TerrainNodeIndexBuffer.Indices;
        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, terrainNode.VertexBuffer.VertexCount, 0, TerrainNodeIndexBuffer.IndexCount / 3);

        //DrawNodeFrustum(terrainNode);
      }
    }


    private void DrawSphere(Sphere sphere)
    {
      // get frustum camera position in planet space
      Camera frustumCamera = cameraManager.ActiveFrustumCamera;
      Position3 frustumCameraPosition = frustumCamera.Position - sphere.Position;

      // update sphere, allowing it to split
      sphere.Update(frustumCameraPosition, MathHelper.ToRadians(frustumCamera.FieldOfView));


      //GraphicsDevice.VertexDeclaration = positionNormalTextureHeight;
      GraphicsDevice.DepthStencilState = DepthStencilState.Default;
      GraphicsDevice.BlendState = BlendState.Opaque;
      GraphicsDevice.RasterizerState = currentRasterizerState;

      CalculateProjection(sphere);


      Camera camera = mainCamera;


      // get frustum camera position in planet space
      Position3 cameraPosition = camera.Position - sphere.Position;

      // calculate horizon angle, using camera position in planet space
      float horizonAngle = sphere.CalculateHorizonAngle(cameraPosition);

      Globals.DrawLevel = 0;
      Globals.DrawCount = 0;
      Globals.HorizonCullCount = 0;
      Globals.FrustumCullCount = 0;

      DrawTerrainNode(sphere.Front, cameraPosition, horizonAngle, sphere, camera);
      DrawTerrainNode(sphere.Back, cameraPosition, horizonAngle, sphere, camera);
      DrawTerrainNode(sphere.Left, cameraPosition, horizonAngle, sphere, camera);
      DrawTerrainNode(sphere.Right, cameraPosition, horizonAngle, sphere, camera);
      DrawTerrainNode(sphere.Top, cameraPosition, horizonAngle, sphere, camera);
      DrawTerrainNode(sphere.Bottom, cameraPosition, horizonAngle, sphere, camera);
    }



    private void DrawTerrainNodeMask(TerrainNode terrainNode, Position3 cameraPosition, float horizonAngle, Sphere sphere, Camera camera)
    {
      // this can happen if a node was partially split
      if (terrainNode == null) return;


      // no need to draw or recurse any deeper if the node isn't visible
      if (CullTerrainNode(terrainNode, cameraPosition, horizonAngle, sphere, cameraManager.ActiveFrustumCamera))
      {
        // if the node is being split then we can cancel the split if it's not visible
        terrainNode.CancelSplitting = true;
        return;
      }


      // we only draw leaf nodes, so recurse down until we find them, then draw them
      // a node that's splitting is considered a leaf node
      if (!terrainNode.Splitting && terrainNode.HasChildren)
      {
        DrawTerrainNodeMask(terrainNode.Children[0], cameraPosition, horizonAngle, sphere, camera);
        DrawTerrainNodeMask(terrainNode.Children[1], cameraPosition, horizonAngle, sphere, camera);
        DrawTerrainNodeMask(terrainNode.Children[2], cameraPosition, horizonAngle, sphere, camera);
        DrawTerrainNodeMask(terrainNode.Children[3], cameraPosition, horizonAngle, sphere, camera);
      }
      else
      {
        // get world space position by adding terrain node position to planet position
        Position3 worldSpacePosition = sphere.Position + terrainNode.Position;

        // translate to camera space by subtracting the camera position, scale by planet scale
        Position3 cameraSpacePosition = (worldSpacePosition - camera.Position) * sphere.Scale;

        Matrix cameraSpaceMatrix = sphere.ScaleMatrix * Matrix.CreateTranslation(cameraSpacePosition.AsVector3);
        Matrix worldViewProjectionMatrix = cameraSpaceMatrix * camera.ViewProjectionMatrix;


        // we're drawing in black, so we don't need to much information
        planetEffectMask.Parameters["WorldViewProj"].SetValue(worldViewProjectionMatrix);

        planetEffectMask.CurrentTechnique.Passes[0].Apply();
        GraphicsDevice.SetVertexBuffer(terrainNode.VertexBuffer.VertexBuffer);
        GraphicsDevice.Indices = TerrainNodeIndexBuffer.Indices;
        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, terrainNode.VertexBuffer.VertexCount, 0, TerrainNodeIndexBuffer.IndexCount / 3);
      }
    }

    private void DrawSphereMask(Sphere sphere)
    {
      //GraphicsDevice.VertexDeclaration = positionNormalTextureHeight;

      GraphicsDevice.DepthStencilState = DepthStencilState.Default;
      GraphicsDevice.BlendState = BlendState.Opaque;
      GraphicsDevice.RasterizerState = currentRasterizerState;

      //CalculateProjection();

      Camera camera = cameraManager.ActiveCamera;


      // get frustum camera position in planet space
      Position3 cameraPosition = camera.Position - sphere.Position;

      // calculate horizon angle, using camera position in planet space
      float horizonAngle = sphere.CalculateHorizonAngle(cameraPosition);

      DrawTerrainNodeMask(sphere.Front, cameraPosition, horizonAngle, sphere, camera);
      DrawTerrainNodeMask(sphere.Back, cameraPosition, horizonAngle, sphere, camera);
      DrawTerrainNodeMask(sphere.Left, cameraPosition, horizonAngle, sphere, camera);
      DrawTerrainNodeMask(sphere.Right, cameraPosition, horizonAngle, sphere, camera);
      DrawTerrainNodeMask(sphere.Top, cameraPosition, horizonAngle, sphere, camera);
      DrawTerrainNodeMask(sphere.Bottom, cameraPosition, horizonAngle, sphere, camera);
    }



#if sun
    private void DrawSunNode(TerrainNode terrainNode, Position3 cameraPosition, float horizonAngle, Sphere sphere, Camera camera)
    {
      // this can happen if a node was partially split
      if (terrainNode == null) return;


      // no need to draw or recurse any deeper if the node isn't visible
      if (CullTerrainNode(terrainNode, cameraPosition, horizonAngle, sphere, cameraManager.ActiveFrustumCamera))
      {
        // if the node is being split then we can cancel the split if it's not visible
        terrainNode.CancelSplitting = true;
        return;
      }


      // we only draw leaf nodes, so recurse down until we find them, then draw them
      // a node that's splitting is considered a leaf node
      if (!terrainNode.Splitting && terrainNode.HasChildren)
      {
        DrawSunNode(terrainNode.Children[0], cameraPosition, horizonAngle, sphere, camera);
        DrawSunNode(terrainNode.Children[1], cameraPosition, horizonAngle, sphere, camera);
        DrawSunNode(terrainNode.Children[2], cameraPosition, horizonAngle, sphere, camera);
        DrawSunNode(terrainNode.Children[3], cameraPosition, horizonAngle, sphere, camera);
      }
      else
      {
        // don't draw if it's not visible
        if (CullTerrainNode(terrainNode, cameraPosition, horizonAngle, sphere, cameraManager.ActiveFrustumCamera))
        {
          // if the node is being split then we can cancel the split if it's not visible
          terrainNode.CancelSplitting = true;
          return;
        }

        // get world space position by adding terrain node position to planet position
        Position3 worldSpacePosition = sphere.Position + terrainNode.Position;

        // translate to camera space by subtracting the camera position, scale by planet scale
        Position3 cameraSpacePosition = (worldSpacePosition - camera.Position) * sphere.Scale;

        Matrix cameraSpaceMatrix = sphere.ScaleMatrix * Matrix.CreateTranslation(cameraSpacePosition.AsVector3);
        Matrix worldViewProjectionMatrix = cameraSpaceMatrix * camera.ViewProjectionMatrix;

        sunBasicEffect.Parameters["PatchPosition"].SetValue(terrainNode.Position.AsVector3);
        sunBasicEffect.Parameters["WorldViewProj"].SetValue(worldViewProjectionMatrix);

        sunBasicEffect.CurrentTechnique.Passes[0].Apply();

        GraphicsDevice.SetVertexBuffer(terrainNode.VertexBuffer.VertexBuffer);
        GraphicsDevice.Indices = TerrainNodeIndexBuffer.Indices;
        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, terrainNode.VertexBuffer.VertexCount, 0, TerrainNodeIndexBuffer.IndexCount / 3);
      }
    }


    private void DrawSun(Sphere sphere)
    {
      // get frustum camera position in planet space
      Camera frustumCamera = cameraManager.ActiveFrustumCamera;
      Position3 frustumCameraPosition = frustumCamera.Position - sphere.Position;

      sun.Update(frustumCameraPosition, MathHelper.ToRadians(frustumCamera.FieldOfView));

      //GraphicsDevice.VertexDeclaration = positionNormalTextureHeight;

      GraphicsDevice.DepthStencilState = DepthStencilState.Default;
      GraphicsDevice.BlendState = BlendState.Opaque;
      GraphicsDevice.RasterizerState = currentRasterizerState;

      CalculateProjection(sphere);

      Camera camera = mainCamera;

      // get frustum camera position in planet space
      Position3 cameraPosition = camera.Position - sphere.Position;

      // calculate horizon angle, using camera position in planet space
      float horizonAngle = sphere.CalculateHorizonAngle(cameraPosition);

      DrawSunNode(sphere.Front, cameraPosition, horizonAngle, sphere, camera);
      DrawSunNode(sphere.Back, cameraPosition, horizonAngle, sphere, camera);
      DrawSunNode(sphere.Left, cameraPosition, horizonAngle, sphere, camera);
      DrawSunNode(sphere.Right, cameraPosition, horizonAngle, sphere, camera);
      DrawSunNode(sphere.Top, cameraPosition, horizonAngle, sphere, camera);
      DrawSunNode(sphere.Bottom, cameraPosition, horizonAngle, sphere, camera);
    }
#endif


#if atmosphere
    private void DrawAtmosphereNode(TerrainNode terrainNode, Position3 cameraPosition, float horizonAngle, Sphere sphere, Camera camera)
    {
      // this can happen if a node was partially split
      if (terrainNode == null) return;


      // no need to draw or recurse any deeper if the node isn't visible
      if (CullTerrainNode(terrainNode, cameraPosition, 0, sphere, cameraManager.ActiveFrustumCamera))
      {
        // if the node is being split then we can cancel the split if it's not visible
        terrainNode.CancelSplitting = true;
        return;
      }


      // we only draw leaf nodes, so recurse down until we find them, then draw them
      // a node that's splitting is considered a leaf node
      if (!terrainNode.Splitting && terrainNode.HasChildren)
      {
        DrawAtmosphereNode(terrainNode.Children[0], cameraPosition, horizonAngle, sphere, camera);
        DrawAtmosphereNode(terrainNode.Children[1], cameraPosition, horizonAngle, sphere, camera);
        DrawAtmosphereNode(terrainNode.Children[2], cameraPosition, horizonAngle, sphere, camera);
        DrawAtmosphereNode(terrainNode.Children[3], cameraPosition, horizonAngle, sphere, camera);
      }
      else
      {
        Globals.DrawCount++;

        // track max draw level
        if (terrainNode.Level > Globals.DrawLevel)
          Globals.DrawLevel = terrainNode.Level;


        // get world space position by adding terrain node position to planet position
        Position3 worldSpacePosition = sphere.Position + terrainNode.Position;

        // translate to camera space by subtracting the camera position, scale by planet scale
        Position3 cameraSpacePosition = (worldSpacePosition - camera.Position) * sphere.Scale;

        Matrix cameraSpaceMatrix = sphere.ScaleMatrix * Matrix.CreateTranslation(cameraSpacePosition.AsVector3);
        Matrix worldViewProjectionMatrix = cameraSpaceMatrix * camera.ViewProjectionMatrix;

        // get light position, translated to camera space, TODO : light position is currently in the solar system center
        Vector3 lightPosition = (Vector3)(camera.Position - new Position3(0, 0, 0));

        // get light direction - "space" doesn't matter here since it's just a direction
        Vector3 lightDirection = (Vector3)(worldSpacePosition - new Position3(0, 0, 0));
        lightDirection.Normalize();

        // determine which shader to use
        double altitude = mainCamera.fLocalPosition.Length();

        Effect effect;
        if (altitude <= Constants.EarthAtmosphereRadius)
          effect = planetEffectAtmosphereAtmosphere;
        else
          effect = planetEffectAtmosphereSpace;


        effect.Parameters["WorldViewProj"].SetValue(worldViewProjectionMatrix);

        // set up atmosphere parameters
        groundFromSpace.ResolveParameters(effect);


        float cameraHeight = (float)(camera.Position - sphere.Position).Length();
        Vector3 cameraPositionNode = (camera.Position - sphere.Position).AsVector3;

        Vector3 vLightDirection = (Vector3)(Position3.Zero - camera.Position);
        vLightDirection.Normalize();

        effect.Parameters["PatchPosition"].SetValue(terrainNode.Position.AsVector3);
        effect.Parameters["v3CameraPos"].SetValue(cameraPositionNode);
        effect.Parameters["v3LightPos"].SetValue(vLightDirection);
        effect.Parameters["fCameraHeight"].SetValue(cameraHeight);
        effect.Parameters["fCameraHeight2"].SetValue(cameraHeight * cameraHeight);


        effect.CurrentTechnique.Passes[0].Apply();

        GraphicsDevice.SetVertexBuffer(terrainNode.VertexBuffer.VertexBuffer);
        GraphicsDevice.Indices = TerrainNodeIndexBuffer.Indices;
        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, terrainNode.VertexBuffer.VertexCount, 0, TerrainNodeIndexBuffer.IndexCount / 3);

        //DrawNodeFrustum(terrainNode);
      }
    }


    private void DrawAtmosphere(Sphere sphere)
    {
      // get frustum camera position in planet space
      Camera frustumCamera = cameraManager.ActiveFrustumCamera;
      Position3 frustumCameraPosition = frustumCamera.Position - sphere.Position;

      // update sphere, allowing it to split
      sphere.Update(frustumCameraPosition, MathHelper.ToRadians(frustumCamera.FieldOfView));

      if (fillMode == FillMode.Solid)
        GraphicsDevice.RasterizerState = CullClockwiseSolid;
      else
        GraphicsDevice.RasterizerState = CullClockwiseWireFrame;

      GraphicsDevice.DepthStencilState = DepthNoWrite;
      GraphicsDevice.BlendState = BlendState.NonPremultiplied;

      //CalculateProjection(sphere);


      Camera camera = mainCamera;


      // get frustum camera position in planet space
      Position3 cameraPosition = camera.Position - sphere.Position;

      // calculate horizon angle, using camera position in planet space
      float horizonAngle = sphere.CalculateHorizonAngle(cameraPosition);

      Globals.DrawLevel = 0;
      Globals.DrawCount = 0;
      Globals.HorizonCullCount = 0;
      Globals.FrustumCullCount = 0;

      DrawAtmosphereNode(sphere.Front, cameraPosition, horizonAngle, sphere, camera);
      DrawAtmosphereNode(sphere.Back, cameraPosition, horizonAngle, sphere, camera);
      DrawAtmosphereNode(sphere.Left, cameraPosition, horizonAngle, sphere, camera);
      DrawAtmosphereNode(sphere.Right, cameraPosition, horizonAngle, sphere, camera);
      DrawAtmosphereNode(sphere.Top, cameraPosition, horizonAngle, sphere, camera);
      DrawAtmosphereNode(sphere.Bottom, cameraPosition, horizonAngle, sphere, camera);
    }
#endif


#if planet
    private void DrawNodeBoundingBox(TerrainNode node)
    {
      // calculate the width of the patch; first we need a vertex on both sides of the patch, in planet space
      Vector3 p1 = node.Position.AsVector3 + node.MinVertex;
      Vector3 p2 = node.Position.AsVector3 + node.MaxVertex;


      // which then lets us create the bounding frustum
      BoundingBox box = new BoundingBox(p1, p2);

      Vector3[] corners = box.GetCorners();

      // draw lines between corners
      VertexPositionColor[] vertices = new VertexPositionColor[24];


      // line 1
      vertices[0] = new VertexPositionColor(corners[0], Color.White);
      vertices[1] = new VertexPositionColor(corners[1], Color.White);

      // line 2
      vertices[2] = new VertexPositionColor(corners[1], Color.White);
      vertices[3] = new VertexPositionColor(corners[2], Color.White);

      // line 3
      vertices[4] = new VertexPositionColor(corners[2], Color.White);
      vertices[5] = new VertexPositionColor(corners[3], Color.White);

      // line 4
      vertices[6] = new VertexPositionColor(corners[3], Color.White);
      vertices[7] = new VertexPositionColor(corners[0], Color.White);


      // line 5
      vertices[8] = new VertexPositionColor(corners[4], Color.White);
      vertices[9] = new VertexPositionColor(corners[5], Color.White);

      // line 6
      vertices[10] = new VertexPositionColor(corners[5], Color.White);
      vertices[11] = new VertexPositionColor(corners[6], Color.White);

      // line 7
      vertices[12] = new VertexPositionColor(corners[6], Color.White);
      vertices[13] = new VertexPositionColor(corners[7], Color.White);

      // line 8
      vertices[14] = new VertexPositionColor(corners[7], Color.White);
      vertices[15] = new VertexPositionColor(corners[4], Color.White);


      // line 9
      vertices[16] = new VertexPositionColor(corners[0], Color.White);
      vertices[17] = new VertexPositionColor(corners[4], Color.White);

      // line 10
      vertices[18] = new VertexPositionColor(corners[1], Color.White);
      vertices[19] = new VertexPositionColor(corners[5], Color.White);

      // line 11
      vertices[20] = new VertexPositionColor(corners[2], Color.White);
      vertices[21] = new VertexPositionColor(corners[6], Color.White);

      // line 12
      vertices[22] = new VertexPositionColor(corners[3], Color.White);
      vertices[23] = new VertexPositionColor(corners[7], Color.White);



      GraphicsDevice device = graphics.GraphicsDevice;
      device.RasterizerState = RasterizerState.CullNone;

      //device.VertexDeclaration = positionColor;


      // get world space position for drawing the frustum
      Position3 worldSpacePosition = sphere.Position;

      // translate to camera space by subtracting the camera position, scale by planet scale
      Position3 cameraSpacePosition = (worldSpacePosition - mainCamera.Position) * sphere.Scale;

      // create world matrix
      Matrix cameraSpaceMatrix = sphere.ScaleMatrix * Matrix.CreateTranslation(cameraSpacePosition.AsVector3);


      testEffect.Projection = mainCamera.ProjectionMatrix;
      testEffect.World = cameraSpaceMatrix;
      testEffect.View = mainCamera.ViewMatrix;
      //testEffect.EnableDefaultLighting();
      testEffect.VertexColorEnabled = true;

      testEffect.CurrentTechnique.Passes[0].Apply();
      GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 12);

    }

    private void DrawNodeFrustum(TerrainNode node)
    {

      // calculate the width of the patch; first we need a vertex on both sides of the patch, in planet space
      Vector3 p1 = node.Position.AsVector3 + node.VertexBuffer.Vertices[1 * 33 + 1].Position;
      Vector3 p2 = node.Position.AsVector3 + node.VertexBuffer.Vertices[1 * 33 + 33].Position;

      // need to move them to the sphere radius
      p1.Normalize();
      p1 *= (float)sphere.Radius;

      p2.Normalize();
      p2 *= (float)sphere.Radius;

      // now we can calculate the opposite and hypotenuse lengths
      float opposite = Vector3.Distance(p1, p2) * 0.5f;    // width is distance between p1 and p2, and we need just half of it
      float hypotenuse = (float)sphere.Radius;

      // once we have the width we can calculate the field of view
      float fieldOfView = (float)(2.0 * Math.Asin(opposite / hypotenuse));

      // now we can create the view and projection matrixes
      Matrix view = Matrix.CreateLookAt(Vector3.Zero, node.Position.AsVector3, Vector3.Up);
      Matrix projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, 1.0f, 1.0f, 9000.0f);

      // which then lets us create the bounding frustum
      BoundingFrustum frustum = new BoundingFrustum(view * projection);

      Vector3[] corners = frustum.GetCorners();

      // draw lines between corners

      VertexPositionColor[] vertices = new VertexPositionColor[24];

      Position3 p = Position3.Zero;

      Position3 p0 = node.Position;
      p0.Normalize();
      p0 *= 9000.0f;


      // line 1
      vertices[0] = new VertexPositionColor(corners[0], Color.White);
      vertices[1] = new VertexPositionColor(corners[1], Color.White);

      // line 2
      vertices[2] = new VertexPositionColor(corners[1], Color.White);
      vertices[3] = new VertexPositionColor(corners[2], Color.White);

      // line 3
      vertices[4] = new VertexPositionColor(corners[2], Color.White);
      vertices[5] = new VertexPositionColor(corners[3], Color.White);

      // line 4
      vertices[6] = new VertexPositionColor(corners[3], Color.White);
      vertices[7] = new VertexPositionColor(corners[0], Color.White);


      // line 5
      vertices[8] = new VertexPositionColor(corners[4], Color.White);
      vertices[9] = new VertexPositionColor(corners[5], Color.White);

      // line 6
      vertices[10] = new VertexPositionColor(corners[5], Color.White);
      vertices[11] = new VertexPositionColor(corners[6], Color.White);

      // line 7
      vertices[12] = new VertexPositionColor(corners[6], Color.White);
      vertices[13] = new VertexPositionColor(corners[7], Color.White);

      // line 8
      vertices[14] = new VertexPositionColor(corners[7], Color.White);
      vertices[15] = new VertexPositionColor(corners[4], Color.White);


      // line 9
      vertices[16] = new VertexPositionColor(corners[0], Color.White);
      vertices[17] = new VertexPositionColor(corners[4], Color.White);

      // line 10
      vertices[18] = new VertexPositionColor(corners[1], Color.White);
      vertices[19] = new VertexPositionColor(corners[5], Color.White);

      // line 11
      vertices[20] = new VertexPositionColor(corners[2], Color.White);
      vertices[21] = new VertexPositionColor(corners[6], Color.White);

      // line 12
      vertices[22] = new VertexPositionColor(corners[3], Color.White);
      vertices[23] = new VertexPositionColor(corners[7], Color.White);



      GraphicsDevice device = graphics.GraphicsDevice;
      // TODO : device.RenderState.CullMode = CullMode.None;


      // get world space position for drawing the frustum
      Position3 worldSpacePosition = sphere.Position;

      // translate to camera space by subtracting the camera position, scale by planet scale
      Position3 cameraSpacePosition = (worldSpacePosition - mainCamera.Position) * sphere.Scale;

      // create world matrix
      Matrix cameraSpaceMatrix = sphere.ScaleMatrix * Matrix.CreateTranslation(cameraSpacePosition.AsVector3);


      testEffect.Projection = mainCamera.ProjectionMatrix;
      testEffect.World = cameraSpaceMatrix;
      testEffect.View = mainCamera.ViewMatrix;
      //testEffect.EnableDefaultLighting();
      testEffect.VertexColorEnabled = true;


      testEffect.CurrentTechnique.Passes[0].Apply();
      GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 12);
    }



    private void DrawTankTriangle()
    {
      if (tank.NodeUnderTank == null) return;
      if (tank.Triangle == null) return;

      // set up line - vertices are in patch space
      for (int i = 0; i < tankTriangle.Length; i++)
        tankTriangle[i].Color = Color.LightGreen;

      tankTriangle[0].Position = tank.Triangle[0]; tankTriangle[0].Color = Color.Red;
      tankTriangle[1].Position = tank.Triangle[1]; tankTriangle[1].Color = Color.Red;
      tankTriangle[2].Position = tank.Triangle[1]; tankTriangle[2].Color = Color.Green;
      tankTriangle[3].Position = tank.Triangle[3]; tankTriangle[3].Color = Color.Green;
      tankTriangle[4].Position = tank.Triangle[3]; tankTriangle[4].Color = Color.Blue;
      tankTriangle[5].Position = tank.Triangle[2]; tankTriangle[5].Color = Color.Blue;
      tankTriangle[6].Position = tank.Triangle[2]; tankTriangle[6].Color = Color.Yellow;
      tankTriangle[7].Position = tank.Triangle[0]; tankTriangle[7].Color = Color.Yellow;

      tankTriangle[8].Position = tank.Triangle[0]; tankTriangle[8].Color = Color.Purple;
      tankTriangle[9].Position = tank.Triangle[4]; tankTriangle[9].Color = Color.Purple;

      //tankTriangle[6].Position = tank.Triangle[2];
      //tankTriangle[7].Position = tank.Triangle[1];
      //tankTriangle[8].Position = tank.Triangle[1];
      //tankTriangle[9].Position = tank.Triangle[3];
      //tankTriangle[10].Position = tank.Triangle[3];
      //tankTriangle[11].Position = tank.Triangle[2];

      // the vertices are in patch space - we need to get them to camera space

      // get world space position by adding terrain node position to planet position
      Position3 worldSpacePosition = sphere.Position + tank.NodeUnderTank.Position;

      // translate to camera space by subtracting the camera position, scale by planet scale
      Position3 cameraSpacePosition = (worldSpacePosition - mainCamera.Position) * sphere.Scale;

      Matrix cameraSpaceMatrix = sphere.ScaleMatrix * Matrix.CreateTranslation(cameraSpacePosition.AsVector3);


      GraphicsDevice device = graphics.GraphicsDevice;
      // TODO : device.RenderState.CullMode = CullMode.None;
      // TODO : device.RenderState.DepthBias = 1f;

      testEffect.Projection = mainCamera.ProjectionMatrix;
      testEffect.World = cameraSpaceMatrix;
      testEffect.View = mainCamera.ViewMatrix;
      testEffect.VertexColorEnabled = true;

      testEffect.CurrentTechnique.Passes[0].Apply();
      GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, tankTriangle, 0, 5);

      // TODO : device.RenderState.DepthBias = 0;
    }
#endif

#if sun
    private void DrawSunGlow()
    {

      /*
      GraphicsDevice Device = this.GraphicsDevice;
      Camera C = cameraManager.ActiveCamera;

      Device.RenderState.DepthBufferWriteEnable = false;
      Device.RenderState.DepthBufferEnable = false;
      Device.RenderState.AlphaBlendEnable = true;
      Device.RenderState.SourceBlend = Blend.SourceAlpha;
      Device.RenderState.DestinationBlend = Blend.One;

      // get star position in view space by subtracting the camera position
      Position3 P = sun.Position - C.Position;

      Matrix W = Matrix.CreateScale(1.6f);
      W *= Matrix.CreateBillboard((Vector3)P, Vector3.Zero,
                                  (Vector3)C.fUp,
                                  (Vector3)C.fForward);


      Matrix O = Matrix.CreateFromQuaternion(C.Orientation);
      Matrix T = Matrix.Identity;
      Matrix V = Matrix.Invert(O * T);

      sunGlowEffect.Parameters["WorldViewProjectionMatrix"].SetValue(W * V * C.ProjectionMatrix);
      sunGlowEffect.Parameters["Billboard"].SetValue(sunGlowTexture);
      sunGlowEffect.Parameters["MaskColor"].SetValue(0.0f);

      sunGlowEffect.Begin();

      for (int I = 0; I < sunGlowEffect.CurrentTechnique.Passes.Count; I++)
      {
        EffectPass Pass = sunGlowEffect.CurrentTechnique.Passes[I];
        Pass.Begin();

        Device.VertexDeclaration = billboardVertexDeclaration;
        Device.Vertices[0].SetSource(sunVertexBuffer, 0, VertexPositionTexture.SizeInBytes);
        Device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
        Device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);

        Pass.End();
      }

      sunGlowEffect.End();
     */
    }
#endif

    protected override void Draw(GameTime gameTime)
    {
      // update the terrain node split manager - this is responsible for
      // generating new terrain nodes using the gpu for the geometry map
      // and a separate thread to build the final terrain mesh
      TerrainNodeSplitManager.Execute();


#if lensflare
      // create lens flare data
      lensFlare.IntensityMapBegin();
      DrawSphereMask(sphere);
      //tank.DrawMask(sphere, cameraManager.ActiveCamera);
      lensFlare.IntensityMapEnd();


      // switch back to normal camera
      cameraManager.ActivateCamera("Main");
#endif


      GraphicsDevice.Clear(Color.Black);
      spaceDome.Draw();

#if sun
      GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
      DrawSun(sun);
      DrawSunGlow();
#endif


      // draw planet sphere
      GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

#if planet
      DrawSphere(sphere);
#endif

#if atmosphere
      DrawAtmosphere(atmosphere);
#endif

      // draw the triangle that the tank is over and using for its orientation calculations
      // DrawTankTriangle();

      // draw frustum
      //DrawNodeBoundingBox(sphere.Left);


      // draw tank
#if planet
      GraphicsDevice.DepthStencilState = DepthStencilState.Default;
      GraphicsDevice.BlendState = BlendState.Opaque;
      GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
      tank.Draw(sphere, mainCamera);
#endif


#if lensflare
      lensFlare.Draw(gameTime);

      // draw intensity map for debugging
      //lensFlare.DrawIntensityMap();
#endif



      // text
      if (showHud)
      {
        GraphicsDevice.BlendState = BlendState.AlphaBlend;
        GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

        spriteBatch.Begin();

        Vector2 p = new Vector2(6, 10);

        spriteBatch.DrawString(debugTextFont, String.Format("FPS: {0:###0}", frameRate.FPS), p, Color.White);
        p.Y += 20;

        spriteBatch.DrawString(debugTextFont, String.Format("Draw Count: {0}", Globals.DrawCount), p, Color.White);
        p.Y += 20;

        spriteBatch.DrawString(debugTextFont, String.Format("Horizon Cull: {0}", Globals.HorizonCullCount), p, Color.White);
        p.Y += 20;

        spriteBatch.DrawString(debugTextFont, String.Format("Frustum Cull: {0}", Globals.FrustumCullCount), p, Color.White);
        p.Y += 20;

        spriteBatch.DrawString(debugTextFont, String.Format("Level: {0}", Globals.DrawLevel), p, Color.White);
        p.Y += 20;

        spriteBatch.DrawString(debugTextFont, String.Format("GeoQueue: {0}", Globals.GeometryQueueDepth), p, Color.White);
        p.Y += 20;

        spriteBatch.DrawString(debugTextFont, String.Format("HeightQueue: {0}", Globals.HeightmapQueueDepth), p, Color.White);
        p.Y += 20;

        spriteBatch.DrawString(debugTextFont, String.Format("TexQueue: {0}", Globals.TextureQueueDepth), p, Color.White);
        p.Y += 20;

        spriteBatch.DrawString(debugTextFont, String.Format("MeshQueue: {0}", Globals.MeshQueueDepth), p, Color.White);
        p.Y += 20;

        spriteBatch.DrawString(debugTextFont, String.Format("Nodes: {0}", Globals.NodeCount), p, Color.White);
        p.Y += 20;

#if planet
        double altitude = mainCamera.fLocalPosition.Length() - sphere.Radius;
#else
        double altitude = 0.0;
#endif

        spriteBatch.DrawString(debugTextFont, String.Format("Altitude: {0:#,##0.000}", altitude), p, Color.White);
        p.Y += 20;

        if (mainCamera.fLocalPosition.Length() > Constants.EarthAtmosphereRadius)
          spriteBatch.DrawString(debugTextFont, "Space", p, Color.White);
        else
          spriteBatch.DrawString(debugTextFont, "Atmosphere", p, Color.White);

        p.Y += 20;

        if (tank.Movement.AcceptInput)
          spriteBatch.DrawString(debugTextFont, "Tank Control", p, Color.White);
        else
          spriteBatch.DrawString(debugTextFont, "Camera Control", p, Color.White);

        p.Y += 20;

        spriteBatch.DrawString(debugTextFont, String.Format("TriangleIndex: {0}", Globals.TriangleIndex), p, Color.White);
        p.Y += 20;

        spriteBatch.End();

        GraphicsDevice.BlendState = BlendState.Opaque;
      }

      base.Draw(gameTime);

      Globals.FrameNumber++;
    }
  }
}
