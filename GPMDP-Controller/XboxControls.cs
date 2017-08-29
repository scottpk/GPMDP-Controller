using System.Threading;
using Microsoft.Xna.Framework.Input;
using System.Configuration;
using System.Collections.Generic;

namespace GPMDP_Controller
{
  /// <summary>
  /// This class is for the Xbox controller
  /// </summary>
  public class XboxControls
  {
    private delegate void ButtonDelegate();
    private int playerIndex = -1;
    GamePadState prevState = new GamePadState();
    WebSocketController wsc;
    ButtonDelegate Big, Back, Start, A, B, X, Y, DpadUp, DpadDown, DpadLeft, DpadRight, LStick, RStick, LShoulder, RShoulder;

    /// <summary>
    /// Instantiator method. This is probably better way to do this.
    /// </summary>
    /// <param name="ui">the UI that is being used - this is passed so we can get the authentication code GPMDP is displaying when needed</param>
    public XboxControls(ControllerUserInterface ui)
    {
      this.wsc = new WebSocketController(ui, this);
      LoadMappings();
    }

    private void PlayPause()
    {
      wsc.SendRequest("playback", "playPause");
      if (playerIndex > -1)
      {
        GamePad.SetVibration(playerIndex, 0.0f, 1.0f);
        Thread.Sleep(50);
        GamePad.SetVibration(playerIndex, 0.0f, 0.0f);
      }
    }

    /// <summary>
    /// Run through each delegate, and map it using Map(delegate,string)
    /// </summary>
    public void LoadMappings()
    {
      Map(ref Big,        ConfigurationManager.AppSettings["BigButtonMapping"]);
      Map(ref Back,       ConfigurationManager.AppSettings["BackButtonMapping"]);
      Map(ref Start,      ConfigurationManager.AppSettings["StartButtonMapping"]);
      Map(ref A,          ConfigurationManager.AppSettings["AButtonMapping"]);
      Map(ref B,          ConfigurationManager.AppSettings["BButtonMapping"]);
      Map(ref X,          ConfigurationManager.AppSettings["XButtonMapping"]);
      Map(ref Y,          ConfigurationManager.AppSettings["YButtonMapping"]);
      Map(ref LStick,     ConfigurationManager.AppSettings["LStickButtonMapping"]);
      Map(ref RStick,     ConfigurationManager.AppSettings["RStickButtonMapping"]);
      Map(ref LShoulder,  ConfigurationManager.AppSettings["LShoulderButtonMapping"]);
      Map(ref RShoulder,  ConfigurationManager.AppSettings["RShoulderButtonMapping"]);
      Map(ref DpadUp,     ConfigurationManager.AppSettings["DpadUpMapping"]);
      Map(ref DpadDown,   ConfigurationManager.AppSettings["DpadDownMapping"]);
      Map(ref DpadLeft,   ConfigurationManager.AppSettings["DpadLeftMapping"]);
      Map(ref DpadRight,  ConfigurationManager.AppSettings["DpadRightMapping"]);
    }

    /// <summary>
    /// Map a given delegate
    /// </summary>
    /// <param name="del">the delegate to set</param>
    /// <param name="function">the function which the delegate will perform</param>
    private void Map(ref ButtonDelegate del, string function)
    {
      if (function != null)
      {
        switch (function.ToUpperInvariant())
        {
          case "PLAY_PAUSE":
            del = delegate () { PlayPause(); };
            break;
          case "TOGGLE_REPEAT":
            del = delegate () { wsc.SendRequest("playback", "toggleRepeat" ); };
            break;
          case "TOGGLE_SHUFFLE":
            del = delegate () { wsc.SendRequest("playback", "toggleShuffle"); };
            break;
          case "INCREASE_VOLUME":
            del = delegate () { wsc.SendRequest("volume", "increaseVolume"); };
            break;
          case "DECREASE_VOLUME":
            del = delegate () { wsc.SendRequest("volume", "decreaseVolume"); };
            break;
          case "BACK":
            del = delegate () { wsc.SendRequest("playback", "rewind"); };
            break;
          case "FORWARD":
            del = delegate () { wsc.SendRequest("playback", "forward"); };
            break;
          case "TOGGLE_THUMBS_UP":
            del = delegate () { wsc.SendRequest("rating", "toggleThumbsUp"); };
            break;
          case "TOGGLE_THUMBS_DOWN":
            del = delegate () { wsc.SendRequest("rating", "toggleThumbsDown"); };
            break;
          default: del = delegate () { PlayPause(); };
            break;
        }
      }
      else
      {
        del = delegate () { PlayPause(); };
      }
    }

    /// <summary>
    /// This is the "main piece" which will look for the first connected controller, and call delegates corresponding to which buttons
    /// have been pressed. This is designed to verify that the button was previously released, so that holding down the button does not
    /// cause the same command to be sent repeatedly.
    /// </summary>
    public void CheckInput()
    {
      for (int i = 0; i < GamePad.MaximumGamePadCount; i++)
      {
        GamePadState gps = GamePad.GetState(i);
        playerIndex = i;

        if (gps.IsConnected)
        {
          if ((gps.Buttons.BigButton.Equals(ButtonState.Pressed)) && (prevState.Buttons.BigButton.Equals(ButtonState.Released)))
          {
            Big();
          }
          if ((gps.Buttons.Back.Equals(ButtonState.Pressed)) && (prevState.Buttons.Back.Equals(ButtonState.Released)))
          {
            Back();
          }
          if ((gps.Buttons.Start.Equals(ButtonState.Pressed)) && (prevState.Buttons.Start.Equals(ButtonState.Released)))
          {
            Start();
          }
          if ((gps.Buttons.A.Equals(ButtonState.Pressed)) && (prevState.Buttons.A.Equals(ButtonState.Released)))
          {
            A();
          }
          if ((gps.Buttons.B.Equals(ButtonState.Pressed)) && (prevState.Buttons.B.Equals(ButtonState.Released)))
          {
            B();
          }
          if ((gps.Buttons.X.Equals(ButtonState.Pressed)) && (prevState.Buttons.X.Equals(ButtonState.Released)))
          {
            X();
          }
          if ((gps.Buttons.Y.Equals(ButtonState.Pressed)) && (prevState.Buttons.Y.Equals(ButtonState.Released)))
          {
            Y();
          }
          if ((gps.Buttons.LeftShoulder.Equals(ButtonState.Pressed)) && (prevState.Buttons.LeftShoulder.Equals(ButtonState.Released)))
          {
            LShoulder();
          }
          if ((gps.Buttons.RightShoulder.Equals(ButtonState.Pressed)) && (prevState.Buttons.RightShoulder.Equals(ButtonState.Released)))
          {
            RShoulder();
          }
          if ((gps.Buttons.LeftStick.Equals(ButtonState.Pressed)) && (prevState.Buttons.LeftStick.Equals(ButtonState.Released)))
          {
            LStick();
          }
          if ((gps.Buttons.RightStick.Equals(ButtonState.Pressed)) && (prevState.Buttons.RightStick.Equals(ButtonState.Released)))
          {
            RStick();
          }
          if (gps.DPad.Down.Equals(ButtonState.Pressed))
          {
            DpadDown();
          }
          if (gps.DPad.Up.Equals(ButtonState.Pressed))
          {
            DpadUp();
          }
          if ((gps.DPad.Left.Equals(ButtonState.Pressed)) && (prevState.DPad.Left.Equals(ButtonState.Released)))
          {
            DpadLeft();
          }
          if ((gps.DPad.Right.Equals(ButtonState.Pressed)) && (prevState.DPad.Right.Equals(ButtonState.Released)))
          {
            DpadRight();
          }

          prevState = gps;
          i = GamePad.MaximumGamePadCount;// only look at one controller
        }
      }
    }

    public void Attention()
    {
      List<Thread> threadlist = new List<Thread>();
      for (int i = 0; i < GamePad.MaximumGamePadCount; i++)
      {
        if (GamePad.GetState(i).IsConnected)
        {
          int j = i;
          threadlist.Add(new Thread(new ThreadStart(() => { 
          GamePad.SetVibration(j, 0.0f, 1.0f);
          Thread.Sleep(200);
          GamePad.SetVibration(j, 0.0f, 0.0f);
          Thread.Sleep(200);
          GamePad.SetVibration(j, 0.0f, 1.0f);
          Thread.Sleep(200);
          GamePad.SetVibration(j, 0.0f, 0.0f);
          Thread.Sleep(200);
          GamePad.SetVibration(j, 0.0f, 1.0f);
          Thread.Sleep(200);
          GamePad.SetVibration(j, 0.0f, 0.0f);
          })));
        }
      }
      foreach(Thread t in threadlist)
      {
        t.Start();
      }
    }
  }
}
