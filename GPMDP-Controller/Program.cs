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
using MonoGame.Framework;
using Microsoft.Xna.Framework.Input;
using System.Configuration;

namespace GPMDP_Controller
{
  class Program
  {
    static void Main(string[] args)
    {
      WebSocketController wsc = new WebSocketController();
      //Console.WriteLine(wsc.ReceiveNonAsync());
      XboxControls xc = new XboxControls(wsc);
      while (true)
      {
        //if (Console.KeyAvailable)
        //{
        //  Console.ReadKey();
        //  Console.WriteLine("keypress");
        //  wsc.PlayPause();
        //}
        xc.CheckInput();
        Thread.Sleep(50);
      }
      //Console.ReadLine();
    }
  }

  class XboxControls
  {
    private delegate void ButtonDelegate();
    GamePadState prevState = new GamePadState();
    WebSocketController wsc;
    ButtonDelegate Big, Back, Start, A, B, X, Y, DpadUp, DpadDown, DpadLeft, DpadRight, LStick, RStick, LShoulder, RShoulder;
    public XboxControls(WebSocketController wsc)
    {
      this.wsc = wsc;
      this.Big = delegate () { wsc.PlayPause(); };
      this.Back = delegate () { wsc.PlayPause(); };
      this.Start = delegate () { wsc.PlayPause(); };
      this.A = delegate () { wsc.PlayPause(); };
      this.B = delegate () { wsc.ToggleRepeat(); };
      this.X = delegate () { wsc.ToggleShuffle(); };
      this.Y = delegate () { wsc.PlayPause(); };
      this.DpadUp = delegate () { wsc.IncreaseVolume(); };
      this.DpadDown = delegate () { wsc.DecreaseVolume(); };
      this.DpadLeft = delegate () { wsc.Back(); };
      this.DpadRight = delegate () { wsc.Forward(); };
      this.LStick = delegate () { wsc.PlayPause(); };
      this.RStick = delegate () { wsc.PlayPause(); };
      this.LShoulder = delegate () { wsc.ToggleThumbsDown(); };
      this.RShoulder = delegate () { wsc.ToggleThumbsUp(); };
    }
    public void CheckInput()
    {
      for (int i = 0; i < GamePad.MaximumGamePadCount; i++)
      {
        GamePadState gps = GamePad.GetState(i);

        if (gps.IsConnected)
        {
          i = GamePad.MaximumGamePadCount;// only look at one controller

          if ((gps.Buttons.BigButton.Equals(ButtonState.Pressed)) && (prevState.Buttons.BigButton.Equals(ButtonState.Pressed)))
          {
            Big();
          }
          if ((gps.Buttons.Back.Equals(ButtonState.Pressed)) && (prevState.Buttons.Back.Equals(ButtonState.Pressed)))
          {
            Back();
          }
          if ((gps.Buttons.Start.Equals(ButtonState.Pressed)) && (prevState.Buttons.Start.Equals(ButtonState.Pressed)))
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
        }
      }
    }
  }

  class WebSocketController
  {
    Uri wsUri;
    ClientWebSocket ws = new ClientWebSocket();
    private UTF8Encoding encoding = new UTF8Encoding();
    SocketResponse response;
    //private string authCode = "";
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
          config.AppSettings.Settings.Add("authCode", value);//. = value;//.Set("authCode", value);
        }

        config.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection("appSettings");
      }
    }
    public WebSocketController()
    {
      UriBuilder builder = new UriBuilder();
      builder.Host = "localhost";
      builder.Port = 5672;
      builder.Scheme = "ws";
      wsUri = builder.Uri;
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
        //string outputString = " Channel: " + response.channel + " Payload: " + response.payload.ToString();
        //Console.WriteLine(outputString);
        if (response.channel == "connect")
        {
          if (response.payload.ToString().ToUpperInvariant() == "CODE_REQUIRED")
          {// need to send the request again with the user's code
           //request to control the playback
            Console.WriteLine("Need your access code. Please enter it here:");
            req = new SocketRequest() { socketNameSpace = "connect", method = "connect", arguments = new string[]{ "Console", Console.ReadLine() } };
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

    private void SendRequest(SocketRequest req)
    {
      MemoryStream ms = new MemoryStream();
      DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(SocketRequest));
      ser.WriteObject(ms, req);
      ms.Position = 0;
      StreamReader reader = new StreamReader(ms);
      string toSend = reader.ReadToEnd();
      Send(toSend).Wait();
    }

    public void ToggleShuffle()
    {
      SocketRequest req = new SocketRequest();
      req.socketNameSpace = "playback";
      req.method = "toggleShuffle";
      SendRequest(req);
    }

    public void ToggleRepeat()
    {
      SocketRequest req = new SocketRequest();
      req.socketNameSpace = "playback";
      req.method = "toggleRepeat";
      SendRequest(req);
    }

    public void PlayPause()
    {
      //request to control the playback
      SocketRequest req = new SocketRequest();
      req.socketNameSpace = "playback";
      req.method = "playPause";
      SendRequest(req);
    }

    public void DecreaseVolume()
    {
      SocketRequest req = new SocketRequest();
      req.socketNameSpace = "volume";
      req.method = "decreaseVolume";
      SendRequest(req);
    }

    public void IncreaseVolume()
    {
      SocketRequest req = new SocketRequest();
      req.socketNameSpace = "volume";
      req.method = "increaseVolume";
      SendRequest(req);
    }

    public void Back()
    {
      SocketRequest req = new SocketRequest();
      req.socketNameSpace = "playback";
      req.method = "rewind";
      SendRequest(req);
    }

    public void Forward()
    {
      SocketRequest req = new SocketRequest();
      req.socketNameSpace = "playback";
      req.method = "forward";
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
