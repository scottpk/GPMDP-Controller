using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Configuration;
using System.Runtime.Serialization;

namespace GPMDP_Controller
{
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
      this.getCode = delegate () {
        GameController.Attention();
        return getCode();
      };
    }
    
    public void Connect()
    {
      ws.ConnectAsync(wsUri, CancellationToken.None);

      // wait until a connection is established
      while (ws.State == WebSocketState.Connecting)
      {
        Thread.Sleep(1);
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
      if (ws.State == WebSocketState.Open)
      {
        ws.SendAsync(new ArraySegment<byte>(encoding.GetBytes(toSend)), WebSocketMessageType.Binary, true, CancellationToken.None);
      }
      else if (ws.State == WebSocketState.Aborted)
      {
        // do not add the overhead of repeating the command, just reconnect and they can try again if they want
        ws = new ClientWebSocket();
        Connect();
      }
    }

    public void SendRequest(string ns, string method)
    {
      SocketRequest req = new SocketRequest() { socketNameSpace = ns, method = method };
      SendRequest(req);
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
    [DataMember(Name = "channel")]
    private string _channel;
    [DataMember(Name = "payload")]
    private object _payload;

    [IgnoreDataMember]
    public string channel { get { return _channel; } }
    [IgnoreDataMember]
    public object payload { get { return (_payload is null ? "" : _payload); } }
  }

  [DataContract]
  class SocketRequest
  {
    [DataMember(Name = "namespace")]
    public string socketNameSpace;
    [DataMember(Name = "method")]
    public string method;

    [DataMember(Name = "arguments")]
    public object[] arguments;
  }
}
