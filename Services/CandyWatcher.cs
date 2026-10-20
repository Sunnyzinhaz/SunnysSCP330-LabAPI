using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using LabApi.Features.Wrappers;
using LabApi.Features.Console;
using InventorySystem.Items.Usables.Scp330;
using MEC;

namespace SunnysSCP330.Services
{
    // Compara o conteudo da bolsa; nao depende de PlayerUsedItemEventArgs revelar a cor.
    // Limite: uma remocao por drop pode ser confundida com consumo; use como camada complementar.
    internal sealed class CandyWatcher
    {
        readonly Settings _settings;
        readonly Action<object,string> _consume;
        readonly Dictionary<object,CandyKindID[]> _snapshots = new Dictionary<object,CandyKindID[]>();
        readonly Dictionary<object,DateTime> _last = new Dictionary<object,DateTime>();
        CoroutineHandle _handle;
        public CandyWatcher(Settings settings, Action<object,string> consume){_settings=settings;_consume=consume;}
        public void Start(){_handle=Timing.RunCoroutine(Watch());}
        public void Stop(){Timing.KillCoroutines(_handle);_snapshots.Clear();_last.Clear();}
        IEnumerator<float> Watch()
        {
            yield return Timing.WaitForSeconds(.4f);
            while(true)
            {
                try
                {
                    var players = Player.List.ToArray();
                    foreach(var player in players)
                    {
                        var bag = GetBag(player);
                        var current=bag?.Candies?.ToArray() ?? Array.Empty<CandyKindID>();
                        if(_snapshots.TryGetValue(player,out var old))
                        {
                            if(old.Length==current.Length+1)
                            {
                                var kinds=old.ToList();
                                foreach(var item in current) kinds.Remove(item);
                                if(kinds.Count==1 && kinds[0]!=CandyKindID.None)
                                {
                                    if(!_last.TryGetValue(player,out var last) || (DateTime.UtcNow-last).TotalMilliseconds>500)
                                    {
                                        _last[player]=DateTime.UtcNow;
                                        _consume(player,kinds[0].ToString());
                                    }
                                }
                            }
                        }
                        _snapshots[player]=current;
                    }
                    foreach(var gone in _snapshots.Keys.Except(players).ToArray()){_snapshots.Remove(gone);_last.Remove(gone);}
                }
                catch(Exception e){if(_settings.Debug)Logger.Warn("SCP330 watcher: "+e.Message);}
                yield return Timing.WaitForSeconds(Math.Max(.05f,_settings.CheckIntervalSeconds));
            }
        }
        static Scp330Bag GetBag(Player player)
        {
            try
            {
                foreach(var item in player.ReferenceHub.inventory.UserInventory.Items.Values)
                    if(item is Scp330Bag bag) return bag;
            }
            catch { }
            return null;
        }
    }
}
