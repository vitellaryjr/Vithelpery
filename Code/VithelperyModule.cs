using Celeste.Mod.Vithelpery.Entities;
using System;

namespace Celeste.Mod.Vithelpery;

public class VithelperyModule : EverestModule {
    public static VithelperyModule Instance { get; private set; }

    public override Type SettingsType => typeof(VithelperyModuleSettings);
    public static VithelperyModuleSettings Settings => (VithelperyModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(VithelperyModuleSession);
    public static VithelperyModuleSession Session => (VithelperyModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(VithelperyModuleSaveData);
    public static VithelperyModuleSaveData SaveData => (VithelperyModuleSaveData) Instance._SaveData;

    public VithelperyModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(VithelperyModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(VithelperyModule), LogLevel.Info);
#endif
    }

    public override void Load() {
        // Entities
        CountBooster.Load();
    }

    public override void Unload() {
        CountBooster.Unload();
    }
}