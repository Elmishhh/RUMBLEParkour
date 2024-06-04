using MelonLoader;
using System.Reflection;

[assembly: AssemblyTitle(RUMBLEParkour.BuildInfo.Description)]
[assembly: AssemblyDescription(RUMBLEParkour.BuildInfo.Description)]
[assembly: AssemblyCompany(RUMBLEParkour.BuildInfo.Company)]
[assembly: AssemblyProduct(RUMBLEParkour.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + RUMBLEParkour.BuildInfo.Author)]
[assembly: AssemblyTrademark(RUMBLEParkour.BuildInfo.Company)]
[assembly: AssemblyVersion(RUMBLEParkour.BuildInfo.Version)]
[assembly: AssemblyFileVersion(RUMBLEParkour.BuildInfo.Version)]
[assembly: MelonInfo(typeof(RUMBLEParkour.RumbleParkour), RUMBLEParkour.BuildInfo.Name, RUMBLEParkour.BuildInfo.Version, RUMBLEParkour.BuildInfo.Author, RUMBLEParkour.BuildInfo.DownloadLink)]
[assembly: MelonColor()]

// Create and Setup a MelonGame Attribute to mark a Melon as Universal or Compatible with specific Games.
// If no MelonGame Attribute is found or any of the Values for any MelonGame Attribute on the Melon is null or empty it will be assumed the Melon is Universal.
// Values for MelonGame Attribute can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame(null, null)]