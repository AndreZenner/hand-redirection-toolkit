# The Virtual Reality Hand Redirection Toolkit (HaRT)

[![HaRT Teaser](../../wiki/uploads/HaRT_teaser.png)](../../wiki)
  
The Virtual Reality Hand Redirection Toolkit (HaRT) is an open-source toolkit developed for the Unity engine. The toolkit aims
to support VR researchers and developers in implementing and evaluating hand redirection techniques. It provides implementations of popular redirection algorithms and exposes a modular class hierarchy for easy integration of new approaches. Moreover, simulation, logging, and visualization features allow users of the toolkit to analyze hand redirection setups with minimal technical effort.

## What is the Virtual Reality Hand Redirection Toolkit?

To learn about this toolkit, please watch our video on YouTube:


[![HaRT Teaser Video](../../wiki/uploads/ReadmeTeaserImg.png)](https://youtu.be/4Gz2Sh8eduk)


For further details, please have a look at the [paper about the HaRT](https://doi.org/10.1145/3411763.3451814) published at ACM CHI 2021.

<p align="center">
  <a href="https://doi.org/10.1145/3411763.3451814">
  <img src="../../wiki/uploads/paperTeaser.png">
  </a>
</p>

## Get Started
To get started with the toolkit **visit the [Wiki](../../wiki)**. The wiki contains short instructions as well as step-by-step tutorials and further details about the toolkit.

## Downloads
- [HaRT_core](Packages/HaRT_core.unitypackage) -> standalone Unity Version (for VR and Non-VR usage); requires [SteamVR](https://assetstore.unity.com/packages/tools/integration/steamvr-plugin-32647)

optional add-on:
- [HaRT_Leap](Packages/HaRT_Leap.unitypackage) -> if you want to use Leap Motion; requires the HaRT_core package (see above), SteamVR (see above), and [Leap Motion SDK](https://developer.leapmotion.com/unity)

For more details, visit our [Get Started Guide in the Wiki](../../wiki/Get-Started)

## Development
We have created and tested the toolkit only on Windows 10 and Unity 2019.4, but as long as the Unity version supports the corresponding VR headset / the Leap Motion, it should work. We tested the toolkit with the Oculus Rift and the HTC Vive but it should work with every VR headset that is either compatible with the SteamVR Unity Plugin or with the LeapMotion SDK. We used the Unity SteamVR plugin version 2.7.2 and the Leap Motion Orion SDK version 4.6.0. <br>
**There is no VR system necessary to run the toolkit**. It also works with mouse and keyboard.

## Reference

If you use the toolkit for one of your cool projects, please reference the toolkit given the information below and feel free to drop us a message:

> André Zenner, Hannah Maria Kriegler, and Antonio Krüger. 2021. HaRT - The Virtual Reality Hand Redirection Toolkit.
> In CHI Conference on Human Factors in Computing Systems Extended Abstracts (CHI ’21 Extended Abstracts), May 8–13, 2021, Yokohama, Japan. 
> ACM, New York, NY, USA, 7 pages. https://doi.org/10.1145/3411763.3451814

``` 
@inproceedings{Zenner:2021:VRHandRedirectionToolkit,
author = {Zenner, Andr\'{e} and Kriegler, Hannah Maria and Kr\"{u}ger, Antonio},
title = {HaRT - The Virtual Reality Hand Redirection Toolkit},
year = {2021},
isbn = {9781450380959},
publisher = {Association for Computing Machinery},
address = {New York, NY, USA},
url = {https://doi.org/10.1145/3411763.3451814},
doi = {10.1145/3411763.3451814},
abstract = { Past research has proposed various hand redirection techniques for virtual reality (VR). Such techniques modify a user’s hand movements and have been successfully used to enhance haptics and 3D user interfaces. Up to now, however, no unified framework exists that implements previously proposed techniques such as body warping, world warping, and hybrid methods. In this work, we present the Virtual Reality Hand Redirection Toolkit (HaRT), an open-source framework developed for the Unity engine. The toolkit aims to support both novice and expert VR researchers and practitioners in implementing and evaluating hand redirection techniques. It provides implementations of popular redirection algorithms and exposes a modular class hierarchy for easy integration of new approaches. Moreover, simulation, logging, and visualization features allow users of the toolkit to analyze hand redirection setups with minimal technical effort. We present the architecture of the toolkit along with the results of a qualitative expert study.},
booktitle = {Extended Abstracts of the 2021 CHI Conference on Human Factors in Computing Systems},
articleno = {387},
numpages = {7},
keywords = {reach redirection, toolkit, redirected touching, haptic retargeting, hand redirection},
location = {Yokohama, Japan},
series = {CHI EA '21}
}
```

## Contact

This toolkit was created by Hannah Kriegler as part of a Bachelor Thesis at the [Ubiquitous Media Technology Lab](https://umtl.cs.uni-saarland.de/) ([Saarland University](https://www.uni-saarland.de/start.html)), advised by [André Zenner](https://umtl.cs.uni-saarland.de/people/andre-zenner.html).
If you have any questions, feel free to contact [Hannah](mailto:hannah.kriegler@dfki.de) or [André](mailto:andre.zenner@dfki.de).
