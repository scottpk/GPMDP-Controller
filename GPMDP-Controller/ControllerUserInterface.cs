using System.Configuration;

namespace GPMDP_Controller
{
  public delegate void OnSaveDelegate(KeyValueConfigurationCollection kvcc);
  public abstract class ControllerUserInterface
  {
    public OnSaveDelegate _onSave { get; protected set; }
    public abstract bool isRunning { get; }
    public ControllerUserInterface(OnSaveDelegate onSave)
    {
      _onSave = onSave;
    }
    public abstract string GetAuthCode();
    public abstract void Start();
    public void Save(KeyValueConfigurationCollection kvcc)
    {
      _onSave(kvcc);
    }
  }
}
