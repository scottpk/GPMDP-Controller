using System.Threading;
using Microsoft.Xna.Framework.Input;
using System.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace GPMDP_Controller
{
  /// <summary>
  /// This class is for the Xbox controller
  /// </summary>
  public class XboxControls : GameController
  {
    private int playerIndex = -1;
    ButtonDelegate Big, Back, Start, A, B, X, Y, DpadUp, DpadDown, DpadLeft, DpadRight, LStick, RStick, LShoulder, RShoulder;

    /// <summary>
    /// Instantiator method. The UI is being passed so we can see when it exits, and also so we can pass it to the WebSocketController
    /// which will use it to request input when GPMDP asks for the user's code. There is probably a better way to do this.
    /// </summary>
    /// <param name="ui">the UI that is being used</param>
    public XboxControls(WebSocketController wsc, ControllerUserInterface ui) : base(wsc, ui)
    {
    }

    /// <summary>
    /// Run through each delegate, and map it using Map(delegate,string)
    /// </summary>
    public override void LoadMappings()
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
    /// This is the "main piece" which will look for the first connected controller, and call delegates corresponding to which buttons
    /// have been pressed. This is designed to verify that the button was previously released, so that holding down the button does not
    /// cause the same command to be sent repeatedly.
    /// </summary>
    public override void CheckInput()
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
          if (gps.DPad.Down.Equals(ButtonState.Pressed) && prevState.DPad.Down.Equals(ButtonState.Released))
          {
            DpadDown();
          }
          if (gps.DPad.Up.Equals(ButtonState.Pressed) && prevState.DPad.Up.Equals(ButtonState.Released))
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

    public override void Pulse()
    {
      if (playerIndex > -1)
      {
        GamePad.SetVibration(playerIndex, 0.0f, 1.0f);
        Thread.Sleep(50);
        GamePad.SetVibration(playerIndex, 0.0f, 0.0f);
      }
    }

    public override async Task<int> GetNumbers(CancellationToken cancelToken)
    {
      return await Task<int>.Run(() => { return GetNumbersNonAsync(cancelToken); });
    }

    private int GetNumbersNonAsync(CancellationToken cancelToken)
    {
      System.Windows.Forms.Form prompt = new System.Windows.Forms.Form()
      {
        Width = 500,
        Height = 400,
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog,
        Text = "Please enter your code",
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen,
        Icon = GPMDPControllerResources.Icon1
      };
      System.Windows.Forms.Label nextNumberLabel = new System.Windows.Forms.Label() { Left = 50, Top = 20, Height = 200, Width = 400, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, Text = "X", Font = new System.Drawing.Font(System.Drawing.FontFamily.GenericSansSerif, 24, System.Drawing.FontStyle.Bold)};
      System.Windows.Forms.Label currentNumberLabel = new System.Windows.Forms.Label() { Left = 50, Top = 200, Height = 200, Width = 400, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, Text = "XXXX", Font = new System.Drawing.Font(System.Drawing.FontFamily.GenericSansSerif, 24, System.Drawing.FontStyle.Bold) };
      prompt.Controls.Add(nextNumberLabel);
      prompt.Controls.Add(currentNumberLabel);
      prompt.Show();
      prompt.Refresh();
      

      List<double[]> sectionList = new List<double[]>();
      double lastDbl = -(System.Math.PI);
      double sectionSize = System.Math.PI / 2.5;
      for (int i = 0; i < 5; i++)
      {
        double[] dbl = new double[] { lastDbl, 0.0 };
        lastDbl += sectionSize;
        dbl[1] = lastDbl;
        sectionList.Add(dbl);
      }
      double[][] sectionArray = sectionList.ToArray();
      string val = "";
      GamePadState gps = new GamePadState();

      for (int i = 0; i < GamePad.MaximumGamePadCount; i++)
      {
        gps = GamePad.GetState(i);
        playerIndex = i;
        //prevState = gps;
        i = GamePad.MaximumGamePadCount;// only look at one controller
      }

      float x = gps.ThumbSticks.Left.X;
      float y = gps.ThumbSticks.Left.Y;
      while ((gps.Triggers.Right < .5f) && (!cancelToken.IsCancellationRequested))
      {
        cancelToken.ThrowIfCancellationRequested();
        x = gps.ThumbSticks.Left.X;
        y = gps.ThumbSticks.Left.Y;

        if (Math.Abs(x) > 0.5f || Math.Abs(y) > 0.5f)
        {
          double leftAngle = System.Math.Atan2(gps.ThumbSticks.Left.Y, gps.ThumbSticks.Left.X);
          //// zone 1 = all the way to the left, slightly up
          //if (angle > )
          int zone = 0;
          //for (int i = 0; (sectionSize * (double) i) < System.Math.PI * 2.0; i++)
          for (int i = 0; i < sectionArray.Length; i++)
          {
            double sectionLimit1 = sectionArray[i][0];
            double sectionLimit2 = sectionArray[i][1];
            if (((leftAngle >= sectionLimit1) && (leftAngle <= sectionLimit2)) || ((leftAngle <= sectionLimit1) && (leftAngle >= sectionLimit2)))
            {
              zone = i;
              break;
            }
          }
          zone = (gps.Triggers.Left > .5f) ? zone + 5 : zone;
          nextNumberLabel.Text = zone.ToString();
          prompt.Refresh();
          if (gps.ThumbSticks.Right.Y > 0.5f)
          {
            val += zone.ToString();
            currentNumberLabel.Text = val;
            prompt.Refresh();
            while (gps.ThumbSticks.Right.Y > 0.5f) {
              Thread.Sleep(1);
              gps = GamePad.GetState(playerIndex);
            }
          }
        }
        Thread.Sleep(1);
        gps = GamePad.GetState(playerIndex);
      }

      prompt.Close();

      return int.Parse(val);
    }
  }
}
