using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using System;
using System.IO;
using System.Text;
using System.Reflection;
using LuaLoader.UI.UIElement;

namespace LuaLoader
{
	public class a : ModItem
	{
		public override void SetStaticDefaults() 
		{
			// DisplayName.SetDefault("a"); // By default, capitalization in classnames will add spaces to the display name. You can customize the display name here by uncommenting this line.
			//Tooltip.SetDefault("This is a basic modded sword.");
		}

		public override void SetDefaults() 
		{
			Item.damage = 50;
			Item.DamageType = DamageClass.Melee;
			Item.width = 40;
			Item.height = 40;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = 1;
			Item.knockBack = 6;
			Item.value = 10000;
			Item.rare = 2;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
		}

		public override void AddRecipes() 
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.DirtBlock, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
		public override bool CanUseItem(Player player)
		{

			LuaLoader.LuaUISystem.Elements["CodeBox"].Show();
            if (File.Exists("C:\\Users\\ASUS\\Documents\\My Games\\Terraria\\tModLoader\\ModSources\\lua.lua"))
			{
				string str = File.ReadAllText("C:\\Users\\ASUS\\Documents\\My Games\\Terraria\\tModLoader\\ModSources\\lua.lua", Encoding.UTF8);
				if (str != "")
				{
					LuaLoader.state["player"] = player;
					LuaLoader.state["ProjID"] = ProjectileID.IceBolt;
					LuaLoader.state["Mouse"] = Main.MouseWorld - player.Center;
					LuaLoader.state.DoString(str);
				}
				else Main.NewText("无lua脚本");
			}
			
			return base.CanUseItem(player);
		}
	}
}