using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Willow.Input
{
  public interface IInputManager
  {
    bool KeyOrButtonDown(PlayerIndex player, Keys Key, Buttons Button);
    bool KeyOrButtonPressed(PlayerIndex player, Keys Key, Buttons Button);
    bool KeyOrButtonReleased(PlayerIndex player, Keys Key, Buttons Button);
    bool KeyOrButtonHeld(PlayerIndex player, Keys Key, Buttons Button);

    bool KeyDown(Keys Key);
    bool KeyPressed(Keys Key);
    bool KeyReleased(Keys Key);
    bool KeyHeld(Keys Key);
    bool KeyCombinationPressed(Keys Key1, Keys Key2);

    bool KeyDown(PlayerIndex player, Buttons Button);
    bool KeyPressed(PlayerIndex player, Buttons Button);
    bool KeyReleased(PlayerIndex player, Buttons Button);
    bool KeyHeld(PlayerIndex player, Buttons Button);
    bool KeyCombinationPressed(PlayerIndex player, Buttons Button1, Buttons Button2);

    Vector2 LeftThumbstickDPadVector(PlayerIndex player);
    Vector2 LeftThumbStickPosition(PlayerIndex player);
    Vector2 RightThumbStickPosition(PlayerIndex player);

    bool IsDisconnected(PlayerIndex player);
    void ResetButtons();

    bool MouseTrackDeltas { get; set; }
    float MouseDeltaX { get; }
    float MouseDeltaY { get; }
    int MouseX { get; }
    int MouseY { get; }
    float MouseDeltaScrollWheel { get; }

    bool LeftButtonDown { get; }
    bool RightButtonDown { get; }
    bool MiddleButtonDown { get; }
    bool LeftButtonPressed { get; }
    bool RightButtonPressed { get; }
    bool MiddleButtonPressed { get; }
    bool LeftButtonReleased { get; }
    bool RightButtonReleased { get; }
    bool MiddleButtonReleased { get; }
    bool LeftButtonHeld { get; }
    bool RightButtonHeld { get; }
    bool MiddleButtonHeld { get; }
  }


  public class InputManager : Microsoft.Xna.Framework.GameComponent, IInputManager
  {
    private GraphicsDeviceManager fGraphics;
    private KeyboardHandler fKeyboardHandler;
    private MouseHandler fMouseHandler;
    private GamePadHandler fGamePadHandler;



    public static IInputManager CreateInputManager(Game game)
    {
      InputManager manager = new InputManager(game);
      game.Components.Add(manager);
      return manager as IInputManager;
    }


    public InputManager(Game game)
      : base(game)
    {
      game.Services.AddService(typeof(IInputManager), this);

      fGraphics = (GraphicsDeviceManager)game.Services.GetService(typeof(IGraphicsDeviceManager));
      fKeyboardHandler = new KeyboardHandler(Game);
      fMouseHandler = new MouseHandler(Game);
      fGamePadHandler = new GamePadHandler(Game);
    }

    public MouseHandler Mouse { get { return fMouseHandler; } }

    public override void Initialize()
    {
      base.Initialize();
    }

    public void ResetButtons()
    {
      fGamePadHandler.Reset();
    }

    public override void Update(GameTime gameTime)
    {
      // update input device handlers
      fKeyboardHandler.Update(gameTime);
      fMouseHandler.Update(gameTime);
      fGamePadHandler.Update(gameTime);

#if DEBUG
      // exit on escape or back button
      if (KeyOrButtonDown(PlayerIndex.One, Keys.Escape, Buttons.Back))
        Game.Exit();
#endif

#if !XBOX
      // toggle full screen
      if (KeyCombinationPressed(Keys.LeftAlt, Keys.Enter))
        fGraphics.ToggleFullScreen();
#endif

      base.Update(gameTime);
    }

    // combined functions
    #region Combined Functions

    public bool KeyOrButtonDown(PlayerIndex player, Keys Key, Buttons Button)
    {
      return KeyDown(Key) || KeyDown(player, Button);
    }

    public bool KeyOrButtonPressed(PlayerIndex player, Keys Key, Buttons Button)
    {
      return KeyPressed(Key) || KeyPressed(player, Button);
    }

    public bool KeyOrButtonReleased(PlayerIndex player, Keys Key, Buttons Button)
    {
      return KeyReleased(Key) || KeyReleased(player, Button);
    }

    public bool KeyOrButtonHeld(PlayerIndex player, Keys Key, Buttons Button)
    {
      return KeyHeld(Key) || KeyHeld(player, Button);
    }



    #endregion

    // keyboard functions
    #region Keyboard Functions

    public bool KeyDown(Keys Key)
    {
      return fKeyboardHandler.KeyDown(Key);
    }

    public bool KeyPressed(Keys Key)
    {
      return fKeyboardHandler.KeyPressed(Key);
    }

    public bool KeyReleased(Keys Key)
    {
      return fKeyboardHandler.KeyReleased(Key);
    }

    public bool KeyHeld(Keys Key)
    {
      return fKeyboardHandler.KeyHeld(Key);
    }

    public bool KeyCombinationPressed(Keys Key1, Keys Key2)
    {
      return fKeyboardHandler.KeyCombinationPressed(Key1, Key2);
    }

    #endregion


    // mouse functions
    #region Mouse Functions

    public bool MouseTrackDeltas { get { return fMouseHandler.TrackDeltas; } set { fMouseHandler.TrackDeltas = value; } }
    public float MouseDeltaX { get { return fMouseHandler.DeltaX; } }
    public float MouseDeltaY { get { return fMouseHandler.DeltaY; } }
    public int MouseX { get { return fMouseHandler.MouseX; } }
    public int MouseY { get { return fMouseHandler.MouseY; } }
    public float MouseDeltaScrollWheel { get { return fMouseHandler.DeltaScrollWheel; } }

    public bool LeftButtonDown { get { return fMouseHandler.LeftButtonDown(); } }
    public bool RightButtonDown { get { return fMouseHandler.RightButtonDown(); } }
    public bool MiddleButtonDown { get { return fMouseHandler.MiddleButtonDown(); } }

    public bool LeftButtonPressed { get { return fMouseHandler.LeftButtonPressed(); } }
    public bool RightButtonPressed { get { return fMouseHandler.RightButtonPressed(); } }
    public bool MiddleButtonPressed { get { return fMouseHandler.MiddleButtonPressed(); } }

    public bool LeftButtonReleased { get { return fMouseHandler.LeftButtonReleased(); } }
    public bool RightButtonReleased { get { return fMouseHandler.RightButtonReleased(); } }
    public bool MiddleButtonReleased { get { return fMouseHandler.MiddleButtonReleased(); } }

    public bool LeftButtonHeld { get { return fMouseHandler.LeftButtonHeld(); } }
    public bool RightButtonHeld { get { return fMouseHandler.RightButtonHeld(); } }
    public bool MiddleButtonHeld { get { return fMouseHandler.MiddleButtonHeld(); } }

    #endregion

    // gamepad functions
    #region GamePad Functions

    public bool KeyDown(PlayerIndex player, Buttons Button)
    {
      return fGamePadHandler.KeyDown(player, Button);
    }

    public bool KeyPressed(PlayerIndex player, Buttons Button)
    {
      return fGamePadHandler.KeyPressed(player, Button);
    }

    public bool KeyReleased(PlayerIndex player, Buttons Button)
    {
      return fGamePadHandler.KeyReleased(player, Button);
    }

    public bool KeyHeld(PlayerIndex player, Buttons Button)
    {
      return fGamePadHandler.KeyHeld(player, Button);
    }

    public bool KeyCombinationPressed(PlayerIndex player, Buttons Button1, Buttons Button2)
    {
      return fGamePadHandler.KeyCombinationPressed(player, Button1, Button2);
    }

    public Vector2 LeftThumbStickPosition(PlayerIndex player)
    {
      return fGamePadHandler.LeftThumbStickPosition(player);
    }

    public Vector2 RightThumbStickPosition(PlayerIndex player)
    {
      return fGamePadHandler.RightThumbStickPosition(player);
    }

    public bool IsDisconnected(PlayerIndex player)
    {
      return fGamePadHandler.IsDisconnected(player);
    }


    public Vector2 LeftThumbstickDPadVector(PlayerIndex player)
    {
      Vector2 result = Vector2.Zero;

      if (KeyDown(player, Buttons.DPadUp))
        result.Y = -1;

      if (KeyDown(player, Buttons.DPadRight))
        result.X = 1;

      if (KeyDown(player, Buttons.DPadDown))
        result.Y = 1;

      if (KeyDown(player, Buttons.DPadLeft))
        result.X = -1;


      // normalize so up/right type combinations don't move extra fast
      if (result != Vector2.Zero)
        result.Normalize();


      // check thumbstick - this overrides the dpad
      Vector2 v = LeftThumbStickPosition(player);

      if (v != Vector2.Zero)
        result = v;

      return result;
    }

    #endregion

  }

  public class MouseHandler
  {
    private Game fGame;
    private MouseState fMouseState;
    private MouseState fPreviousMouseState;
    private MouseState fPreviousButtonState;
    private float fDeltaX;
    private float fDeltaY;
    private int fMouseX;
    private int fMouseY;
    private float fDeltaScrollWheel;
    private bool fWasActive = false;
    private bool fTrackDeltas = true;


    public MouseHandler(Game game)
    {
      fGame = game;

      Mouse.SetPosition(fGame.Window.ClientBounds.Width / 2, fGame.Window.ClientBounds.Height / 2);
      fMouseState = Mouse.GetState();
      fPreviousMouseState = fMouseState;
      fPreviousButtonState = fMouseState;
    }

    public bool TrackDeltas { get { return fTrackDeltas; } set { fTrackDeltas = value; } }
    public float DeltaX { get { return fDeltaX; } }
    public float DeltaY { get { return fDeltaY; } }
    public int MouseX { get { return fMouseX; } }
    public int MouseY { get { return fMouseY; } }
    public float DeltaScrollWheel { get { return fDeltaScrollWheel; } }


    public void Update(GameTime gameTime)
    {
      fDeltaX = 0;
      fDeltaY = 0;
      fDeltaScrollWheel = 0;

      // don't do anything if the game isn't active
      if (!fGame.IsActive)
      {
        fWasActive = false;
        return;
      }

      // if the game is active, and was active last frame, then just get the new mouse state
      // if it wasn't active the last frame then use the previous mouse state to avoid ugly
      // things happening when the game receives focus
      if (fWasActive)
      {
        if (!fTrackDeltas)
          fPreviousMouseState = fMouseState;
        fPreviousButtonState = fMouseState;
        fMouseState = Mouse.GetState();
      }
      else
        fMouseState = fPreviousMouseState;

      fWasActive = true;


      // calculate deltas
      if (fTrackDeltas)
      {
        fDeltaX = fMouseState.X - fPreviousMouseState.X;
        fDeltaY = fMouseState.Y - fPreviousMouseState.Y;

        // move mouse back to center of window
        Mouse.SetPosition(fGame.Window.ClientBounds.Width / 2, fGame.Window.ClientBounds.Height / 2);
      }
      else
      {
        fMouseX = fMouseState.X;
        fMouseY = fMouseState.Y;
      }

      fDeltaScrollWheel = fMouseState.ScrollWheelValue - fPreviousMouseState.ScrollWheelValue;

      if (fTrackDeltas)
        fPreviousMouseState = Mouse.GetState();
    }

    public bool LeftButtonDown()
    {
      return fMouseState.LeftButton == ButtonState.Pressed;
    }

    public bool RightButtonDown()
    {
      return fMouseState.RightButton == ButtonState.Pressed;
    }

    public bool MiddleButtonDown()
    {
      return fMouseState.MiddleButton == ButtonState.Pressed;
    }

    public bool LeftButtonPressed()
    {
      return fMouseState.LeftButton == ButtonState.Pressed &&
             fPreviousButtonState.LeftButton == ButtonState.Released;
    }

    public bool RightButtonPressed()
    {
      return fMouseState.RightButton == ButtonState.Pressed &&
             fPreviousButtonState.RightButton == ButtonState.Released;
    }

    public bool MiddleButtonPressed()
    {
      return fMouseState.MiddleButton == ButtonState.Pressed &&
             fPreviousButtonState.MiddleButton == ButtonState.Released;
    }

    public bool LeftButtonReleased()
    {
      return fMouseState.LeftButton == ButtonState.Released &&
             fPreviousButtonState.LeftButton == ButtonState.Pressed;
    }

    public bool RightButtonReleased()
    {
      return fMouseState.RightButton == ButtonState.Released &&
             fPreviousButtonState.RightButton == ButtonState.Pressed;
    }

    public bool MiddleButtonReleased()
    {
      return fMouseState.MiddleButton == ButtonState.Released &&
             fPreviousButtonState.MiddleButton == ButtonState.Pressed;
    }

    public bool LeftButtonHeld()
    {
      return fMouseState.LeftButton == ButtonState.Pressed &&
             fPreviousButtonState.LeftButton == ButtonState.Pressed;
    }

    public bool RightButtonHeld()
    {
      return fMouseState.RightButton == ButtonState.Pressed &&
             fPreviousButtonState.RightButton == ButtonState.Pressed;
    }

    public bool MiddleButtonHeld()
    {
      return fMouseState.MiddleButton == ButtonState.Pressed &&
             fPreviousButtonState.MiddleButton == ButtonState.Pressed;
    }
  }

  public class GamePadHandler
  {
    private Game fGame;
    private GamePadState[] gamePadState;
    private GamePadState[] previousGamePadState;
    private bool[] wasConnected;
    private bool[] isConnected;

    public GamePadHandler(Game game)
    {
      fGame = game;

      gamePadState = new GamePadState[4];
      previousGamePadState = new GamePadState[4];
      wasConnected = new bool[4];
      isConnected = new bool[4];

      // initialize the current and previous states
      for (int player = 0; player < 4; player++)
      {
        gamePadState[player] = GamePad.GetState((PlayerIndex)player);
        previousGamePadState[player] = gamePadState[player];
      }
    }

    public void Reset()
    {
      // preserve previous game pad state
      for (int i = 0; i < 4; i++)
        previousGamePadState[i] = gamePadState[i];
    }

    public void Update(GameTime gameTime)
    {
      // preserve previous game pad state
      for (int i = 0; i < 4; i++)
        previousGamePadState[i] = gamePadState[i];

      if (!fGame.IsActive) return;

      // get current game pad state
      for (int i = 0; i < 4; i++)
        gamePadState[i] = GamePad.GetState((PlayerIndex)i);


      for (int i = 0; i < 4; i++)
      {
        // keep track if this game pad was ever connected
        if (gamePadState[i].IsConnected)
          wasConnected[i] = true;

        // check for connect/reconnect state
        if (wasConnected[i])
          isConnected[i] = gamePadState[i].IsConnected;
      }
    }

    public bool IsDisconnected(PlayerIndex player)
    {
      return (wasConnected[(int)player] && !isConnected[(int)player]);
    }


    public bool KeyDown(PlayerIndex player, Buttons Button)
    {
      // return true if the key is down
      return (gamePadState[(int)player].IsButtonDown(Button));
    }


    public bool KeyPressed(PlayerIndex player, Buttons Button)
    {
      // return true if the key is down, and it was up before
      return (gamePadState[(int)player].IsButtonDown(Button) && previousGamePadState[(int)player].IsButtonUp(Button));
    }

    public bool KeyReleased(PlayerIndex player, Buttons Button)
    {
      // return true if the key is up, and it was down before
      return (gamePadState[(int)player].IsButtonUp(Button) && previousGamePadState[(int)player].IsButtonDown(Button));
    }

    public bool KeyHeld(PlayerIndex player, Buttons Button)
    {
      // return true if the key is down, and it was down before
      return (gamePadState[(int)player].IsButtonDown(Button) && previousGamePadState[(int)player].IsButtonDown(Button));
    }

    public bool KeyCombinationPressed(PlayerIndex player, Buttons Button1, Buttons Button2)
    {
      return (KeyHeld(player, Button1) && KeyPressed(player, Button2));
    }

    public Vector2 LeftThumbStickPosition(PlayerIndex player)
    {
      return gamePadState[(int)player].ThumbSticks.Left;
    }

    public Vector2 RightThumbStickPosition(PlayerIndex player)
    {
      return gamePadState[(int)player].ThumbSticks.Right;
    }
  }


  public class KeyboardHandler 
  {
    private Game fGame;
    private KeyboardState fKeyboardState;
    private KeyboardState fPreviousKeyboardState;

    public KeyboardHandler(Game game)
    {
      fGame = game;
      fKeyboardState = Keyboard.GetState();
      fPreviousKeyboardState = fKeyboardState;
    }


    public void Update(GameTime gameTime)
    {
      fPreviousKeyboardState = fKeyboardState;
      if (!fGame.IsActive) return;
      fKeyboardState = Keyboard.GetState();
    }


    public bool KeyDown(Keys Key)
    {
      // return true if the key is down
      return (fKeyboardState.IsKeyDown(Key));
    }


    public bool KeyPressed(Keys Key)
    {
      // return true if the key is down, and it was up before
      return (fKeyboardState.IsKeyDown(Key) && fPreviousKeyboardState.IsKeyUp(Key));
    }

    public bool KeyReleased(Keys Key)
    {
      // return true if the key is up, and it was down before
      return (fKeyboardState.IsKeyUp(Key) && fPreviousKeyboardState.IsKeyDown(Key));
    }

    public bool KeyHeld(Keys Key)
    {
      // return true if the key is down, and it was down before
      return (fKeyboardState.IsKeyDown(Key) && fPreviousKeyboardState.IsKeyDown(Key));
    }

    public bool KeyCombinationPressed(Keys Key1, Keys Key2)
    {
      return (KeyHeld(Key1) && KeyPressed(Key2));
    }
  }
}