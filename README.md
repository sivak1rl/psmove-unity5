<pre>
  _____   _____ __  __  ______      ________      _    _       _ _         _____ 
 |  __ \ / ____|  \/  |/ __ \ \    / /  ____|    | |  | |     (_) |       | ____|    
 | |__) | (___ | \  / | |  | \ \  / /| |__ ______| |  | |_ __  _| |_ _   _| |__  
 |  ___/ \___ \| |\/| | |  | |\ \/ / |  __|______| |  | | '_ \| | __| | | |___ \ 
 | |     ____) | |  | | |__| | \  /  | |____     | |__| | | | | | |_| |_| |___) |
 |_|    |_____/|_|  |_|\____/   \/   |______|     \____/|_| |_|_|\__|\__, |____/ 
                                                                      __/ |      
                                                                     |___/       
</pre>

Plugin for using PSMove as input in Unity 5.3.4p1 (Oculus SDK 1.3).

Check out this [video](https://www.youtube.com/watch?v=HRLblxNbAEI&feature=youtu.be) to see some tracking in action.

This is adapted from the [psmove-ue4](https://github.com/cboulay/psmove-ue4) plugin and inspired by [UniMove](https://github.com/CopenhagenGameCollective/UniMove).

# Overview
This library is inteded as a way for Unity5 developers to iterate at their desk on tracked VR controller games/applications when you have limited or no access other advanced tracking solutions in the office. The PS3 Move controller on the PC has tracking that isn't as good as Valve's or Oculus' solution, but with some filtering, it's good enough for developer iteration, and it's super [cheap](http://www.amazon.com/Playstation-Move-Motion-Controller-3/dp/B002I0J51U). 

This library is built on top of the [psmoveapi](https://github.com/thp/psmoveapi), a C library for reading raw data from a PS move controller. As awesome as the base psmove library is, it was missing a few pieces of functionality that made it work well for VR applications. Cboulay and I have been adding new VR specific functionality in his [fork](https://github.com/cboulay/psmoveapi) of psmoveapi. My [fork](https://github.com/HipsterSloth/psmoveapi/tree/psmove_unity5) has the most recent changes, but I try and push most my work to his fork, with the exception of the minor tweaks I do to better support this Unity plugin. Point being, if you want to make tweaks to the psmoveapi.dll or psmovetracker.dll used by this plugin, you'll need to clone my [fork](https://github.com/HipsterSloth/psmoveapi/tree/psmove_unity5), not cboulay's and build it locally.

This plugin works in Windows 64-bit (7/8.1/10), though LibUSB drivers for the camera in Win7 are flakey at times.

# Working features
- Position and orientation of multiple controllers.
- Button presses trigger events.
- Trigger-button value (0-255)
- Set vibration (0-255)
- Controller color changes
- Coregistration with the Oculus DK2 camera pose estimation tool
- Calibration mat based tracking camera pose estimation tool (https://www.youtube.com/watch?v=33cWRaCC9hU)
- MultiCamera support
- Position tracking filter (using Kalman filter or LowPass filter)
- Yaw-drift free orientation filter using Iterative Jacobian Gradient Descent algorithm

# Planned features
- Integration with psmove windows service/unix daemon (http://github.com/cboulay/PSMoveService)

# Install & Use

Read the [Wiki](https://github.com/HipsterSloth/psmove-unity5/wiki).
