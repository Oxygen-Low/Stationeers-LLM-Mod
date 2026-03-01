# Self-Hosted Runner Requirements & Setup

This repository uses a self-hosted GitHub Actions runner on Ubuntu for building and releasing the LLMPlayer mod.

## Prerequisites for the Runner (Ubuntu Server)

1.  **Git**: For checking out the repository.
    ```bash
    sudo apt update
    sudo apt install git -y
    ```
2.  **.NET 8 SDK**: For building the .NET project.
    ```bash
    sudo apt-get update && \
      sudo apt-get install -y dotnet-sdk-8.0
    ```
3.  **GitHub Runner Software**:
    Follow the official GitHub instructions for adding a self-hosted runner to your repository (**Settings > Actions > Runners > New self-hosted runner**).

## Dependency Setup

The build process expects essential Stationeers and BepInEx DLLs to be available in a `files/` folder in the root of the repository during the build.

To ensure the build succeeds:
1. Create a folder named `files/` in the repository root (this should be done locally before pushing, or by the runner if it persists).
2. Upload the following files from your local Stationeers installation to the `files/` folder:
   - `rocketstation_Data/Managed/Assembly-CSharp.dll`
   - `rocketstation_Data/Managed/UnityEngine.dll`
   - `rocketstation_Data/Managed/UnityEngine.CoreModule.dll`
   - `rocketstation_Data/Managed/UnityEngine.ImageConversionModule.dll`
   - `rocketstation_Data/Managed/UnityEngine.InputLegacyModule.dll`
   - `rocketstation_Data/Managed/UnityEngine.UI.dll`
   - `rocketstation_Data/Managed/UnityEngine.UIModule.dll`
   - `rocketstation_Data/Managed/UnityEngine.AssetBundleModule.dll`
   - `rocketstation_Data/Managed/UnityEngine.PhysicsModule.dll`
   - `rocketstation_Data/Managed/UnityEngine.UnityWebRequestModule.dll`
   - `rocketstation_Data/Managed/Newtonsoft.Json.dll`
   - `BepInEx/core/BepInEx.dll`
   - `BepInEx/core/BepInEx.Core.dll`
   - `BepInEx/core/0Harmony.dll`

The `LLMPlayer.props` file has been configured to look for these files in the `files/` directory relative to the repository root.

## Troubleshooting Builds on Linux

- **NuGet Package**: The project uses `Microsoft.NETFramework.ReferenceAssemblies` to allow building .NET Framework 4.7.2 projects on Linux.
- **Paths**: Ensure all file names in the `files/` folder match the casing exactly as they are in the Unity/BepInEx installation (usually CamelCase like `Assembly-CSharp.dll`). Ubuntu is case-sensitive.
