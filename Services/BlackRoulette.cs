using System;
using System.Linq;
using System.Collections.Generic;
using LabApi.Features.Wrappers;
using UnityEngine;
using InventorySystem.Items.Usables.Scp330;

namespace SunnysSCP330.Services
{
    // Recria as opcoes da roleta original sem depender de Exiled.API.
    // Chamadas nativas que mudam por versao usam Native e reportam falhas.
    internal sealed class BlackRoulette
    {
        readonly Settings c;
        readonly Random rnd;
        readonly Action<object,float,float> gravity;
        public BlackRoulette(Settings c,Random rnd,Action<object,float,float> gravity){this.c=c;this.rnd=rnd;this.gravity=gravity;}
        void Hint(object p,string msg)=>Native.Hint(p,"<b><color=#aaaaaa>PRETO:</color> "+msg+"</b>",c.HintDuration);
        bool Do(object p,string method,params object[] args)=>Native.TryInvoke(p,method,args);
        void Item(object p,string name) { if(!Native.AddItem(p,name))Hint(p,"Não foi possível adicionar "+name); }
        void Effect(object p,string name,byte intensity,float secs)=>Native.ApplyEffect(p,name,intensity,secs);
        void Role(object p,string name)=>Native.SetRole(p,name);
        void All(Action<object> action) {foreach(var p in Player.List)action(p);}
        void Teleport(object p,object other) {var pos=Native.Get(other,"Position");if(pos!=null)Native.Set(p,"Position",pos);}
        public void Execute(object p)
        {
            var options=new List<Action>();
            if(c.BlackSpectator)options.Add(()=>{Role(p,"Spectator");Hint(p,"Você deixou de existir.");});
            if(c.BlackAntiCola)options.Add(()=>{Effect(p,"AntiScp207",1,12);Effect(p,"Flashed",1,2);Hint(p,"Bênção ou maldição?");});
            if(c.BlackSpeedThenSlow)options.Add(()=>{Effect(p,"MovementBoost",80,7);MEC.Timing.CallDelayed(7,()=>Effect(p,"Slowness",70,8));Hint(p,"Rápido... por enquanto.");});
            if(c.BlackPinkCandy)options.Add(()=>{Native.AddCandy(p,"Pink");Hint(p,"Ganhou um doce rosa.");});
            if(c.BlackDoubleBlack)options.Add(()=>{Native.AddCandy(p,"Black");Native.AddCandy(p,"Black");Hint(p,"Dois pretos!");});
            if(c.BlackNearestScpTeleport)options.Add(()=>{var target=Player.List.FirstOrDefault(x=>!ReferenceEquals(x,p) && (Native.Get(Native.Get(x,"Role"),"Type")?.ToString()??"").StartsWith("Scp"));if(target!=null)Teleport(p,target);Hint(p,"Visitou um SCP.");});
            if(c.BlackCoin)options.Add(()=>{Item(p,"Coin");Hint(p,"Uma moeda!");});
            if(c.BlackRandomNonLczTeleport)options.Add(()=>{var arr=Player.List.Where(x=>!ReferenceEquals(x,p)).ToArray();if(arr.Length>0)Teleport(p,arr[rnd.Next(arr.Length)]);Hint(p,"Teleporte aleatório.");});
            if(c.BlackEveryoneCoin)options.Add(()=>{All(x=>Item(x,"Coin"));Hint(p,"Todos ganharam moedas.");});
            if(c.BlackEveryoneCandy)options.Add(()=>{All(x=>Native.AddCandy(x,"Pink"));Hint(p,"Todos ganharam doce.");});
            if(c.BlackAuraFarming)options.Add(()=>{Effect(p,"MovementBoost",25,10);Hint(p,"Aura farming.");});
            if(c.BlackReviveSpectator)options.Add(()=>{var x=Player.List.FirstOrDefault(q=>(Native.Get(Native.Get(q,"Role"),"Type")?.ToString()??"")=="Spectator");if(x!=null)Role(x,"ClassD");Hint(p,"Um espectador reviveu.");});
            if(c.BlackSwapClass)options.Add(()=>{var x=Player.List.Where(q=>!ReferenceEquals(q,p)).OrderBy(q=>rnd.Next()).FirstOrDefault();if(x!=null){var a=Native.Get(Native.Get(p,"Role"),"Type")?.ToString();var b=Native.Get(Native.Get(x,"Role"),"Type")?.ToString();if(a!=null&&b!=null){Role(p,b);Role(x,a);}}Hint(p,"Troca de classe!");});
            if(c.BlackBecomeScp)options.Add(()=>{Role(p,new[]{"Scp049","Scp173","Scp096","Scp106","Scp939"}[rnd.Next(5)]);Hint(p,"Você virou um SCP!");});
            if(c.BlackEvilCandy)options.Add(()=>{Native.AddCandy(p,"Evil");Hint(p,"Ganhou um Evil.");});
            if(c.BlackChosenOne)options.Add(()=>{Effect(p,"MovementBoost",100,14);Effect(p,"Vitality",1,14);Hint(p,"Você foi escolhido!");});
            if(c.BlackHeal)options.Add(()=>{var max=Native.Get(p,"MaxHealth");if(max!=null)Native.Set(p,"Health",max);Hint(p,"Vida recuperada.");});
            if(c.BlackCassie67)options.Add(()=>{Native.Cassie("6 7");Hint(p,"CASSIE 6 7.");});
            if(c.BlackNukeFlip)options.Add(()=>{Native.NukeFlip();Hint(p,"A ogiva mudou de estado.");});
            if(c.BlackLanternEnjoyer)options.Add(()=>{Native.ClearInventory(p);for(int i=0;i<5;i++)Item(p,"Lantern");Hint(p,"Lanternas para todos os lados.");});
            if(c.BlackJail)options.Add(()=>{Effect(p,"Disabled",1,c.BlackJailDuration);Hint(p,"Prisão temporária.");});
            if(c.BlackRandomPlayerTeleport)options.Add(()=>{var arr=Player.List.Where(x=>!ReferenceEquals(x,p)).ToArray();if(arr.Length>0)Teleport(p,arr[rnd.Next(arr.Length)]);Hint(p,"Foi visitar alguém.");});
            if(c.BlackDreamTeleport)options.Add(()=>{Effect(p,"Amnesia",1,12);Hint(p,"Uma viagem estranha.");});
            if(c.BlackIntercom)options.Add(()=>{Native.Intercom(p,c.BlackIntercomDuration);Hint(p,"Hora de falar!");});
            if(c.BlackFloat)options.Add(()=>{gravity(p,c.BlackFloatRise?c.BlackFloatUpwardGravity:c.BlackFloatNeutralGravity,c.BlackFloatDuration);Hint(p,"Você está flutuando! (sem Position)");});
            if(c.BlackBlackForEveryone)options.Add(()=>{All(x=>Native.AddCandy(x,"Black"));Hint(p,"Preto para todos.");});
            if(c.BlackColaEnjoyer)options.Add(()=>{Native.ClearInventory(p);for(int i=0;i<5;i++)Item(p,"SCP207");Hint(p,"Só cola!");});
            if(c.BlackCoinEnjoyer)options.Add(()=>{Native.ClearInventory(p);for(int i=0;i<5;i++)Item(p,"Coin");Hint(p,"Só moedas!");});
            if(c.BlackSilentPhase)options.Add(()=>{Effect(p,"Ghostly",1,c.BlackPhaseDuration);Effect(p,"SilentWalk",1,c.BlackPhaseDuration);Hint(p,"Fase fantasma.");});
            if(c.BlackCapybara)options.Add(()=>{Native.Set(p,"Position",new Vector3(65.059f,-90.243f,9.156f));Hint(p,"Capivara!");});
            if(c.BlackTantrum)options.Add(()=>{Native.CreateTantrum(p);Hint(p,"Ops... fez cocô!");});
            if(options.Count>0)options[rnd.Next(options.Count)]();
        }
    }
}
