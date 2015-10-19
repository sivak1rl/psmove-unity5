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

Plugin for using PSMove as input in Unity 5 PRO

Check out this [video](https://www.youtube.com/watch?v=HRLblxNbAEI&feature=youtu.be) to see some tracking in action.

This is adapted from the [psmove-ue4](https://github.com/cboulay/psmove-ue4) plugin and inspired by [UniMove](https://github.com/CopenhagenGameCollective/UniMove).

# Overview
This library is inteded as a way for Unity5 Pro developers to iterate at their desk on tracked VR controller games/applications when you have limited or no access other advanced tracking solutions in the office. The PS3 Move controller on the PC has tracking that isn't as good as Valve's or Oculus' solution, but with some filtering, it's good enough for developer iteration, and it's super [cheap](http://www.amazon.com/Playstation-Move-Motion-Controller-3/dp/B002I0J51U). 

This library is built on top of the [psmoveapi](https://github.com/thp/psmoveapi), a C library for reading raw data from a PS move controller. As awesome as the base psmove library is, it was missing a few pieces of functionality that made it work well for VR applications. Cboulay and I have been adding new VR specific functionality in his [fork](https://github.com/cboulay/psmoveapi) of psmoveapi. My [fork](https://github.com/brendanwalker/psmoveapi) has the most recent changes, but I push all my work to his fork.

This plugin works in Windows 64-bit (7/8.1). It should work in OS X but I still need to build the psmove libs on that platform.

# Working features
- Position and orientation of multiple controllers.
- Button presses trigger events.
- Trigger-button value (0-255)
- Set vibration (0-255)
- Controller color changes
- Coregistration with the Oculus DK2
- Position tracking filter (using Kalman filter or LowPass filter)
- Yaw-drift free orientation filter using Iterative Jacobian Gradient Descent algorithm

# Planned features
- Improving ps3eye camera driver performance (pretty terrible frame capture perf at the moment due to libusb in Windows)
- Integration with psmove windows service/unix daemon
- Elminate the requirement to need Unity Pro (get the controller data via socket connection to the service)

# Install & Use

Read the [Wiki](https://github.com/brendanwalker/psmove-unity5/wiki).
