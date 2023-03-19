using System;
using System.IO;
using LiteDB;
using Terraria;
using Terraria.ModLoader;

namespace LuaLoader
{
    public class ItemLuaLoader
    {
        private static string ItemsPath = ModLoader.ModPath + "\\LuaLoader\\Items";
        public void init()
        {
            if (!Directory.Exists(ItemsPath)) Directory.CreateDirectory(ItemsPath);
            var db = new LiteDatabase($"{ItemsPath}\\Items.ldb");
            var Items = db.GetCollection("Item");
        }
    }
}
