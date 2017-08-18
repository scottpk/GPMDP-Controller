using System.Configuration;

namespace GPMDP_Controller
{
  public delegate void OnSaveDelegate(KeyValueConfigurationCollection kvcc);
  public interface ControllerUserInterface
  {
    string GetAuthCode();
    void Start();
  }
}
