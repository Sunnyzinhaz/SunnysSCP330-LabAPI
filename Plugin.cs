using System;
using System.IO;
using LabApi;
using LabApi.Features.Console;
using LabApi.Loader.Features.Plugins;
using SunnysSCP330.Services;
namespace SunnysSCP330
{
    public sealed class Plugin : LabApi.Loader.Features.Plugins.Plugin
    {
        public static Plugin Instance { get; private set; }
        public Settings Settings { get; private set; }
        private CandyEngine _engine;
        public override string Name => "SunnysSCP330";
        public override string Author => "Sunnyz";
        public override string Description => "Doces personalizados SCP-330 para LabAPI";
        public override Version Version => new Version(2, 1, 1);
        public override Version RequiredApiVersion => new Version(LabApiProperties.CompiledVersion);
        public override void Enable()
        {
            Instance = this;
            var cfg = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SCP Secret Laboratory", "LabAPI", "SunnysSCP330.ini");
            Settings = Settings.Load(cfg);
            _engine = new CandyEngine(Settings);
            _engine.Start();
            Logger.Info("SunnysSCP330 v2.1: sistema de doces habilitado; configuracao: " + cfg);
        }
        public override void Disable()
        {
            _engine?.Stop();
            _engine = null;
            Instance = null;
        }
    }
}