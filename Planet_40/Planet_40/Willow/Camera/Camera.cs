using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Willow.Input;

namespace Willow.Camera
{

  public partial class Camera : Microsoft.Xna.Framework.GameComponent
  {
    private IInputManager fInputManager;


    // local orientation
    public Vector3 fUpLocal = new Vector3(0, 1, 0);             // local up vector
    public Vector3 fForwardLocal = new Vector3(0, 0, -1);       // local forward vector
    public Vector3 fRightLocal = new Vector3(1, 0, 0);          // local right vector

    // world orientation
    public Vector3 fUp;                                         // rotated up vector
    public Vector3 fForward;                                     // rotated forward vector
    public Vector3 fRight;                                       // rotated right vector
    public Position3 Position = new Position3(0, 0, 6436.1369f);    // camera position
    public Position3 fLocalPosition;
    public Position3 AttachedPosition;


    // acceleration forces
    private Vector3 fRotationalForce = new Vector3(955.0f, 955.0f, 955.0f);
    //private Vector3 fRotationalForce = new Vector3(55.0f, 55.0f, 55.0f);
    private Vector3 fRotationalDrag = new Vector3(0.85f, 0.85f, 0.85f);
    private float fForwardForce = 10.0f;
    private float fReverseForce = 10.0f;
    private float fLinearDrag = 0.90f;
    private float fRotationTime = 0.0f;
    private float fLinearTime = 0.0f;

    public float fSpeedFactor = 1.0f; // 0.000001f; //  1.0f;

    // current acceleration - acceleration being applied based on input, gravity, etc.
    private Vector3 fRotationalAcceleration = Vector3.Zero;
    private Vector3 fLinearAcceleration = Vector3.Zero;

    // velocity
    private Vector3 fRotationalVelocity = Vector3.Zero;
    private Vector3 fLinearVelocity = Vector3.Zero;



    // matrices and quaternions
    private Matrix fViewMatrix;
    private Quaternion fOrientation;
    private Matrix fProjectionMatrix;
    private Matrix fViewProjectionMatrix;

    public BoundingFrustum fFrustum;

    // projection
    private float fNearClip = 0.1f;
    private float fFarClip = 20000.0f; // 1000.0f;
    float fFieldOfView = 60.0f;
    public bool AcceptInput = false;
    public Viewport fViewport;


    // camera attachment
#if ship
    public Position3 fChasePosition;
    public Position3 fChaseDirection;
    public Position3 fChaseUp;
    public Quaternion fChaseOrientation;
    public float fChaseSpeed;
    private Position3 fDesiredPositionOffset = new Position3(0, 0.0001f, 0.0f);
    private Position3 fLookAtOffset = new Position3(0, 0.0000f, 0);
    private Position3 fDesiredPosition;
    private Position3 fLookAt;
    
    /*
    private float fStiffness = 200.0f;
    private float fDamping = 25.0f;
    private float fMass = 1.0f;
    */
    public bool fFollow = false;
    public bool fChase = false;

    private float fSlerpTime = 0.0f;


#endif

    public Camera(Game game) : base(game)
    {
      fInputManager = (IInputManager)game.Services.GetService(typeof(IInputManager));
    }


    public override void Initialize()
    {
      base.Initialize();

      // initialize orientation
      fOrientation = Quaternion.CreateFromYawPitchRoll(0.0f, MathHelper.ToRadians(45.0f), 0.0f);
      UpdateViewMatrix();
      UpdateProjectionMatrix();
      UpdateCamera(0);
    }


    public Matrix ViewMatrix { get { return fViewMatrix; } }
    public Matrix ProjectionMatrix { get { return fProjectionMatrix; }}
    public Matrix ViewProjectionMatrix { get { return fViewProjectionMatrix; } }
    public Quaternion Orientation { get { return fOrientation; } set { fOrientation = value; } }
    public float FieldOfView { get { return fFieldOfView; } set { fFieldOfView = value; }}
    public BoundingFrustum Frustum { get { return fFrustum; } }
    public float NearClip { get { return fNearClip; } set { fNearClip = value; } }
    public float FarClip { get { return fFarClip; } set { fFarClip = value; } }

    public void UpdateProjectionMatrix()
    {
      fProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fFieldOfView),
                                                              fViewport.AspectRatio, fNearClip, fFarClip);

      fViewProjectionMatrix = fViewMatrix * fProjectionMatrix;
      fFrustum = new BoundingFrustum(fViewProjectionMatrix);
    }

    public void SetProjectionMatrix(Matrix P)
    {
      fProjectionMatrix = P;
      fViewProjectionMatrix = fViewMatrix * fProjectionMatrix;
      fFrustum = new BoundingFrustum(fViewProjectionMatrix);
    }

    public void UpdateViewMatrix()
    {
      Matrix O = Matrix.CreateFromQuaternion(fOrientation);
      fViewMatrix = Matrix.Invert(O);
      fViewProjectionMatrix = fViewMatrix * fProjectionMatrix;
      fFrustum = new BoundingFrustum(fViewProjectionMatrix);
    }



    public void LookAt(Position3 Target)
    {
      Target -= Position;

      fViewMatrix = Matrix.CreateLookAt(Vector3.Zero, (Vector3)Target, (Vector3)fUp);

      Vector3 Scale;
      Vector3 Translation;
      fViewMatrix.Decompose(out Scale, out fOrientation, out Translation);

      fOrientation = Quaternion.Inverse(fOrientation);

      // transform the local reference vectors to get the world vectors
      Vector3.Transform(ref fForwardLocal, ref fOrientation, out fForward);
      Vector3.Transform(ref fRightLocal, ref fOrientation, out fRight);
      Vector3.Transform(ref fUpLocal, ref fOrientation, out fUp);

      fViewProjectionMatrix = fViewMatrix * fProjectionMatrix;
      fFrustum = new BoundingFrustum(fViewProjectionMatrix);
    }

    public void SetViewMatrix(Matrix V)
    {
      fViewMatrix = V;

      Vector3 Scale;
      Vector3 Translation;
      fViewMatrix.Decompose(out Scale, out fOrientation, out Translation);

      fOrientation = Quaternion.Inverse(fOrientation);

      // transform the local reference vectors to get the world vectors
      Vector3.Transform(ref fForwardLocal, ref fOrientation, out fForward);
      Vector3.Transform(ref fRightLocal, ref fOrientation, out fRight);
      Vector3.Transform(ref fUpLocal, ref fOrientation, out fUp);

      fViewProjectionMatrix = fViewMatrix * fProjectionMatrix;
      fFrustum = new BoundingFrustum(fViewProjectionMatrix);
    }



    public void Clone(Camera camera)
    {
      Orientation = camera.Orientation;
      fLocalPosition = camera.fLocalPosition;
      UpdateCamera(0);
    }


    public void MoveUp()
    {
      fLinearAcceleration += fUp * fForwardForce * fSpeedFactor;
    }

    public void MoveDown()
    {
      fLinearAcceleration -= fUp * fForwardForce * fSpeedFactor;
    }


    public void MoveForward()
    {
      fLinearAcceleration += fForward * fForwardForce * fSpeedFactor;
    }

    public void MoveBackward()
    {
      fLinearAcceleration += fForward * -fReverseForce * fSpeedFactor;
    }

    public void MoveRight()
    {
      fLinearAcceleration += fRight * fForwardForce * fSpeedFactor;
    }

    public void MoveLeft()
    {
      fLinearAcceleration += fRight * -fForwardForce * fSpeedFactor;
    }

    public void YawLeft()
    {
      fRotationalAcceleration.Y = fRotationalForce.Y;
    }

    public void YawRight()
    {
      fRotationalAcceleration.Y = -fRotationalForce.Y;
    }

    public void SpeedChange(float Amount)
    {
      fSpeedFactor += (fSpeedFactor * Amount * 0.1f);
      if (fSpeedFactor < 0.000001f) fSpeedFactor = 0.000001f;
      if (fSpeedFactor > 100000) fSpeedFactor = 100000;
    }

    public void YawChange(float Amount)
    {
      fRotationalAcceleration.Y = Amount * 100.0f;
    }

    public void PitchChange(float Amount)
    {
      fRotationalAcceleration.X = Amount * 100.0f;
    }

    public void PitchUp()
    {
      fRotationalAcceleration.X = fRotationalForce.X;
    }

    public void PitchDown()
    {
      fRotationalAcceleration.X = -fRotationalForce.X;
    }

    public void RollLeft()
    {
      fRotationalAcceleration.Z = fRotationalForce.Z;
    }

    public void RollRight()
    {
      fRotationalAcceleration.Z = -fRotationalForce.Z;
    }


    private void HandleInput(float Elapsed)
    {
      if (!AcceptInput) return;

      if (fInputManager.KeyPressed(Keys.PageUp))
      {
        fFieldOfView -= 5.0f;
        if (FieldOfView < 10.0f)
          fFieldOfView = 10.0f;
      }

      if (fInputManager.KeyPressed(Keys.PageDown))
      {
        fFieldOfView += 5.0f;
        if (fFieldOfView > 130.0f)
          FieldOfView = 130.0f;
      }

      if (fInputManager.KeyPressed(Keys.End))
      {
        fFieldOfView = 60.0f;
      }

      if (fInputManager.KeyOrButtonDown(PlayerIndex.One, Keys.D, Buttons.DPadRight))
      {
        MoveRight();
      }

      if (fInputManager.KeyOrButtonDown(PlayerIndex.One, Keys.A, Buttons.DPadLeft))
      {
        MoveLeft();
      }

      if (fInputManager.KeyOrButtonDown(PlayerIndex.One, Keys.W, Buttons.DPadUp))
      {
        MoveForward();
      }

      if (fInputManager.KeyOrButtonDown(PlayerIndex.One, Keys.S, Buttons.DPadDown))
      {
        MoveBackward();
      }

      if (fInputManager.KeyOrButtonDown(PlayerIndex.One, Keys.Q, Buttons.RightShoulder))
      {
        RollLeft();
      }

      if (fInputManager.KeyOrButtonDown(PlayerIndex.One, Keys.E, Buttons.LeftShoulder))
      {
        RollRight();
      }

      if (fInputManager.KeyDown(Keys.Insert))
        MoveUp();

      if (fInputManager.KeyDown(Keys.Delete))
        MoveDown();


      // TODO : this is currently frame rate dependent

      // mouse yaw
      if (fInputManager.MouseDeltaX != 0)
        YawChange(-fInputManager.MouseDeltaX);


      // mouse pitch
      if (fInputManager.MouseDeltaY != 0)
        PitchChange(-fInputManager.MouseDeltaY);


      // scroll wheel speed change
      if (fInputManager.MouseDeltaScrollWheel != 0)
      {
        float C = fInputManager.MouseDeltaScrollWheel / 120.0f;
        SpeedChange(C);
      }
    }


    public void UpdateCamera(float elapsed)
    {
      fRotationalVelocity += (fRotationalAcceleration * elapsed);

      fRotationTime += elapsed;
      while (fRotationTime >= (1.0f / 60.0f))
      {
        fRotationalVelocity *= fRotationalDrag;
        fRotationTime -= (1.0f / 60.0f);
        fRotationalAcceleration = Vector3.Zero;
      }

      float Yaw = MathHelper.ToRadians(fRotationalVelocity.Y * elapsed);
      float Pitch = MathHelper.ToRadians(fRotationalVelocity.X * elapsed);
      float Roll = MathHelper.ToRadians(fRotationalVelocity.Z * elapsed);

      // create a rotation quaternion based on orientation changes - this is the amount to rotate
      Quaternion Rotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, Roll);

      // add the rotation to the current rotation quaternion
      fOrientation *= Rotation;

      // transform the local reference vectors to get the world vectors
      Vector3.Transform(ref fForwardLocal, ref fOrientation, out fForward);
      Vector3.Transform(ref fRightLocal, ref fOrientation, out fRight);
      Vector3.Transform(ref fUpLocal, ref fOrientation, out fUp);

      // TODO : not positive this needs to be done
      fForward.Normalize();
      fRight.Normalize();
      fUp.Normalize();

      // apply linear acceleration to velocity
      fLinearVelocity += fLinearAcceleration * elapsed;

      fLinearTime += elapsed;
      while (fLinearTime >= (1.0f / 60.0f))
      {
        fLinearVelocity *= fLinearDrag;
        fLinearTime -= (1.0f / 60.0f);
        fLinearAcceleration = Vector3.Zero;
      }


      fLocalPosition.Add(fLinearVelocity);
      Position = fLocalPosition + AttachedPosition;



      // update matrices
      UpdateViewMatrix();
    }

    public override void Update(GameTime gameTime)
    {
      float Elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

      HandleInput(Elapsed);
      UpdateCamera(Elapsed);

      base.Update(gameTime);
    }

    public void RenderFrustum(GraphicsDevice Device)
    {

      Vector3[] Corners = fFrustum.GetCorners();



    }
  }
}


