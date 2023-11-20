# Welcome to the Enemies cinematic demo project.

## Getting started

There are two Unity scenes that need to be loaded for the project to work as expected: 'Bootstrap.unity' and 'Enemies.unity', where 'Enemies' is expected to be the active scene. There is a convenience tool for loading those scenes in the main menu at 'Enemies -> Load Scenes'.

As a cinematic experience, the primary way of interacting with the project is by using Timeline. The main timeline asset in the project is called '_MainTimeline' and is bound to a playable director object called 'Timeline_Main' in the 'Enemies' scene. It is important that the playable object in the scene is selected and not the timeline asset (otherwise scrubbing timeline won't have any effect on the current scene). It is also highly recommended to lock selection in the Timeline window (tiny padlock on the right) otherwise the timeline will be deselected and reset the scene state whenever any other object is interacted with. There is also a convenience tool for opening timeline and locking in the master director player at 'Enemies -> Open Timeline'. From the master timeline window, one can double click on any of the control tracks to drill-down to the various child-timelines for camera, performance, vfx and so on.

Please be aware that Enemies uses nearly all available features in Unity High-Definition Render Pipeline and as such when you freshly import the project there are A LOT of shaders that have not yet been compiled and cached. Until the required shaders have been cached in the project you are likely to experience frequent stalls and hiccups in the editor. While this might be a bit frustrating as a first-time experience, it will cease happening once everything in the project has been seen or visited once and things should be smooth from there.

Learn more about Enemies here: https://unity.com/demos/enemies


## Supported platforms and system requirements

The official development environment for the project is Windows 10/11 running DirectX 12 on a reasonably modern gaming computer. While there are no strict hardware requirements, something in the ballpark of an Intel i7-9700K, NVIDIA GeForce RTX 2080, 32 GB RAM, SSD should be considered the minimum spec for having a pleasant experience in the Editor.

Although it's possible to override the quality preset used in playmode, there is no traditional range of quality presets that can load the editor into a significantly cheaper mode; it is assumed that development stations roughly meet the suggested hardware range. If the default settings are too heavy, a suggestion is to tweak the quality settings and HDRP asset as required, or in a pinch load into DX11 which will implicitly disable all ray-tracing effects. To preview a specific quality preset in editor playmode, change the value of 'Editor Playmode Default Quality' on the 'QuickSettings' object in the 'Bootstrap' scene - be aware that this will not force any resolution change - that has to be specified separately in the game view.

System requirements for running the standalone are lower since it can be more easily scaled down to lower quality settings and lower resolution. While the project is intended to be enjoyed with real-time ray-tracing enabled, these effects are successively disabled on lower quality settings allowing the cinematic to be run on most modern computers with a dedicated GPU.

Platforms and APIs like macOS, Linux, DX11, Metal, Vulkan are considered unsupported but are expected to work most of the time (be aware that there are some issues in the current beta releases).


## Currently known issues:

- [HDRP-2884] Line rendering is not declaring to render graph its intention to read shadows which can lead to instabilities in certain cases
- [UUM-31322] Some Async compute effects get unintentionally disabled when ray-tracing is enabled 


## Building standalone for desktop platforms

There are no special steps required, just follow the normal procedure for making standalone builds using the existing project and build settings (https://docs.unity3d.com/2023.1/Documentation/Manual/BuildSettings.html).


## Building standalone for console platforms

Licensed developers need to acquire the version matching platform module as well as the corresponding extension packages for Render Pipelines and Input System from their respective platform partners. Once the platform module is installed, and the packages added to the project, just switch to the desired target plaform and follow the normal build process.


## Standalone / Playmode key bindings and arguments

PLease see 'Assets/Meta/PlayerScripts/README-Enemies.txt' for a description of input bindings and command-line launch arguments available in playmode and standalone builds.
