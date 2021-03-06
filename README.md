# GPMDP-Controller
A simple application to control Google Play Music Desktop Player using an Xbox controller. This uses MonoGame to read the state of connected controllers. I own both a wired-only USB Xbox 360 controller and an Xbox One controller with a wireless USB receiver - the program seems to work fine with both.
This project is currently very, VERY early stages but works using GPMDP's JSON API. You need to have this feature enabled for the program to work.
You should not need to do much configuration to get this running, aside from setting file paths in the app.config.
Currently this application is Windows-only but this will likely change.
No drivers should be needed if you are using an actual USB Xbox controller.

# Purpose
This is intended for media center PCs and similar use cases where the user is likely not right by the mouse and keyboard or directly in front of the screen, but usually has a controller hooked up which gets used for emulators and the like.

# Inputting The Authentication Code
Google Play Music Desktop Player will show a code on-screen when it receives the request to control playback. GPMDP-Controller 
currently offers two means to input this code:
- A dialog with a text box - just type it in and click the button
- Xbox controller input (explained below)

GPMDP-Controller only needs this code the first time, after which it will receive a permanent token from GPMDP. You should not need this manual step again unless something happens to cause GPMDP or GPMDP-Controller to lose authentication.

## Using the Xbox Controller to Input Your Code
This method involves using the left stick and left trigger to modify each individual digit; the right stick to finalize each digit; and the right trigger to submit the code.
The digits are arranged counterclockwise around the left stick. Starting from the lower left:
* Without the left trigger held: 0, 1, 2, 3, 4
* With the left trigger held:    5, 6, 7, 8, 8

To choose that digit, move the right stick up. To submit the code, press the right trigger. There is a UI window which will pop up to show which digits you are selecting - you do not need to memorize the positions.

# Setting the Filepath
You can set the filepath for GPMDP's program files in the settings. The keys to set are as follows:
* GPMDPFilePath - this should be set to the file path where the updater .exe is installed
* GPMDPFileExe - this is the name of GPMDP's executable (for example "Google Play Music Desktop Player.exe")
* GPMDPUpdaterExe - this is the name of the updater .exe (for example Update.exe)

# Controller Mappings
There is now a UI for mapping the buttons to different functions. The Settings window will show when you start the program, and you can map the buttons however you'd like. Changes will take effect as soon as you press "Save". Please note that the Xbox/Guide Button does not work. If you prefer you can also change the mappings inside App.config; however, these changes will not take effect until the application is restarted.

# The UI
The UI for this is very simple. The Settings window will show when you launch the application. Use the dropdowns to remap a button; press "Save" to commit your changes. Close the window to hide it. You can bring this up again by either double-clicking the tray icon, or by right-clicking the tray icon and choosing to show the window.
