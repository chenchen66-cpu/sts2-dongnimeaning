using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Modding;

namespace DongniDefense.DongniDefenseCode;

//You're recommended but not required to keep all your code in this package and all your assets in the DongniDefense folder.
[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "DongniDefense"; //Used for resource filepath
    public const string ResPath = $"res://{ModId}";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();

        //If you want to use scripts defined in your mod for Godot scenes, uncomment the following line.
        //Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(assembly);

        Harmony harmony = new(ModId);

        harmony.PatchAll(assembly);

        LogContentDiagnostics();
    }

    /// <summary>
    /// Logs the ids the card and power end up with, plus whether their art was found inside the mod's .pck.
    /// Handy for checking a build: the game log then shows the exact localization keys and image paths.
    /// </summary>
    private static void LogContentDiagnostics()
    {
        try
        {
            string cardEntry = ModelDb.GetId(typeof(global::DongniDefense.DongniDefenseCode.Cards.DongniDefense)).Entry;
            string powerEntry = ModelDb.GetId(typeof(global::DongniDefense.DongniDefenseCode.Powers.DongniDefensePower)).Entry;
            string strikeEntry = ModelDb.GetId(typeof(global::DongniDefense.DongniDefenseCode.Cards.DongniStrike)).Entry;
            string curseEntry = ModelDb.GetId(typeof(global::DongniDefense.DongniDefenseCode.Cards.DongniCurse)).Entry;
            string eventEntry = ModelDb.GetId(typeof(global::DongniDefense.DongniDefenseCode.Events.DongniMeaning)).Entry;

            Logger.Info($"[{ModId}] defense card/power ids: {cardEntry} / {powerEntry}");
            Logger.Info($"[{ModId}] strike card id: {strikeEntry}");
            Logger.Info($"[{ModId}] curse card id: {curseEntry}");
            Logger.Info($"[{ModId}] event id: {eventEntry}");
            Logger.Info($"[{ModId}] art found (small/big): defense {ResourceLoader.Exists(ArtPath("card_portraits/dongni_defense.png"))}/{ResourceLoader.Exists(ArtPath("card_portraits/big/dongni_defense.png"))}, " +
                        $"strike {ResourceLoader.Exists(ArtPath("card_portraits/dongni_strike.png"))}/{ResourceLoader.Exists(ArtPath("card_portraits/big/dongni_strike.png"))}, " +
                        $"curse {ResourceLoader.Exists(ArtPath("card_portraits/dongni_curse.png"))}/{ResourceLoader.Exists(ArtPath("card_portraits/big/dongni_curse.png"))}");
            Logger.Info($"[{ModId}] power icon found: {ResourceLoader.Exists(ArtPath("powers/dongni_defense_power.png"))} / big {ResourceLoader.Exists(ArtPath("powers/big/dongni_defense_power.png"))}");
            Logger.Info($"[{ModId}] event art found: {ResourceLoader.Exists(ArtPath("events/dongni_meaning.png"))}");
        }
        catch (Exception e)
        {
            Logger.Warn($"[{ModId}] content diagnostics failed: {e}");
        }
    }

    private static string ArtPath(string innerPath) => $"{ResPath}/images/{innerPath}";
}
