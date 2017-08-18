using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;

namespace GPMDP_Controller
{
  public partial class ControllerSettings : Form, ControllerUserInterface
  {
    private XboxControls xc;
    private bool _messageShown;
    public ControllerSettings(XboxControls xc)
    {
      InitializeComponent();
      this.xc = xc;
      Dictionary<string, string> vals = new Dictionary<string, string>
      {
        {"PLAY_PAUSE","Play/Pause" },
        {"TOGGLE_REPEAT","Toggle Repeat" },
        {"TOGGLE_SHUFFLE","Toggle Shuffle" },
        {"INCREASE_VOLUME","Increase Volume" },
        {"DECREASE_VOLUME","Decrease Volume" },
        {"BACK","Back" },
        {"FORWARD","Forward" },
        {"TOGGLE_THUMBS_UP","Toggle Thumbs Up" },
        {"TOGGLE_THUMBS_DOWN","Toggle Thumbs Down" }
      };
      foreach (ComboBox cb in this.Controls.OfType<ComboBox>())
      {
        cb.DataSource = new BindingSource(vals, null);
        cb.ValueMember = "Key";
        cb.DisplayMember = "Value";
        //if (vals.ContainsKey(cb.Name.Replace("cb", "")+"Mapping"))
        string appKey = cb.Name.Replace("cb", "") + "Mapping";
        if (ConfigurationManager.AppSettings[appKey] != null)
        {
          cb.SelectedItem = vals.FirstOrDefault(v => v.Key == ConfigurationManager.AppSettings[appKey].ToUpperInvariant());
        }
      }
      tsmiShowWindow.Click += (object sender, EventArgs args) => {
        Show();
      };
      tsmiExit.Click += (object sender, EventArgs args) =>
      {
        Application.ExitThread();
      };
      tsmiAbout.Click += (object sender, EventArgs args) =>
      {
        MessageBox.Show("GPMDP Controller\r\nBy Scott Karbon", "About");
      };
      gpmcNotifyIcon.Visible = true;
    }

    private void ControllerSettings_Load(object sender, EventArgs e)
    {
    }

    public string GetAuthCode()
    {
      Form prompt = new Form()
      {
        Width = 500,
        Height = 150,
        FormBorderStyle = FormBorderStyle.FixedDialog,
        Text = "Please enter your code",
        StartPosition = FormStartPosition.CenterScreen
      };
      Label textLabel = new Label() { Left = 50, Top = 20, Text = "Put it here" };
      TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400, };
      Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
      confirmation.Click += (sender, e) => {
        prompt.Close();
      };
      prompt.Controls.Add(textBox);
      prompt.Controls.Add(confirmation);
      prompt.Controls.Add(textLabel);
      prompt.AcceptButton = confirmation;

      int response = 0;
      bool collected = false;
      while (!collected)
      {
        collected = int.TryParse(prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "", out response);
      }
      return response.ToString();
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
      // Open App.Config of executable
      Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

      foreach (ComboBox cb in Controls.OfType<ComboBox>())
      {
        string appKey = cb.Name.Replace("cb", "") + "Mapping";
        string value = ((KeyValuePair<string, string>)cb.SelectedItem).Key;

        if (ConfigurationManager.AppSettings[appKey] == null)
        {
          config.AppSettings.Settings.Add(appKey, value);
        }
        else
        {
          config.AppSettings.Settings.Remove(appKey);
          config.AppSettings.Settings.Add(appKey, value);
        }
      }

      config.Save(ConfigurationSaveMode.Modified);
      ConfigurationManager.RefreshSection("appSettings");

      xc.LoadMappings();
    }

    private void ControllerSettings_FormClosing(object sender, FormClosingEventArgs e)
    {
      e.Cancel = true;
      gpmcNotifyIcon.Visible = true;
      Hide();
      if (!_messageShown)
      {
        gpmcNotifyIcon.ShowBalloonTip(3000, "GPMDP Controller", "The application is still running. Use the taskbar icon to bring up the settings menu again.", ToolTipIcon.Info);
        _messageShown = true;
      }
    }

    private void gpmcNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      Show();
    }

    private void gpmcContextMenuStrip_Opening(object sender, CancelEventArgs e)
    {

    }

    public void Start()
    {
      Application.Run(this);
    }
  }
}
