using System.Data;
using Terraria.ModLoader;
using NLua;
using Terraria;
using Terraria.DataStructures;
using System.Reflection;
using System;
using Microsoft.Xna.Framework;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using LuaLoader.UI;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection.Emit;
using LuaLoader.Utils.FontInfos;
using LuaLoader.Utils;
using FontStashSharp;
using Steamworks;
using MonoMod.Cil;

namespace LuaLoader
{
    public class LuaLoader : Mod
    {
        public static Lua state;
        public static List<Assembly> Assemblies = new List<Assembly>();
        public static ItemLuaLoader itemLoader = new ItemLuaLoader();
        public static TextureLoader textureLoader = new TextureLoader();
        public static List<Type> ItemTypes = new List<Type>();
        public static LuaLoader Instance { get => ModContent.GetInstance<LuaLoader>(); }
        public static LuaUISystem LuaUISystem
        {
            get
            {
                if (Instance.uiSystem == null)
                    Instance.uiSystem = new LuaUISystem();
                return Instance.uiSystem;
            }
        }
        public const float DEFAULT_FONT_SIZE = 40f;
        public static FontSystem DefaultFontSystem => FontManager["Fonts/SourceHanSansHWSC-VF.ttf"];
        public static DynamicSpriteFont DefaultFont = DefaultFontSystem.GetFont(DEFAULT_FONT_SIZE);

        private LuaUISystem uiSystem;

        public static DynamicSpriteFontInfoManager DynamicSpriteFontInfoManager
        {
            get
            {
                if (Instance.infoManager == null)
                    Instance.infoManager = new DynamicSpriteFontInfoManager();
                return Instance.infoManager;
            }
        }

        private DynamicSpriteFontInfoManager infoManager;
        internal static FontManager FontManager
        {
            get
            {
                if (Instance._fontManager == null)
                    Instance._fontManager = new FontManager();
                return Instance._fontManager;
            }
        }

        private FontManager _fontManager;
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
                builder = LoadOverrideMethod(item, builder);

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
        public TypeBuilder LoadOverrideMethod(LuaItem item, TypeBuilder luaItem)
        {
            foreach(var method in item.overrideMethods)
            {
                var target = typeof(LuaLoaderItem).GetMethod(method);
                var m = luaItem.DefineMethod(method, target.Attributes, CallingConventions.HasThis, target.ReturnType, ParaToType(target.GetParameters()));
                var il = m.GetILGenerator();
                var f = GetType().GetField("state", BindingFlags.Static | BindingFlags.Public);
                il.Emit(OpCodes.Ldsfld, f);
                il.Emit(OpCodes.Ldstr, $"{method}_{item.name}()");
                il.Emit(OpCodes.Call, typeof(Lua).GetMethod("DoString", BindingFlags.Public | BindingFlags.Instance, new Type[] { typeof(string), typeof(string) }));
                var i = 0;
                while (i < target.GetParameters().Length + 1)
                { 
                    il.Emit(OpCodes.Ldarg, i);
                    i++;
                } 
                il.Emit(OpCodes.Call, target);
                il.Emit(OpCodes.Ret);
                luaItem.DefineMethodOverride(m, target);
            }
            return luaItem;
        }
        public Type[] ParaToType(ParameterInfo[] paras)
        {
            Type[] types = new Type[paras.Length];
            var list = paras.ToList();
            foreach (var p in list)
            {
                types[list.IndexOf(p)] = p.ParameterType;
            }
            return types;
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
    public interface ILua
    {

    }
}
