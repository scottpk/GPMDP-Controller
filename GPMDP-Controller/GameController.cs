using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GPMDP_Controller
{
  public abstract class GameController
  {
    protected GamePadState prevState = new GamePadState();
    protected WebSocketController wsc;

    private Thread t;

    public delegate void ButtonDelegate();
    public abstract void CheckInput();
    public abstract void Pulse();
    public abstract void LoadMappings();

    public virtual async Task<int> GetNumbers()
    {
      throw new System.NotImplementedException();
    }

    public GameController(WebSocketController wsc, ControllerUserInterface ui)
    {
      this.wsc = wsc;
      LoadMappings();
      t = new Thread(new ThreadStart(() =>
      {
        while (ui.isRunning)
        {
          CheckInput();
          Thread.Sleep(1);
        }
      }
      ));
    }

    public static void Attention() {

      List<Thread> threadlist = new List<Thread>();
      for (int i = 0; i < GamePad.MaximumGamePadCount; i++)
      {
        if (GamePad.GetState(i).IsConnected)
        {
          int j = i;
          threadlist.Add(new Thread(new ThreadStart(() =>
          {
            GamePad.SetVibration(j, 0.0f, 1.0f);
            Thread.Sleep(500);
            GamePad.SetVibration(j, 0.0f, 0.0f);
            Thread.Sleep(150);
            GamePad.SetVibration(j, 0.0f, 1.0f);
            Thread.Sleep(500);
            GamePad.SetVibration(j, 0.0f, 0.0f);
            Thread.Sleep(150);
            GamePad.SetVibration(j, 0.0f, 1.0f);
            Thread.Sleep(500);
            GamePad.SetVibration(j, 0.0f, 0.0f);
          })));
        }
      }
      foreach (Thread t in threadlist)
      {
        t.Start();
      }

    }

    public void ThreadStart()
    {
      t.Start();
    }
    
    private void PlayPause()
    {
      wsc.SendRequest("playback", "playPause");
      Pulse();
    }


    /// <summary>
    /// Map a given delegate
    /// </summary>
    /// <param name="del">the delegate to set</param>
    /// <param name="function">the function which the delegate will perform</param>
    protected void Map(ref ButtonDelegate del, string function)
    {
      if (function != null)
      {
        switch (function.ToUpperInvariant())
        {
          case "PLAY_PAUSE":
            del = delegate () { PlayPause(); };
            break;
          case "TOGGLE_REPEAT":
            del = delegate () { wsc.SendRequest("playback", "toggleRepeat"); };
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
          default:
            del = delegate () { PlayPause(); };
            break;
        }
      }
      else
      {
        del = delegate () { PlayPause(); };
      }
    }
  }
}