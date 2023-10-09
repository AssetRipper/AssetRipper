# Contributing to AssetRipper

## First Time Contributors

You can find a list of "good first issues" [here](https://github.com/AssetRipper/AssetRipper/contribute). These issues are selected for their relative approachability to first-time contributors.


## Pull Requests
I welcome contributions to AssetRipper and have accepted many patches, but if you want your code to be included, please familiarize yourself with the following guidelines:
* Your submission must be your own work, and provided under the terms of the [contributor license agreement](https://github.com/AssetRipper/ContributorLicenseAgreement).
* You will need to make sure your code conforms to the layout and naming conventions used elsewhere in AssetRipper.
* Try to write "clean code" - avoid long functions and long classes. Try to add a new feature by creating a new class rather than putting loads of extra code inside an existing one.
* Unless your patch is a small bugfix, I will code review it and give you feedback. You will need to be willing to make the recommended changes before it can be integrated into the main code.
* Patches should be provided using the Pull Request feature of GitHub.


## Localizations

AssetRipper has a built-in localization system, so that people can use it in their native language/dialect if localized text has been contributed. 

A web platform has been integrated to make contributing as easy as possible. You can get started [here](http://weblate.samboy.dev/engage/assetripper/).

You'll need to sign up to start translating to a new localization, or you can suggest translations for an existing localization without an account (but someone with an account will have to manually verify your suggestions). This will send an email to the address you entered. Be sure to check your spam folder, as Gmail sometimes redirects the email there.


## Issues

Posting well-written issues is an excellent way to report bugs and share ideas about how AssetRipper can be improved.

Before posting an issue, make sure it hasn't already been addressed in the [previously posted issues](https://github.com/AssetRipper/AssetRipper/issues?q=is%3Aissue).

If posting a bug report, please be sure to include your log file. It is in the same folder as the `exe` file and will be called `AssetRipper.log`.


## Compiling

AssetRipper uses Electron.NET for its user interface, which has a slightly more complicated build process than a standard .NET Core application.

1. Install the [ElectronNET.CLI](https://www.nuget.org/packages/ElectronNET.CLI/) tool.

```
dotnet tool install ElectronNET.CLI -g
```

If already installed, it can be updated with the following command:

```
dotnet tool update ElectronNET.CLI -g
```

2. Install a node version manager. I use [nvm-windows](https://github.com/coreybutler/nvm-windows).

3. Use the node version manager to install and enable a recent version of node.

4. Change directory to the GUI project.

```
cd ./Source/AssetRipper.GUI.Electron
```

5. Launch the application.

```
electronize start /PublishSingleFile false /PublishReadyToRun false
```

There is a simple script, `start.bat`, that can replace steps 4 and 5. Double-clicking it will launch the application.

6. Attach a debugger, if desired.

There is a button for this in the `Window` menu.