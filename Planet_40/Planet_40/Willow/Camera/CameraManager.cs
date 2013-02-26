using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace Willow.Camera
{
  public interface ICameraManager
  {
    Camera CreateCamera(string Name);
    void AddCamera(string Name, Camera C);
    void RemoveCamera(string Name);
    Camera GetCamera(string Name);
    void ActivateCamera(string Name);
    void ActivateFrustumCamera(string Name);

    Camera ActiveCamera { get; }
    Camera ActiveFrustumCamera { get; }
  }


  public class CameraManager : Microsoft.Xna.Framework.GameComponent, ICameraManager
  {
    private GraphicsDeviceManager fGraphics;
    private Dictionary<String, Camera> fCameras;
    private Camera fActiveCamera;
    private Camera fActiveFrustumCamera;

    public static ICameraManager CreateCameraManager (Game game)
    {
      CameraManager manager = new CameraManager(game);
      manager.UpdateOrder = 0;
      game.Components.Add(manager);
      return manager as ICameraManager;
    }


    public CameraManager(Game game) : base(game)
    {
      game.Services.AddService(typeof(ICameraManager), this);

      fGraphics = (GraphicsDeviceManager)game.Services.GetService(typeof(IGraphicsDeviceManager));
      fCameras = new Dictionary<String, Camera>();
      fActiveCamera = null;
      fActiveFrustumCamera = null;
    }

    public Camera ActiveCamera { get { return fActiveCamera; } }
    public Camera ActiveFrustumCamera { get { return fActiveFrustumCamera; } }

    public override void Initialize()
    {
      foreach (KeyValuePair<String, Camera> K in fCameras)
        K.Value.Initialize();

      base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
      foreach (KeyValuePair<String, Camera> K in fCameras)
        if (K.Value.Enabled)
          K.Value.Update(gameTime);

      base.Update(gameTime);
    }

    public Camera CreateCamera(string Name)
    {
      Camera Result = new Camera(Game);
      AddCamera(Name, Result);
      return Result;
    }

    public void AddCamera(string Name, Camera C)
    {
      fCameras.Add(Name, C);

      // active this camera if it's the first one added
      if (fActiveCamera == null && fCameras.Count == 1)
      {
        fActiveCamera = C;
        fActiveCamera.AcceptInput = true;

        if (fActiveFrustumCamera == null)
          fActiveFrustumCamera = fActiveCamera;
      }
    }

    public void RemoveCamera(string Name)
    {
      Camera C = GetCamera(Name);
      if (fActiveCamera == C)
      {
        fActiveCamera = null;
        if (fActiveFrustumCamera == C)
          fActiveFrustumCamera = null;
      }

      fCameras.Remove(Name);
    }

    public Camera GetCamera(string Name)
    {
      Camera Result = null;
      fCameras.TryGetValue(Name, out Result);
      return Result;
    }

    public void ActivateCamera(string Name)
    {
      //if (fActiveCamera != null)
      //  fActiveCamera.AcceptInput = false;

      fActiveCamera = GetCamera(Name);

      if (fActiveFrustumCamera == null)
        fActiveFrustumCamera = fActiveCamera;

      //if (fActiveCamera != null)
      //  fActiveCamera.AcceptInput = true;
    }

    public void ActivateFrustumCamera(string Name)
    {
      fActiveFrustumCamera = GetCamera(Name);
    }


 }
}