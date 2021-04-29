# The Virtual Reality Hand Redirection Toolkit (HaRT)

<Bild aus Wiki>
  
The Virtual Reality Hand Redirection Toolkit (HaRT) is an open-source toolkit developed for the Unity engine. The toolkit aims
to support VR researchers and developers in implementing and evaluating hand redirection techniques. It provides implementations of popular redirection algorithms and exposes a modular class hierarchy for easy integration of new approaches. Moreover, simulation, logging, and visualization features allow users of the toolkit to analyze hand redirection setups with minimal technical effort.

## What is the Virtual Reality Hand Redirection Toolkit?

To learn about this toolkit, please watch our video on YouTube:


[![HaRT Teaser Video](../../wiki/uploads/ReadmeTeaserImg.png)](http://www.youtube.com/watch?v=w_HhzcV0ndM)


For further details, please have a look at the [paper about the HaRT](https://doi.org/10.1145/3411763.3451814) published at ACM CHI 2021.

<Bild von Paper als Link zur DOI>

## Get Started
To get started with the toolkit **visit the [Wiki](../../wiki)**. The wiki contains short instructions as well as step-by-step tutorials and further details about the toolkit.

## Downloads
- [HaRT_core](Packages/HaRT_core.unitypackage) -> standalone Unity Version (No VR), requires the HaRT_core package and [SteamVR](https://assetstore.unity.com/packages/tools/integration/steamvr-plugin-32647)
- [HaRT_Leap](Packages/HaRT_leap.unitypackage) -> requires the HaRT_core and [Leap Motion SDK](https://developer.leapmotion.com/unity)

For more details, visit our [Get Started Guide in the Wiki](../../wiki/Get-Started)

## Development
We have created and tested the toolkit only on Windows 10 and Unity 2019.4, but as long as the Unity version supports the corresponding VR headset / the Leap Motion, it should work. We tested the toolkit with the Oculus Rift and the HTC Vive but it should work with every VR headset that is either compatible with the SteamVR Unity Plugin or with the LeapMotion SDK. We used the Unity SteamVR plugin version 2.7.2 and the Leap Motion Orion SDK version 4.6.0. <br>
**There is no VR system necessary to run the toolkit**. It also works with mouse and keyboard.

## Reference

If you use the toolkit for one of your cool projects, please reference the toolkit given the information below and feel free to drop us a message:

``` 
TODO BIBTEX HERE
```

> TODO REFERENCE

## Contact

This toolkit was created by Hannah Kriegler as part of a Bachelor Thesis at the [Ubiquitous Media Technology Lab](https://umtl.cs.uni-saarland.de/) ([Saarland University](https://www.uni-saarland.de/start.html)), advised by [André Zenner](https://umtl.cs.uni-saarland.de/people/andre-zenner.html).
If you have any questions, feel free to contact [Hannah](hannah.kriegler@dfki.de) or [André](andre.zenner@dfki.de)
