using System;
using System.IO;
using LiteDB;
using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace LuaLoader
{
    public class ItemLuaLoader
    {
        private static string ItemsPath = ModLoader.ModPath + "\\LuaLoader\\Items";
	    public List<LuaItem> items;
        public void registerLuaFunc()
        {
            LuaLoader.state.RegisterFunction();
        }
        public void init()
        {
            if (!Directory.Exists(ItemsPath)) Directory.CreateDirectory(ItemsPath);
		    items = new List<LuaItem>();
            var db = new LiteDatabase($"{ItemsPath}\\Items.ldb");
            var Items = db.GetCollection<LuaItem>();
	        foreach(var item in Items)
	        {
		        if(item.name != "")
		        {
		            if(File.Exists($"{ItemsPath}\\{item.name}.lua"))
				    {
                        var lua = File.ReadAllText($"{ItemsPath}\\{item.name}.lua");
						item.lua = lua;
                        items.Add(item);
                        LuaLoader.state.DoString(lua);
					}

		        }
	        }
        }
        public void reloadItemsLua()
        {
            var db = new LiteDataBase($"{ItemPath}\\Item.ldb");
            var Items = db.GetCollection<LuaItem>();
            foreach(var item in Items)
            {
                if(item.name != "")
                {
                    if(File.Exists(|$"{ItemsPath}\\{item.name}.lua"))
                    {
                        
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
    }

}

