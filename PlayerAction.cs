using System.IO;
using System.Text;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria;
using System.Collections.Generic;
using NLua;
using Microsoft.Xna.Framework.Input;
using System;

namespace LuaLoader
{
    public class PlayerAction : ModPlayer
    {
        public List<Action> actions;
        public List<Combo> combos;
        public override void Initialize()
        {
            //LuaLoader.state.RegisterFunction("IsKeyUp", null, this.GetType().GetMethod("IsKeyUp"));
            //LuaLoader.state.RegisterFunction("IsKeyDown", null, this.GetType().GetMethod("IsKeyDown"));
            //actions = new List<Action>();
            //actions.Add(new Action("HeadGo"));
            //foreach (var i in actions) i.init();
            base.Initialize();
        }
        public static bool IsKeyUp(string key)
        {
            return Keyboard.GetState().IsKeyUp((Keys)Enum.Parse(typeof(Keys), key));
        }
        public static bool IsKeyDown(string key)
        {
            return Keyboard.GetState().IsKeyDown((Keys)Enum.Parse(typeof(Keys), key));
        }
        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            //LuaLoader.state["drawInfo"] = drawInfo;
            //actions[0].DoAction(1);
            base.ModifyDrawInfo(ref drawInfo);
        }
    }
    public class Action
    {
        public string name;
        public int cost;
        public int power;
        public int frame;
        public Keys[] key;
        public int maxFrame;
        public string lua = "";
        public void init()
        {
            if (File.Exists("C:\\Users\\ASUS\\Documents\\My Games\\Terraria\\tModLoader\\ModSources\\" + name + ".lua"))
            {
                lua = File.ReadAllText("C:\\Users\\ASUS\\Documents\\My Games\\Terraria\\tModLoader\\ModSources\\" + name + ".lua", Encoding.UTF8);
                LuaLoader.state["this"] = this;
                if (lua == "") Main.NewText("无lua脚本");
                else
                {
                    LuaLoader.state.DoString(lua);
                    (LuaLoader.state["init_" + name] as LuaFunction).Call();
                }
            }
        }
        public Action(string name)
        {
            this.name = name;
        }
        public void checkInput()
        {
        }
        public void DoAction(int frame)
        {
            (LuaLoader.state[name + ".DoAction"] as LuaFunction).Call(frame);
        }
    }
    public class Combo
    {
        public string name;
        public Keys[] keySequence;
        public Action action;
        public bool canChangeState;
        public Combo(string name, params Action[] paras)
        {
            this.name = name;
            this.keySequence = keySequence;
            this.action = action;
        }
    }
}
