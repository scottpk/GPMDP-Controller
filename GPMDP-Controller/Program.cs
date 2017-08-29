using System.Threading;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Configuration;
using System.Collections.Generic;

namespace GPMDP_Controller
{
  public delegate string GetCodeDelegate();
  class Program
  {
    static ControllerUserInterface cs;
    static XboxControls xc = null;
    static void Main(string[] args)
    {
      StartIfNotRunning();

      //XboxControls xc = null;
      OnSaveDelegate onSave = Save;
      cs = new ControllerSettings(onSave);

      while (cs == null)
      {
        Thread.Sleep(10);
      }

      xc = new XboxControls(cs);

      Thread t3 = new Thread(new ThreadStart(() =>
      {
        while (cs.isRunning)
        {
          xc.CheckInput();
          Thread.Sleep(1);
        }
      }));
      t3.Start();

      t3.Join();
    }

    private static void Save(KeyValueConfigurationCollection kvcc)
    {
      // Open App.Config of executable
      Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

      foreach (KeyValueConfigurationElement ce in kvcc)
      {
        string key = ce.Key;
        string value = ce.Value;
        if (ConfigurationManager.AppSettings[key] == null)
        {
          config.AppSettings.Settings.Add(key, value);
        }
        else
        {
          config.AppSettings.Settings.Remove(key);
          config.AppSettings.Settings.Add(key, value);
        }
      }

      config.Save(ConfigurationSaveMode.Modified);
      ConfigurationManager.RefreshSection("appSettings");

      xc.LoadMappings();
    }

    private static void StartIfNotRunning()
    {
      string filePath = ConfigurationManager.AppSettings["GPMDPFilePath"];
      string fileExe = ConfigurationManager.AppSettings["GPMDPFileExe"];
      if (!IsRunning(filePath, fileExe))
      {
        string updaterExe = Directory.GetFiles(filePath, ConfigurationManager.AppSettings["GPMDPUpdaterExe"], SearchOption.TopDirectoryOnly).First<string>();
        Process.Start(updaterExe, " --processStart \"" + fileExe + "\"");
      }
    }

    private static bool IsRunning(string path, string exeName)
    {
      string fullPath = Directory.GetFiles(path, exeName, SearchOption.AllDirectories).First<string>();
      return Process.GetProcesses().Any(p => p.StartInfo.FileName.ToUpperInvariant() == fullPath.ToUpperInvariant());
    }
  }
}
