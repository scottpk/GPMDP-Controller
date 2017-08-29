# GPMDP-Controller
A simple application to control Google Play Music Desktop Player using an Xbox controller.
This project is currently very, VERY early stages but works using GPMDP's JSON API.

# Setting the Filepath
You can set the filepath for GPMDP's program files in the settings. The keys to set are as follows:
* GPMDPFilePath - this should be set to the file path where the updater .exe is installed
* GPMDPFileExe - this is the name of GPMDP's executable (for example "Google Play Music Desktop Player.exe")
* GPMDPUpdaterExe - this is the name of the updater .exe (for example Update.exe)

# Controller Mappings
There is now a UI for mapping the buttons to different functions. The Settings window will show when you start the program, and you can map the buttons however you'd like. Changes will take effect as soon as you press "Save". Please note that the Xbox/Guide Button does not work. If you prefer you can also change the mappings inside App.config; however, these changes will not take effect until the application is restarted.

# The UI
The UI for this is very simple. The Settings window will show when you launch the application. Use the dropdowns to remap a button; press "Save" to commit your changes. Close the window to hide it. You can bring this up again by either double-clicking the tray icon, or by right-clicking the tray icon and choosing to show the window.
