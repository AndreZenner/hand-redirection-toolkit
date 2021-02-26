# Hand-Redirection-Toolkit

The Virtual Reality Hand Redirection Toolkit (HaRT) is an open-source toolkit developed for the Unity engine. The toolkit aims
to support VR researchers and developers in implementing and evaluating hand redirection techniques. It provides implementations of popular redirection algorithms and exposes a modular class hierarchy for easy integration of new approaches. Moreover, simulation, logging, and visualization features allow users of the toolkit to analyze hand redirection setups with minimal technical effort.

## Get Started
To get started with the toolkit read over the [Paper]() and visit the [Wiki](../../wiki). The wiki contains short instructions as well as step-by-step tutorials and further details about the toolkit.

## Downlaods
- [HaRT_core](Packages/HaRT_core.unitypackage) -> standalone Unity Version (No VR)
- [HaRT_VR](Packages/HaRT_VR.unitypackage) -> requires the HaRT_core package and [SteamVR](https://assetstore.unity.com/packages/tools/integration/steamvr-plugin-32647)
- [HaRT_Leap](Packages/HaRT_leap.unitypackage) -> requires the HaRT_core, HaRT_VR packages and [Leap Motion SDK](https://developer.leapmotion.com/unity)

For more details, visit our [Wiki](../../wiki)

## Development
We have created and tested the toolkit only on Windows 10 and Unity 2019.4, but as long as the Unity version supports the corresponding VR headset / the Leap Motion, it should work. We tested the toolkit with the Oculus Rift and the HTC Vive but it should work with every VR headset that is either compatible with the SteamVR Unity Plugin or with the LeapMotion SDK. We used the Unity SteamVR plugin version 2.7.2 and the Leap Motion Orion SDK version 4.6.0. <br>
**There is no VR system necessary to run the toolkit**. It also works with mouse and keyboard.
