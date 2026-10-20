using Logger = LabApi.Features.Console.Logger;
using Random = System.Random;
using System;
using System.Collections.Generic;
using LabApi.Events.Handlers;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Console;
using UnityEngine;
using MEC;
using System.Collections;
namespace SunnysSCP330.Services
{
    internal sealed class CandyEngine
    {
        private readonly Settings _config;
        private readonly Random _random = new Random();
        private readonly Dictionary<object,GravityLease> _gravity = new Dictionary<object,GravityLease>();
        private CoroutineHandle _expiryWatcher;
        private CandyWatcher _watcher;
        internal CandyEngine(Settings config) { _config=config; }
        internal void Start() { PlayerEvents.Left += Left; _expiryWatcher = Timing.RunCoroutine(ExpireGravity()); _watcher = new CandyWatcher(_config, Apply); _watcher.Start(); }
        internal void Stop() { _watcher?.Stop(); PlayerEvents.Left -= Left; Timing.KillCoroutines(_expiryWatcher); foreach(var x in _gravity.Values)x.Dispose(); _gravity.Clear(); }
        private IEnumerator<float> ExpireGravity() { while(true) { yield return Timing.WaitForSeconds(.15f); ClearExpired(); } }
        private void ClearExpired() { var expired=new List<object>();foreach(var pair in _gravity)if(pair.Value.Expired)expired.Add(pair.Key);foreach(var key in expired){_gravity[key].Dispose();_gravity.Remove(key);} }
        private void Left(PlayerLeftEventArgs e) { if(_gravity.TryGetValue(e.Player,out var lease)){lease.Dispose();_gravity.Remove(e.Player);} }
        private void UsedItem(PlayerUsedItemEventArgs e)
        {
            var expired=new List<object>();foreach(var pair in _gravity)if(pair.Value.Expired)expired.Add(pair.Key);foreach(var key in expired){_gravity[key].Dispose();_gravity.Remove(key);}
            if(!_config.Enabled || e.Player==null)return;
            var item=Native.Get(e,"Item");
            var kind=Native.Get(item,"Type")??Native.Get(item,"ItemTypeId")??Native.Get(item,"CandyType")??Native.Get(e,"CandyType");
            var name=kind?.ToString()??"";
            if(name.IndexOf("Candy",StringComparison.OrdinalIgnoreCase)<0 && name.IndexOf("Scp330",StringComparison.OrdinalIgnoreCase)<0)return;
            // Algumas versoes reportam somente ItemType.SCP330: tentar reconhecer pela bolsa.
            if(name.Equals("Scp330",StringComparison.OrdinalIgnoreCase)||name.Equals("SCP330",StringComparison.OrdinalIgnoreCase))
            { var candy=Native.Get(e,"Candy")??Native.Get(e,"CandyType")??Native.Get(item,"Candy");name=candy?.ToString()??""; }
            if(name.Length==0){if(_config.Debug)Logger.Warn("SunnysSCP330: consumiu SCP330 sem tipo de doce informado pelo evento.");return;}
            Apply(e.Player,name);
        }
        internal void Apply(object player,string value)
        {
            var candy=value.Replace("CandyKindID.","").Replace("Candy","").Trim();
            if(_config.Debug)Logger.Info("SunnysSCP330: doce detectado = "+candy);
            switch(candy.ToLowerInvariant())
            {
                case "green":
                    Gravity(player,_config.GreenGravityY,_config.GreenDuration);
                    Native.ApplyEffect(player,"Lightweight",_config.GreenLightweightIntensity,_config.GreenDuration);
                    Native.Hint(player,"<color=green>Gravidade lunar extrema!</color>",_config.HintDuration); break;
                case "black":
                    new BlackRoulette(_config, _random, (p,y,d)=>Gravity(p,y,d)).Execute(player);
                    break;
                case "brown":
                    if(!Native.PlayNative(player,"Brown") && _config.BrownTantrumFallback)Native.CreateTantrum(player);
                    Native.Hint(player,"<color=#784421>Seu estômago está fazendo barulhos...</color>",_config.HintDuration);break;
                case "orange":
                    if(_config.OrangeNativeGlow){if(!Native.PlayNative(player,"Orange"))Native.ApplyEffect(player,"Glowing",1,20f);}
                    Native.Hint(player,"<color=orange>Brilho assombrado!</color>",_config.HintDuration);break;
                case "evil":
                    Native.PlayNative(player,"Evil");
                    Native.Hint(player,"<color=red>Muahahaha!</color>",_config.HintDuration);break;
                case "white":
                    Native.ApplyEffect(player,"Ghostly",1,16f); Native.ApplyEffect(player,"SilentWalk",1,16f);Native.ApplyEffect(player,"Fade",1,16f);break;
                case "gray":
                case "grey":
                    Native.ApplyEffect(player,"HeavyFooted",50,18f);break;
                case "yellow":
                    Native.ApplyEffect(player,"MovementBoost",65,12f);break;
                case "purple":
                    Native.ApplyEffect(player,"SilentWalk",1,18f);break;
                case "red":
                    Native.ApplyEffect(player,"Vitality",1,10f);break;
                case "blue":
                    Native.ApplyEffect(player,"RainbowTaste",1,10f);break;
                case "rainbow":
                    var fx=new[]{"MovementBoost","Slowness","Vitality","Blindness","SilentWalk"};
                    Native.ApplyEffect(player,fx[_random.Next(fx.Length)],35,15f);break;
            }
        }
        private bool Roll(float percent)=>_random.NextDouble()*100<Math.Max(0,Math.Min(100,percent));
        private void Gravity(object player,float y,float duration)
        {
            if(_gravity.TryGetValue(player,out var old)){old.Dispose();_gravity.Remove(player);}
            var lease=new GravityLease(player,y,duration,()=>_gravity.Remove(player));
            if(lease.Valid)_gravity[player]=lease;
        }
    }
    internal sealed class GravityLease : IDisposable
    {
        private readonly object _player;private readonly object _previous;private readonly Action _done;
        private readonly DateTime _until; public bool Expired => DateTime.UtcNow>=_until;
        public bool Valid {get;}
        public GravityLease(object player,float y,float seconds,Action done)
        {
            _player=player;_done=done;
            _previous=Native.Get(player,"Gravity");
            if(_previous is Vector3)
            {
                // Modifica apenas gravidade; nunca Position.
                Valid=Native.Set(player,"Gravity",new Vector3(0,y,0));
                if(Valid)_until=DateTime.UtcNow.AddSeconds(Math.Max(.1,seconds));
            }
        }
        public void Dispose(){if(Valid)Native.Set(_player,"Gravity",_previous);}
    }
}