using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace LuaLoader
{
    public class TextureLoader : ILoader
    {
        DirectoryInfo dir;
        FileInfo[] fileInfos;
        public override string LoaderPath => ModLoader.ModPath + "\\LuaLoader\\Textures";

        public override void init()
        {
            base.init();
        }
        public Dictionary<string, byte[]> GetAllTexBytes()
        {
            Dictionary<string, byte[]> bytes = new Dictionary<string, byte[]>();
            dir = new DirectoryInfo(LoaderPath);
            fileInfos = dir.GetFiles("*.png");
            foreach(var file in fileInfos)
            {
                bytes.Add(file.FullName, getBytes(file.FullName));
            }
            return bytes;
        }
        public Texture2D GetTexture(string PicPath)
        {
            if (!File.Exists(PicPath))
            {
                return ModContent.Request<Texture2D>("LuaLoader/Textures/NULL").Value;
            }
            else
            {
                FileStream fs = new FileStream(PicPath, FileMode.Open, FileAccess.Read);
                int byteLength = (int)fs.Length;
                byte[] imgBytes = new byte[byteLength];
                fs.Read(imgBytes, 0, byteLength);
                fs.Close();
                fs.Dispose();
                Texture2D texture = Texture2D.FromStream(Main.instance.GraphicsDevice, new MemoryStream(imgBytes));
                return texture;
            }
        }
        public byte[] getBytes(string path)
        {
            if (!File.Exists(path))
            {
                byte[] nul = { };
                return nul;
            }
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            int byteLength = (int)fs.Length;
            byte[] imgBytes = new byte[byteLength];
            fs.Read(imgBytes, 0, byteLength);
            fs.Close();
            fs.Dispose();
            return imgBytes;
        }
        public void SetTexture(int type, Texture2D tex)
        {
            var t2d = typeof(Asset<Texture2D>).GetField("ownValue", BindingFlags.NonPublic | BindingFlags.Instance);
            t2d.SetValue(TextureAssets.Item[type], tex);
        }
    }
}
