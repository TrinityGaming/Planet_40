using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Willow.Input;

namespace Willow
{

  public class Movement
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

    public float fSpeedFactor = 0.001f; //  1.0f;

    // current acceleration - acceleration being applied based on input, gravity, etc.
    private Vector3 fRotationalAcceleration = Vector3.Zero;
    private Vector3 fLinearAcceleration = Vector3.Zero;

    // velocity
    private Vector3 fRotationalVelocity = Vector3.Zero;
    public Vector3 fLinearVelocity = Vector3.Zero;
    


    // matrices and quaternions
    private Matrix fWorldMatrix;
    private Quaternion fOrientation;

    public bool AcceptInput = false;


    public Movement(Game game)
    {
      fInputManager = (IInputManager)game.Services.GetService(typeof(IInputManager));
      Initialize();
    }


    private void Initialize()
    {
      // initialize orientation
      fOrientation = Quaternion.CreateFromYawPitchRoll(0.0f, MathHelper.ToRadians(-90.0f), 0.0f);
      UpdateWorldMatrix();
    }


    public Matrix WorldMatrix { get { return fWorldMatrix; } }
    public Quaternion Orientation { get { return fOrientation; } set { fOrientation = value; } }


    public void UpdateWorldMatrix()
    {
      fWorldMatrix = Matrix.CreateFromQuaternion(fOrientation);
    }



    public void LookAt(Position3 Target)
    {
      Target -= Position;
      SetWorldMatrix(Matrix.CreateLookAt(Vector3.Zero, (Vector3)Target, (Vector3)fUp));
    }


    public void SetWorldMatrix(Matrix V)
    {
      fWorldMatrix = V;

      Vector3 Scale;
      Vector3 Translation;
      fWorldMatrix.Decompose(out Scale, out fOrientation, out Translation);

      fOrientation = Quaternion.Inverse(fOrientation);

      // transform the local reference vectors to get the world vectors
      Vector3.Transform(ref fForwardLocal, ref fOrientation, out fForward);
      Vector3.Transform(ref fRightLocal, ref fOrientation, out fRight);
      Vector3.Transform(ref fUpLocal, ref fOrientation, out fUp);
    }



    public void Clone(Movement movement)
    {
      Orientation = movement.Orientation;
      fLocalPosition = movement.fLocalPosition;
      UpdateMovement(0);
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

    public void OrientUp(Vector3 up)
    {
      Matrix o = WorldMatrix;

      // build the orientation we want to be at
      o.Up = up;
      o.Right = Vector3.Cross(o.Forward, o.Up);
      o.Right = Vector3.Normalize(o.Right);
      o.Forward = Vector3.Cross(o.Up, o.Right);
      o.Forward = Vector3.Normalize(o.Forward);

      // create a target quaternion
      Quaternion newOrientation = Quaternion.CreateFromRotationMatrix(o);


      // slerp towards that orientation
      fOrientation = Quaternion.Slerp(fOrientation, newOrientation, 0.5f);
      UpdateOrientation();
      UpdateWorldMatrix();
    }

    private void HandleInput(float Elapsed)
    {
      if (!AcceptInput) return;

      if (fInputManager.KeyDown(Keys.D))
      {
        MoveRight();
      }

      if (fInputManager.KeyDown(Keys.A))
      {
        MoveLeft();
      }

      if (fInputManager.KeyDown(Keys.W))
      {
        MoveForward();
      }

      if (fInputManager.KeyDown(Keys.S))
      {
        MoveBackward();
      }

      if (fInputManager.KeyDown(Keys.Q))
      {
        RollLeft();
      }

      if (fInputManager.KeyDown(Keys.E))
      {
        RollRight();
      }

      if (fInputManager.KeyDown(Keys.Insert))
        MoveUp();

      if (fInputManager.KeyDown(Keys.Delete))
        MoveDown();


      if (fInputManager.KeyDown(Keys.Left))
        YawLeft();

      if (fInputManager.KeyDown(Keys.Right))
        YawRight();


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

    private void UpdateOrientation()
    {
      // transform the local reference vectors to get the world vectors
      Vector3.Transform(ref fForwardLocal, ref fOrientation, out fForward);
      Vector3.Transform(ref fRightLocal, ref fOrientation, out fRight);
      Vector3.Transform(ref fUpLocal, ref fOrientation, out fUp);

      // TODO : not positive this needs to be done
      fForward.Normalize();
      fRight.Normalize();
      fUp.Normalize();
    }


    public void UpdateMovement(float elapsed)
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

      UpdateOrientation();

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
      UpdateWorldMatrix();
    }

    public void Update(GameTime gameTime)
    {
      float Elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

      HandleInput(Elapsed);
      UpdateMovement(Elapsed);
    }

  }
}


