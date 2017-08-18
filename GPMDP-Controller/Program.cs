using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Input;
using System.Configuration;

namespace GPMDP_Controller
{
  public delegate string GetCodeDelegate();
  class Program
  {
    static ControllerUserInterface cs;
    static void Main(string[] args)
    {
      //WebSocketController wsc = new WebSocketController(delegate()
      //{
      //  return cs.GetAuthCode();
      //});

      //XboxControls xc = new XboxControls(wsc);
      XboxControls xc = new XboxControls(delegate ()
      {
        return cs.GetAuthCode();
      });

      Thread t = new Thread(new ThreadStart(() =>
      {
        cs = new ControllerSettings(xc);
        //System.Windows.Forms.Application.Run(cs);
        cs.Start();
      }));
      t.Start();

      Thread t3 = new Thread(new ThreadStart(() =>
      {
        while (t.IsAlive)
        {
          xc.CheckInput();
          Thread.Sleep(50);
        }
      }));
      t3.Start();

      t3.Join();
    }
  }


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
    /// Instantiator method. I'm not sure it was the best design to have this take in a class as a parameter.
    /// </summary>
    /// <param name="wsc">WebSocketController which will do all the things</param>
    public XboxControls(WebSocketController wsc)
    {
      this.wsc = wsc;
      LoadMappings();
    }

    /// <summary>
    /// Instantiator method. This might be better than passing a class but it might be better to just not have a separate class
    /// for the websockets
    /// </summary>
    /// <param name="getCode">a delegate that can be called to get the authentication code GPMDP is displaying</param>
    public XboxControls(GetCodeDelegate getCode)
    {
      this.wsc = new WebSocketController(getCode);
      LoadMappings();
    }

    private void PlayPause()
    {
      wsc.PlayPause();
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
            del = delegate () { wsc.ToggleRepeat(); };
            break;
          case "TOGGLE_SHUFFLE":
            del = delegate () { wsc.ToggleShuffle(); };
            break;
          case "INCREASE_VOLUME":
            del = delegate () { wsc.IncreaseVolume(); };
            break;
          case "DECREASE_VOLUME":
            del = delegate () { wsc.DecreaseVolume(); };
            break;
          case "BACK":
            del = delegate () { wsc.Back(); };
            break;
          case "FORWARD":
            del = delegate () { wsc.Forward(); };
            break;
          case "TOGGLE_THUMBS_UP":
            del = delegate () { wsc.ToggleThumbsUp(); };
            break;
          case "TOGGLE_THUMBS_DOWN":
            del = delegate () { wsc.ToggleThumbsDown(); };
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
  }

  public class WebSocketController
  {
    Uri wsUri;
    ClientWebSocket ws = new ClientWebSocket();
    private UTF8Encoding encoding = new UTF8Encoding();
    SocketResponse response;
    GetCodeDelegate getCode;
    private string authCode
    {
      get
      {
        if (ConfigurationManager.AppSettings["authCode"] == null) return "";
        else return ConfigurationManager.AppSettings["authCode"];
      }
      set
      {
        // Open App.Config of executable
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        if (ConfigurationManager.AppSettings["authCode"] == null)
        {
          config.AppSettings.Settings.Add("authCode", value);
        }
        else
        {
          config.AppSettings.Settings.Remove("authCode");
          config.AppSettings.Settings.Add("authCode", value);
        }

        config.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection("appSettings");
      }
    }

    public WebSocketController(GetCodeDelegate getCode, string host = "localhost", int port = 5672, string scheme = "ws")
    {
      UriBuilder builder = new UriBuilder();
      builder.Host = host;
      builder.Port = port;
      builder.Scheme = scheme;
      wsUri = builder.Uri;
      this.getCode = getCode;
      ws.ConnectAsync(wsUri, CancellationToken.None);

      // wait until a connection is established
      while (ws.State == WebSocketState.Connecting)
      {
        Thread.Sleep(100);
        if (ws.State == WebSocketState.Closed)
        {
          ws = new ClientWebSocket();
          ws.ConnectAsync(wsUri, CancellationToken.None);
        }
      }

      //request to control the playback
      SocketRequest req = new SocketRequest() { socketNameSpace = "connect", method = "connect" };
      if (authCode == "") req.arguments = new string[] { "Console" };
      else req.arguments = new string[] { "Console", authCode };
      SendRequest(req);

      //read the responses
      response = new SocketResponse();
      MemoryStream ms = new MemoryStream(ReceiveBytes().Result);
      DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(SocketResponse));
      ms.Position = 0;
      response = (SocketResponse)ser.ReadObject(ms);
      while (authCode == "")
      {
        if (response.channel == "connect")
        {
          if (response.payload.ToString().ToUpperInvariant() == "CODE_REQUIRED")
          {// need to send the request again with the user's code
           //request to control the playback
            req = new SocketRequest() { socketNameSpace = "connect", method = "connect", arguments = new string[]{ "Console", getCode() } };
            SendRequest(req);
            ms = new MemoryStream(ReceiveBytes().Result);
            ser = new DataContractJsonSerializer(typeof(SocketResponse));
            ms.Position = 0;
            response = (SocketResponse)ser.ReadObject(ms);
          }
          else
          {
            authCode = response.payload.ToString();
            req = new SocketRequest() { socketNameSpace = "connect", method = "connect", arguments = new string[] { "Console", authCode } };
            SendRequest(req);
          }
        }
        else
        {
          ms = new MemoryStream(ReceiveBytes().Result);
          ser = new DataContractJsonSerializer(typeof(SocketResponse));
          ms.Position = 0;
          response = (SocketResponse)ser.ReadObject(ms);
        }
      }
    }

    /// <summary>
    /// Send a specific SocketRequest() to GPMDP
    /// </summary>
    /// <param name="req">the request to send</param>
    private void SendRequest(SocketRequest req)
    {
      MemoryStream ms = new MemoryStream();
      DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(SocketRequest));
      ser.WriteObject(ms, req);
      ms.Position = 0;
      StreamReader reader = new StreamReader(ms);
      string toSend = reader.ReadToEnd();
      Send(toSend);
    }

    public void ToggleShuffle()
    {
      SocketRequest req = new SocketRequest() { socketNameSpace = "playback", method = "toggleShuffle" };
      SendRequest(req);
    }

    public void ToggleRepeat()
    {
      SocketRequest req = new SocketRequest() { socketNameSpace = "playback", method = "toggleRepeat" };
      SendRequest(req);
    }

    public void PlayPause()
    {
      SocketRequest req = new SocketRequest() { socketNameSpace = "playback", method = "playPause" };
      SendRequest(req);
    }

    public void DecreaseVolume()
    {
      SocketRequest req = new SocketRequest() { socketNameSpace = "volume", method = "decreaseVolume" };
      SendRequest(req);
    }

    public void IncreaseVolume()
    {
      SocketRequest req = new SocketRequest() { socketNameSpace = "volume", method = "increaseVolume" };
      SendRequest(req);
    }

    public void Back()
    {
      SocketRequest req = new SocketRequest() { socketNameSpace = "playback", method = "rewind" };
      SendRequest(req);
    }

    public void Forward()
    {
      SocketRequest req = new SocketRequest() { socketNameSpace = "playback", method = "forward" };
      SendRequest(req);
    }

    public void ToggleThumbsUp()
    {
      SocketRequest req = new SocketRequest() { socketNameSpace = "rating", method = "toggleThumbsUp" };
      SendRequest(req);
    }

    public void ToggleThumbsDown()
    {
      SocketRequest req = new SocketRequest() { socketNameSpace = "rating", method = "toggleThumbsDown" };
      SendRequest(req);
    }

    private async Task Send(string data)
    {
      if (ws.State == WebSocketState.Open)
      {
        await ws.SendAsync(new ArraySegment<byte>(encoding.GetBytes(data)), WebSocketMessageType.Binary, true, CancellationToken.None);
      }
    }

    public async Task<byte[]> ReceiveBytes()
    {
      List<byte> outputList = new List<byte>();
      byte[] buffer = new byte[1];
      while (ws.State == WebSocketState.Open)
      {
        WebSocketReceiveResult result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        if (result.MessageType == WebSocketMessageType.Close)
        {
          await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }
        else
        {
          outputList.Add(buffer[0]);
        }
        if (result.EndOfMessage)
        {
          return outputList.ToArray<byte>();
        }
      }
      return outputList.ToArray<byte>();
    }
  }

  [DataContract]
  class SocketResponse
  {
    [DataMember(Name ="channel")]
    private string _channel;
    [DataMember(Name ="payload")]
    private object _payload;

    [IgnoreDataMember]
    public string channel { get { return _channel; } }
    [IgnoreDataMember]
    public object payload { get { return (_payload is null ? "" : _payload); } }
  }

  [DataContract]
  class SocketRequest
  {
    [DataMember(Name ="namespace")]
    public string socketNameSpace;
    [DataMember(Name = "method")]
    public string method;

    [DataMember(Name = "arguments")]
    public object[] arguments;
  }
}
