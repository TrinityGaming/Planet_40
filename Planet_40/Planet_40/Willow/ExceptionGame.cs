using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Willow
{
  public class ExceptionGame : Game
  {
    private const string errorTitle = "Unexpected Error";
    private const string errorMessage =
      "The game was shut down due to an unexpected error. We are sorry for the inconvenience.";

    private static readonly string[] errorButtons = new[]
	    {
		    "Exit to Dashboard",
		    "View Error Details"
	    };

    private readonly Exception exception;
    private bool shownMessage;
    private bool displayException;
    private SpriteBatch batch;
    private SpriteFont font;


    public ExceptionGame(Exception e)
    {
      new GraphicsDeviceManager(this)
      {
        PreferredBackBufferWidth = 1280,
        PreferredBackBufferHeight = 720
      };
      exception = e;

      Content.RootDirectory = "Content";
    }

    protected override void LoadContent()
    {
      batch = new SpriteBatch(GraphicsDevice);
      font = Content.Load<SpriteFont>("ErrorFont");
    }

    protected override void Update(GameTime gameTime)
    {
      // Globals.GamerServices.Update(gameTime);

      // check for back button on any controller
      if (shownMessage)
        for (PlayerIndex index = PlayerIndex.One; index <= PlayerIndex.Four; index++)
          if (GamePad.GetState(index).Buttons.Back == ButtonState.Pressed)
          {
            this.Exit();
            break;
          }


      if (!shownMessage)
      {
        try
        {
          if (!Guide.IsVisible)
          {
            Guide.BeginShowMessageBox(
              errorTitle,
              errorMessage,
              errorButtons,
              0,
              MessageBoxIcon.Error,
              result =>
              {
                int? choice = Guide.EndShowMessageBox(result);

                if (choice.HasValue && choice.Value == 1)
                  displayException = true;
                else
                  Exit();
              },
              null);
            shownMessage = true;
          }
        }
        catch { }
      }

      base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
      GraphicsDevice.Clear(Color.Black);

      if (displayException)
      {

        Vector2 p = new Vector2(
          GraphicsDevice.Viewport.TitleSafeArea.X,
          GraphicsDevice.Viewport.TitleSafeArea.Y + 60);
        batch.Begin();
        batch.DrawString(font, "Press Back to Exit", new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X,
                                                                 GraphicsDevice.Viewport.TitleSafeArea.Y), Color.White);
        batch.DrawString(font, exception.ToString(), p, Color.White);
        batch.End();
      }

      base.Draw(gameTime);
    }
  }

}
