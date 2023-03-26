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

namespace LuaLoader
{
    public class LuaLoader : Mod
    {
        public static Lua state = new Lua();
        public static List<Assembly> Assemblies = new List<Assembly>();
        public static ItemLuaLoader itemLoader = new ItemLuaLoader();
        public static TextureLoader textureLoader = new TextureLoader();
        public override void Load()
        {
            Assemblies.Add(typeof(Vector2).Assembly);

            itemLoader.init();
            textureLoader.init();
            //var bytes = textureLoader.GetAllTexBytes();

            foreach(var item in itemLoader.items)
            {
                var luaItem = new LuaLoaderItem();
                var entity = typeof(ModItem).GetProperties(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(t => t.Name == "Entity");
                entity.GetSetMethod(true).Invoke(luaItem, new object[] { new Item() });
                var disName = typeof(ModItem).GetProperty("DisplayName", BindingFlags.Public | BindingFlags.Instance);
                disName.GetSetMethod(true).Invoke(luaItem, new object[] { LocalizationLoader.CreateTranslation(this, "ItemName." + item.name) });
                luaItem.DisplayName.SetDefault(item.name);
                luaItem.Item.DamageType = DamageClass.Melee;
                state["item"] = luaItem.Item;
                state.DoString($"SetDefault_{item.name}()");
                AddContent(luaItem);
            }

            state.LoadCLRPackage();
            state.State.Encoding = Encoding.UTF8;
            state.RegisterFunction("Create", null, this.GetType().GetMethod("Create"));
            state.RegisterFunction("NewText", null, typeof(Main).GetMethod("NewText", new Type[] {typeof(object), typeof(Color)}));
            state.RegisterFunction("NewProj", null, typeof(Projectile).GetMethod("NewProjectile", new Type[] { typeof(IEntitySource), typeof(Vector2), typeof(Vector2), typeof(int),
            typeof(int), typeof(float), typeof(int), typeof(float), typeof(float)}));
            base.Load();
        }
        public override void Unload()
        {
            state.Dispose();
            base.Unload();
        }
        public static void Create(string type, string name, params object[] paras)
        {
            var obj = new object();
            foreach(var i in Assemblies)
            {
                if(i.GetTypes().Where(t => t.Name == type).ToList().Count > 0)
                {
                    obj = Activator.CreateInstance(i.GetTypes().Where(t => t.Name == type).ToList()[0], paras);
                }
            }
            state[name] = obj;
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