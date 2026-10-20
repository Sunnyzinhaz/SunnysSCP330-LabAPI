using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using LabApi.Features.Console;
namespace SunnysSCP330.Services
{
    // Adaptador conservador para internals que mudam com atualizacoes de SCP:SL.
    internal static class Native
    {
        public static object Get(object o,string name) { if(o==null)return null;var t=o.GetType();try{return t.GetProperty(name,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static)?.GetValue(o)??t.GetField(name,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static)?.GetValue(o);}catch{return null;} }
        public static bool Set(object o,string name,object value) { if(o==null)return false;try{var t=o.GetType();var p=t.GetProperty(name,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);if(p!=null&&p.CanWrite){p.SetValue(o,ConvertValue(value,p.PropertyType));return true;}var f=t.GetField(name,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);if(f!=null){f.SetValue(o,ConvertValue(value,f.FieldType));return true;}}catch{}return false; }
        static object ConvertValue(object v,Type t) { if(v==null)return null; if(t.IsInstanceOfType(v))return v;if(t.IsEnum)return Enum.Parse(t,v.ToString(),true);return Convert.ChangeType(v,t); }
        public static object Invoke(object o,string name,params object[] args) { if(o==null)return null;foreach(var m in o.GetType().GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Where(m=>m.Name==name&&m.GetParameters().Length==args.Length)){try{var pr=m.GetParameters();var a=new object[args.Length];for(int i=0;i<a.Length;i++)a[i]=ConvertValue(args[i],pr[i].ParameterType);return m.Invoke(o,a);}catch{}}return null; }
        public static Type FindType(string name) { foreach(var a in AppDomain.CurrentDomain.GetAssemblies()){Type t=null;try{t=a.GetTypes().FirstOrDefault(x=>x.Name==name||x.FullName==name);}catch{}if(t!=null)return t;}return null; }
        public static bool ApplyEffect(object player,string effectName,byte intensity,float duration)
        {
            var effects=Get(player,"EffectsManager")??Get(Get(player,"ReferenceHub"),"playerEffectsController");
            if(effects==null)return false;
            foreach(var m in effects.GetType().GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Where(x=>x.Name=="ChangeState"||x.Name=="EnableEffect"))
            {var ps=m.GetParameters();if(ps.Length<1)continue;try{
                var a=new object[ps.Length];var type=ps[0].ParameterType;
                if(type==typeof(string))a[0]=effectName;
                else if(type==typeof(Type))a[0]=FindType(effectName);
                else if(type.IsEnum)a[0]=Enum.Parse(type,effectName,true);
                else continue;
                if(a[0]==null)continue;
                for(int i=1;i<a.Length;i++){var pt=ps[i].ParameterType;var n=ps[i].Name.ToLowerInvariant();if(pt==typeof(byte))a[i]=intensity;else if(pt==typeof(float))a[i]=n.Contains("intens")?(float)intensity:duration;else if(pt==typeof(int))a[i]=(int)intensity;else if(pt==typeof(bool))a[i]=true;else if(ps[i].HasDefaultValue)a[i]=ps[i].DefaultValue;else a[i]=pt.IsValueType?Activator.CreateInstance(pt):null;}
                m.Invoke(effects,a);return true;
            }catch{}}
            return false;
        }
        public static void Hint(object player,string text,float seconds) { Invoke(player,"SendHint",text,seconds);Invoke(player,"ShowHint",text,seconds); }
        public static bool CreateTantrum(object player)
        {
            var pos=Get(player,"Position");
            foreach(var name in new[]{"PlaceTantrum","SpawnTantrum","CreateTantrum"}){
                var methods=player?.GetType().GetMethods().Where(m=>m.Name==name).ToArray();if(methods!=null)foreach(var m in methods)try { var ps=m.GetParameters(); if(ps.Length==0){m.Invoke(player,new object[0]);return true;} if(ps.Length==1 && pos!=null && ps[0].ParameterType.IsInstanceOfType(pos)){m.Invoke(player,new[]{pos});return true;}}catch{}
            }
            var t=FindType("TantrumEnvironmentalHazard");if(t==null)return false;
            // Sem instancia de prefab, nao arriscar criar um objeto invalido.
            return false;
        }
        public static bool PlayNative(object player,string candy)
        {
            string native=candy=="Orange"?"HauntedCandyOrange":candy=="Evil"?"HauntedCandyEvil":candy=="Brown"?"HauntedCandyBrown":null;
            if(native==null)return false;
            var type=FindType(native);if(type==null)return false;
            object instance=null;try{instance=Activator.CreateInstance(type,true);}catch{return false;}
            foreach(var method in type.GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic))
            {
                if(method.Name!="ServerProcess"&&method.Name!="ServerApply"&&method.Name!="ApplyEffect"&&method.Name!="OnConsumed")continue;
                var ps=method.GetParameters();if(ps.Length!=1)continue;
                object arg=player;var hub=Get(player,"ReferenceHub");if(!ps[0].ParameterType.IsInstanceOfType(arg))arg=hub;
                if(arg==null||!ps[0].ParameterType.IsInstanceOfType(arg))continue;
                try{method.Invoke(instance,new[]{arg});return true;}catch(Exception e){Logger.Warn("SunnysSCP330 native "+native+": "+e.Message);}
            }
            return false;
        }
        public static bool TryInvoke(object obj,string method,params object[] args)
        {
            if(obj==null)return false;
            foreach(var m in obj.GetType().GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).Where(x=>x.Name==method && x.GetParameters().Length==args.Length))
                try { var ps=m.GetParameters();var converted=new object[args.Length];for(int i=0;i<args.Length;i++)converted[i]=ConvertValue(args[i],ps[i].ParameterType);m.Invoke(obj,converted);return true; }catch{}
            return false;
        }
        public static bool AddItem(object p,string item)
        {
            var type=FindType("ItemType");if(type==null)return false;
            try{var enumItem=Enum.Parse(type,item,true);return TryInvoke(p,"AddItem",enumItem) || TryInvoke(Get(p,"Inventory"),"AddItem",enumItem);}catch{return false;}
        }
        public static bool AddCandy(object p,string candy)
        {
            var hub=Get(p,"ReferenceHub");var inv=Get(hub,"inventory");var inventory=Get(inv,"UserInventory");var items=Get(inventory,"Items") as IDictionary;
            if(items==null)return false;
            foreach(var item in items.Values)
            {
                if(item==null || item.GetType().Name!="Scp330Bag")continue;
                var enumtype=FindType("CandyKindID");
                if(enumtype==null)return false;
                try{var v=Enum.Parse(enumtype,candy,true);if(TryInvoke(item,"ServerAddCandy",v)||TryInvoke(item,"AddCandy",v))return true;}catch{}
            }
            return false;
        }
        public static bool SetRole(object p,string role)
        {
            var roleType=FindType("RoleTypeId");if(roleType==null)return false;
            try{var type=Enum.Parse(roleType,role,true);return TryInvoke(Get(p,"Role"),"Set",type)||TryInvoke(p,"SetRole",type);}catch{return false;}
        }
        public static void ClearInventory(object p){TryInvoke(p,"ClearInventory");}
        public static void Cassie(string text)
        {
            var t=FindType("Cassie");if(t==null)return;
            foreach(var m in t.GetMethods(BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic).Where(x=>x.Name=="Message" || x.Name=="SendMessage"))
            {
                try{var a=m.GetParameters().Select((x,i)=>i==0?(object)text:x.HasDefaultValue?x.DefaultValue:x.ParameterType.IsValueType?Activator.CreateInstance(x.ParameterType):null).ToArray();m.Invoke(null,a);return;}catch{}
            }
        }
        public static void NukeFlip()
        {
            var t=FindType("AlphaWarheadController");var instance=t?.GetProperty("Singleton",BindingFlags.Public|BindingFlags.Static)?.GetValue(null);
            if(instance==null)return;
            var active=Get(instance,"InProgress");if(active is bool b){if(b)TryInvoke(instance,"CancelDetonation");else TryInvoke(instance,"StartDetonation");}
        }
        public static void Intercom(object p,float duration)
        {
            // Sem garantia de controle do intercom nativo nesta build: registra tentativa conservadora.
            TryInvoke(p,"SetIntercom",true);
            MEC.Timing.CallDelayed(duration,()=>TryInvoke(p,"SetIntercom",false));
        }
    }
}