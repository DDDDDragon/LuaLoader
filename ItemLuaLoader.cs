using LiteDB;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

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
            foreach (var item in Items)
            {
                if (item.name != "")
                {
                    if (File.Exists($"{LoaderPath}\\{item.name}.lua"))
                    {
                        var lua = File.ReadAllText($"{LoaderPath}\\{item.name}.lua");
                        item.lua = lua;
                        items.Add(item);
                        LuaLoader.state.DoString(lua);
                    }

                }
            }
            var litem = new LuaItem("testItem", "测试", "");
            litem.overrideMethods.Add("UpdateInventory");
            items.Add(litem);
            LuaLoader.state.DoString(File.ReadAllText($"{LoaderPath}\\testItem.lua"));
        }
        public void reloadItemsLua()
        {
            var db = new LiteDatabase($"{LoaderPath}\\Item.tdb");
            var Items = db.GetCollection<LuaItem>().FindAll();
            items.Clear();
            foreach (var item in Items)
            {
                if (item.name != "")
                {
                    if (File.Exists($"{LoaderPath}\\{item.name}.lua"))
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
        public List<string> overrideMethods;
        public LuaItem() { }
        public LuaItem(string name, string discription, string lua)
        {
            this.name = name;
            this.discription = discription;
            this.lua = lua;
            overrideMethods = new List<string>();
        }
    }
    [Autoload(false)]
    public class LuaLoaderItem : ModItem
    {
        public bool init = false;
        public string texturePath = ModLoader.ModPath + "LuaLoader\\Textures\\";
        public override string Texture
        {
            get
            {
                //if (File.Exists(texturePath)) return texturePath + Name;
                return "LuaLoader/Textures/NULL";
            }
        }
        public override void UpdateInventory(Player player)
        {
            //Main.NewText(Tooltip.GetDefault());
            if(!init) 
            {
                LuaLoader.state["item"] = Item;
                LuaLoader.state.DoString($"SetDefault_{GetType().Name}()");
                //Item.DamageType = DamageClass.Melee;
                init = true;
            }
            var ins = typeof(ContentInstance<>).MakeGenericType(GetType())
                .GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null) as LuaLoaderItem;
            inst = ins.Item;
            base.UpdateInventory(player);
        }
        public Item inst;
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (inst != null)
            {
                Item item = inst;
                string text = $"damage：{item.damage}\n" +
                    $"width：{item.width}\n" +
                    $"height：{item.height}\n" +
                    $"useTime：{item.useTime}\n" +
                    $"useAnimation：{item.useAnimation}\n" +
                    $"useStyle：{item.useStyle}\n" +
                    $"knockBack：{item.knockBack}\n" +
                    $"value：{item.value}\n" +
                    $"rare：{item.rare}\n" +
                    $"type：{item.type}\n" +
                    $"LuaLoaderItemtype：{Type}\n" +
                    $"damageType：{item.DamageType} \n" +
                    $"autoReuse：{item.autoReuse}\n";
                spriteBatch.DrawString(FontAssets.MouseText.Value, text, Main.LocalPlayer.Center + new Vector2(200, -300) - Main.screenPosition, Color.Black);
            }
            return true;
        }
    }
}

