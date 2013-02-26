using System;
using Microsoft.Xna.Framework;


namespace Willow
{

  public partial class FrameRate : Microsoft.Xna.Framework.DrawableGameComponent
  {
    private float deltaTime;
    private float frameCount;
    private float frameRate;

    public static FrameRate CreateFrameRate(Game game)
    {
      FrameRate frameRate = new FrameRate(game);
      frameRate.UpdateOrder = 1;
      game.Components.Add(frameRate);

      return frameRate;
    }

    public FrameRate(Game game)
      : base(game)
    {
    }

    public float FPS { get { return frameRate; } }


    public override void Draw(GameTime gameTime)
    {
      float Elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds; 

      deltaTime += Elapsed;
      frameCount++;

      if (deltaTime >= 1.0f)
      {
        frameRate = frameCount;
        frameCount = 0;
        deltaTime -= 1.0f;
      }

      base.Draw(gameTime);
    }
  }
}


