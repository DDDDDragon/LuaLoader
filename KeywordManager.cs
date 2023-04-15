using Humanizer;
using KeraLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LuaLoader
{
    public class KeywordManager
    {
    }
    public class Keyword
    {
        public virtual string Value => "";
        public Regex Regex;
        public Regex Replace;
        public Keyword() { }
        public Keyword(Regex regex, Regex replace)
        {
            Regex = regex;
            Replace = replace;
        }
        public virtual node KeywordState(ref int cur, ref List<token> tokens, ref List<error> errors)
        {
            return new node();
        }
        /*
        public delegate void MatchStr(ref string lua, Match match);
        public event MatchStr OnMatch;
        public virtual void Match(ref string lua, Match match, LuaObj obj)
        {
            OnMatch(ref lua, match);
        }
        */
    }
    public class Nobase : Keyword
    {
        public override string Value => "nobase";
    }
    public class Override : Keyword
    {
        public override string Value => "override";
        public override node KeywordState(ref int cur, ref List<token> tokens, ref List<error> errors)
        {
            var len = LuaParser.GetPreLen(cur, tokens);
            var funcTabs = LuaParser.GetPreTabs(cur, tokens);
            cur++;
            cur = LuaParser.ignoreSpace(cur, tokens);
            var keys = new List<string>();
            if (tokens[cur].Type == "nobase")
            {
                keys.Add("nobase");
                cur++;
                cur = LuaParser.ignoreSpace(cur, tokens);
            }
            var func = LuaParser.FuncState(ref cur, ref tokens, ref errors, funcTabs);
            func.StartPos = len + 1;
            if (keys.Count == 0) func.KeyWords.Add(Value);
            else func.KeyWords = keys;
            return func;
        }
    }
}
