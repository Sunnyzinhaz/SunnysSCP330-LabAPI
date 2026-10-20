using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
namespace SunnysSCP330
{
    public sealed class Settings
    {
        public bool Enabled = true;
        public float CheckIntervalSeconds = .25f;
        public float GreenGravityY = -0.9f; // muito baixa; ajuste na configuracao
        public float GreenDuration = 22f;
        public byte GreenLightweightIntensity = 190;
        public float BlackFloatGravityY = 0.35f; // leve aceleracao para cima
        public float BlackFloatDuration = 7f;
        public float BlackTantrumChance = 15f;
        public float BlackFloatChance = 25f;
        public bool BrownTantrumFallback = true;
        public bool OrangeNativeGlow = true;
        public bool EvilNativeLaugh = true;
        public float HintDuration = 5f;
        // Roleta Black preserva os 30 resultados da v1.2.6 (ativacao individual).
        public bool BlackSpectator = true;
        public bool BlackAntiCola = true;
        public bool BlackSpeedThenSlow = true;
        public bool BlackPinkCandy = true;
        public bool BlackDoubleBlack = true;
        public bool BlackNearestScpTeleport = true;
        public bool BlackCoin = true;
        public bool BlackRandomNonLczTeleport = true;
        public bool BlackEveryoneCoin = true;
        public bool BlackEveryoneCandy = true;
        public bool BlackAuraFarming = true;
        public bool BlackReviveSpectator = true;
        public bool BlackSwapClass = true;
        public bool BlackBecomeScp = true;
        public bool BlackEvilCandy = true;
        public bool BlackChosenOne = true;
        public bool BlackHeal = true;
        public bool BlackCassie67 = true;
        public bool BlackNukeFlip = true;
        public bool BlackLanternEnjoyer = true;
        public bool BlackJail = true;
        public bool BlackRandomPlayerTeleport = true;
        public bool BlackDreamTeleport = true;
        public bool BlackIntercom = true;
        public bool BlackFloat = true;
        public bool BlackBlackForEveryone = true;
        public bool BlackColaEnjoyer = true;
        public bool BlackCoinEnjoyer = true;
        public bool BlackSilentPhase = true;
        public bool BlackCapybara = true;
        public bool BlackTantrum = true;
        public float BlackJailDuration = 60f;
        public float BlackPhaseDuration = 10f;
        public float BlackIntercomDuration = 5f;
        public float BlackFloatUpwardGravity = 0.18f;
        public float BlackFloatNeutralGravity = -0.08f;
        public bool BlackFloatRise = true;
        public bool UseNativeWatcher = true;
        public bool Debug = false;
        public static Settings Load(string path)
        {
            var s = new Settings();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            if (File.Exists(path)) foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim(); if (line.Length == 0 || line.StartsWith("#") || !line.Contains("=")) continue;
                int ix = line.IndexOf('='); var key = line.Substring(0,ix).Trim(); var val=line.Substring(ix+1).Trim();
                var f=typeof(Settings).GetField(key); if(f==null)continue;
                try { if(f.FieldType==typeof(bool))f.SetValue(s,bool.Parse(val)); else if(f.FieldType==typeof(float))f.SetValue(s,float.Parse(val,CultureInfo.InvariantCulture)); else if(f.FieldType==typeof(byte))f.SetValue(s,byte.Parse(val,CultureInfo.InvariantCulture)); } catch { }
            }
            using(var writer=new StreamWriter(path,false)) foreach(var field in typeof(Settings).GetFields()) writer.WriteLine(field.Name+"="+Convert.ToString(field.GetValue(s),CultureInfo.InvariantCulture));
            return s;
        }
    }
}