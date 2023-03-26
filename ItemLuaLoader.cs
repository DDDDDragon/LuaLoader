using System;
using System.IO;
using LiteDB;
using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.GameContent;
using ReLogic.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LuaLoader
{
    public class ItemLuaLoader : ILoader
    {
        public override string LoaderPath => ModLoader.ModPath + "\\LuaLoader\\Items";
	    public List<LuaItem> items;
        public void registerLuaFunc()
        {
            //LuaLoader.state.RegisterFunction();
        }
        public override void init()
        {
            base.init();
            items = new List<LuaItem>();
            var db = new LiteDatabase($"{LoaderPath}\\Items.tdb");
            var Items = db.GetCollection<LuaItem>().FindAll();
	        foreach(var item in Items)
	        {
		        if(item.name != "")
		        {
		            if(File.Exists($"{LoaderPath}\\{item.name}.lua"))
				    {
                        var lua = File.ReadAllText($"{LoaderPath}\\{item.name}.lua");
						item.lua = lua;
                        items.Add(item);
                        LuaLoader.state.DoString(lua);
					}

		        }
	        }
            items.Add(new LuaItem("testItem", "测试", ""));
            LuaLoader.state.DoString(File.ReadAllText($"{LoaderPath}\\testItem.lua"));
        }
        public void reloadItemsLua()
        {
            var db = new LiteDatabase($"{LoaderPath}\\Item.tdb");
            var Items = db.GetCollection<LuaItem>().FindAll();
            items.Clear();
            foreach(var item in Items)
            {
                if(item.name != "")
                {
                    if(File.Exists($"{LoaderPath}\\{item.name}.lua"))
                    {
                        var lua = File.ReadAllText($"{LoaderPath}\\{item.name}.lua");
                        item.lua = lua;
                        items.Add(item);
                        LuaLoader.state.DoString(lua);
                    }
                }
            } 
        }
    }
    public class LuaItem
    {
	    public string name;
	    public string discription;
        public string lua;
        public LuaItem() { }
        public LuaItem(string name, string discription, string lua)
        {
            this.name = name;
            this.discription = discription;
            this.lua = lua;
        }
    }
    [Autoload(false)]
    public class LuaLoaderItem : ModItem
    {
        public string texturePath = ModLoader.ModPath + "LuaLoader\\Textures\\";
        public override string Texture 
        {
            get 
            {
                //if (File.Exists(texturePath)) return texturePath + Name;
                return "LuaLoader/Textures/NULL";
            }
        }
        public override void AutoStaticDefaults()
        {
            //LuaLoader.textureLoader.SetTexture(Type, LuaLoader.textureLoader.GetTexture(Texture));
            base.AutoStaticDefaults();
        }
    }
}

