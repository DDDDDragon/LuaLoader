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
	public Dictionary<string, string> Lua;
        public void init()
        {
            if (!Directory.Exists(ItemsPath)) Directory.CreateDirectory(ItemsPath);
		    lua = new Dictionary<string, string>();
            var db = new LiteDatabase($"{ItemsPath}\\Items.ldb");
            var Items = db.GetCollection<LuaItem>();
	        foreach(var item in Items)
	        {
		        if(item.name != "")
		        {
		            if(File.Exists($"{ItemsPath}\\{item.name}.lua"))
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
    }

}

