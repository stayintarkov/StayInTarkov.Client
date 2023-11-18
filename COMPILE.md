## How to compile? 
1. Create Working Directory for all Tarkov Modding {EFT_WORK}
2. Clone this {SIT_CORE} to a {SIT_CORE} directory inside {EFT_WORK}
3. Copy your Live Tarkov Directory somewhere else {EFT_OFFLINE}
4. Deobfuscate latest Assembly-CSharp in {EFT_OFFLINE} via [SIT.Launcher](https://github.com/paulov-t/SIT.Tarkov.Launcher). Ensure to close and restart Launcher after Deobfuscation.
5. Copy all of {EFT_OFFLINE}\EscapeFromTarkov_Data\Managed assemblies to References {TARKOV.REF} in the folder of this project {EFT_WORK}
6. You will need BepInEx Nuget Feed installed on your PC by running the following command in a terminal. 
```
dotnet new -i BepInEx.Templates --nuget-source https://nuget.bepinex.dev/v3/index.json
```
7. Open the .sln with Visual Studio 2022
8. Rebuild Solution (This should download and install all nuget packages on compilation)

## Which version of BepInEx is this built for?
Version 5