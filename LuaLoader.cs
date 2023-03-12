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

namespace LuaLoader
{
    public class LuaLoader : Mod
    {
        public static Lua state = new Lua();
        public static List<Assembly> Assemblies = new List<Assembly>();
        public override void Load()
        {
            Assemblies.Add(typeof(Vector2).Assembly);

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
}