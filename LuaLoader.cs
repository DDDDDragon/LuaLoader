using System.Data;
using Terraria.ModLoader;
using NLua;
using Terraria;
using Terraria.DataStructures;
using System.Reflection;
using System;
using Microsoft.Xna.Framework;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection.Emit;
using Microsoft.CodeAnalysis.Differencing;
using static XPT.Core.Audio.MP3Sharp.Decoding.Decoder;

namespace LuaLoader
{
    public class LuaLoader : Mod
    {
        public static Lua state;
        public static List<Assembly> Assemblies = new List<Assembly>();
        public static ItemLuaLoader itemLoader = new ItemLuaLoader();
        public static TextureLoader textureLoader = new TextureLoader();
        public static List<Type> ItemTypes = new List<Type>();
        public override void Load()
        {
            Assemblies.Add(typeof(Vector2).Assembly);
            Assemblies.Add(typeof(ModItem).Assembly);
            
            if(!File.Exists("Libraries/Native/Windows/lua54.dll"))
            {
                var file = File.Create("Libraries/Native/Windows/lua54.dll");
                var dll = GetFileBytes("lua54.dll");
                file.Write(dll, 0, dll.Length);
                file.Close();
            }
            state = new Lua();
            foreach(var i in GetType().GetMethods(BindingFlags.Static | BindingFlags.Public))
            {
                state.RegisterFunction(i.Name, null, i);
            }
            var itemLua = GetFileBytes("lua/Item.lua");
            state.DoString(Encoding.UTF8.GetString(itemLua));
            itemLoader.init();
            textureLoader.init();
            //var bytes = textureLoader.GetAllTexBytes();
            AssemblyName an = new AssemblyName("LuaLoad");
            AssemblyBuilder ab = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder mb = ab.DefineDynamicModule(an.Name);

            foreach(var item in itemLoader.items)
            {
                TypeBuilder builder = mb.DefineType(item.name, TypeAttributes.Public, typeof(LuaLoaderItem));
                var luaItem = Activator.CreateInstance(builder.CreateType()) as LuaLoaderItem ;
                var entity = typeof(ModItem).GetProperties(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(t => t.Name == "Entity");
                entity.GetSetMethod(true).Invoke(luaItem, new object[] { new Item() });
                luaItem.Item.DamageType = DamageClass.Melee;
                state["item"] = luaItem.Item;
                state.DoString($"SetDefault_{item.name}()");
                AddContent(Activator.CreateInstance(builder.CreateType()) as LuaLoaderItem);
                ItemTypes.Add(builder.CreateType());
                var ins = typeof(ContentInstance<>).MakeGenericType(builder.CreateType()).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                luaItem.Item.type = (ins.GetValue(null) as LuaLoaderItem).Item.type;
                ins.GetSetMethod(true).Invoke(null, new object[] { luaItem });
            }

            state.LoadCLRPackage();
            state.State.Encoding = Encoding.UTF8;
            
            //state.RegisterFunction("Create", null, GetType().GetMethod("Create"));
           
            state.RegisterFunction("NewText", null, typeof(Main).GetMethod("NewText", new Type[] {typeof(object), typeof(Color)}));
            //state.RegisterFunction("GetEnum", null, GetType().GetMethod("getEnum"));
            state.RegisterFunction("NewProj", null, typeof(Projectile).GetMethod("NewProjectile", new Type[] { typeof(IEntitySource), typeof(Vector2), typeof(Vector2), typeof(int),
            typeof(int), typeof(float), typeof(int), typeof(float), typeof(float), typeof(float)}));
            base.Load();
        }
        public override void Unload()
        {
            state.Dispose();
            base.Unload();
        }
        public static object Create(string type, params object[] paras)
        {
            var obj = new object();
            foreach (var i in Assemblies)
            {
                if (i.GetTypes().Where(t => t.Name == type).ToList().Count > 0)
                {
                    obj = Activator.CreateInstance(i.GetTypes().Where(t => t.Name == type).ToList()[0], paras);
                }
            }
            return obj;
        }
        public static Type getType(object obj)
        {
            return obj.GetType();
        }
        public static Enum getEnum(string name, Enum obj)
        {
            return Enum.Parse(obj.GetType(), name) as Enum;
        }
    }
    public class ILoader
    {
        public virtual string LoaderPath => "";
        public virtual void init()
        {
            if (!Directory.Exists(LoaderPath)) Directory.CreateDirectory(LoaderPath);
        }
    }
}