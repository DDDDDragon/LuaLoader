using LuaLoader.UI.UIElement;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Terraria;
using Terraria.IO;

namespace LuaLoader
{
    public class LuaParser
    {
        public static List<string> KeyWords = new List<string>() {
            "if", "and", "else", "elseif", "while", "for", "in", "local",
            "function", "break", "return", "false", "nil", "true",
            "goto", "not", "or", "repeat", "then", "until", "while", "end"
        };
        public static Dictionary<string, Keyword> AdditionalKeywords = new Dictionary<string, Keyword>();
        public static List<string> Separators = new List<string>() {
            ",", ";", "(", ")", ":", "."
        };
        public static List<string> Operators = new List<string>() {
            "+", "-", "*", "/", "=", "<", ">", "~", ">=", "<=", "~=", "==",
            "//", "^", "%", "&", "|", ">>", "<<", ".."
        };
        public static List<token> lex(string filepath, bool fileCreate = true)
        {
            var file = "";
            if (File.Exists(filepath)) file = File.ReadAllText(filepath);
            else if (fileCreate) File.Create(filepath);
            return lex(file);
        }
        public static void LoadKeywords()
        {
            var Keywords = from c in typeof(LuaParser).Assembly.GetTypes() 
                           where !c.IsAbstract && c.IsSubclassOf(typeof(Keyword)) 
                           select c;
            Keyword keyword;
            foreach (var c in Keywords)
            {
                keyword = (Keyword)Activator.CreateInstance(c);
                if (keyword.Value != "")
                {
                    KeyWords.Add(keyword.Value);
                    AdditionalKeywords.Add(keyword.Value, keyword);
                }
            }
        }
        public static List<token> lex(string lua)
        {
            var cur = 0;
            var tokens = new List<token>();
            var line = 1;
            var startPos = 1;
            while (cur < lua.Length)
            {
                if (lua[cur].ToString() == "-")
                {
                    if (lua[++cur].ToString() == "-")
                        while (cur < lua.Length && lua[cur].ToString() != "\n") cur++;
                }
                else if (new Regex("[a-zA-Z_]").IsMatch((lua[cur].ToString())))
                {
                    var word = lua[cur++].ToString();
                    while (cur < lua.Length && new Regex("[a-zA-Z0-9_]").IsMatch((lua[cur].ToString()))) word += lua[cur++].ToString();
                    if (KeyWords.Contains(word))
                    {
                        tokens.Add(new token(word, word, line, startPos));
                        startPos += word.Length;
                    }
                    else if (new List<string>() { "true", "false" }.Contains(word))
                    {
                        tokens.Add(new token("Boolean", word, line, startPos));
                        startPos += word.Length;
                    }
                    else
                    {
                        tokens.Add(new token("Identifier", word, line, startPos));
                        startPos += word.Length;
                    }
                }
                else if (Separators.Contains(lua[cur].ToString()))
                {
                    tokens.Add(new token("Separator", lua[cur++].ToString(), line, startPos));
                    startPos++;
                }
                else if (Operators.Contains(lua[cur].ToString()))
                {
                    var Operator = lua[cur++].ToString();
                    if (new List<string>() { ">", "<", "~", "=" }.Contains(lua[cur].ToString()))
                    {
                        if (lua[cur].ToString() == "=") Operator += lua[cur++].ToString();
                    }
                    tokens.Add(new token("Operator", Operator, line, startPos));
                    startPos += Operator.Length;
                }
                else if (new Regex("[0-9] ").IsMatch((lua[cur].ToString())))
                {
                    var val = lua[cur++].ToString();
                    while (cur < lua.Length && new Regex("[0-9] ").IsMatch((lua[cur].ToString()))) val += lua[cur++].ToString();
                    tokens.Add(new token("Number", val, line, startPos));
                    startPos += val.Length;
                }
                else if (new List<string>() { "\n", "\t", "\r" }.Contains(lua[cur].ToString()))
                {
                    tokens.Add(new token("WhiteSpace", lua[cur].ToString(), line, startPos));
                    if (lua[cur].ToString() == "\n")
                    {
                        line++;
                        startPos = 1;
                    }
                    cur++;
                }
                else if (lua[cur].ToString() == " ")
                {
                    cur++;
                    var len = 1;
                    while (cur < lua.Length && lua[cur].ToString() == " ")
                    {
                        len++;
                        cur++;
                    }

                    tokens.Add(new token("WhiteSpace", " *" + len, line, startPos));
                    startPos += len;
                }
                else if (new List<string>() { "\"", "'" }.Contains(lua[cur].ToString()))
                {
                    var start = lua[cur].ToString();
                    cur++;
                    startPos++;
                    var str = "";
                    while (lua[cur].ToString() != start && cur < lua.Length)
                    {
                        str += lua[cur].ToString();
                        cur++;
                        startPos++;
                    }
                    if (lua[cur].ToString() == start)
                    {
                        tokens.Add(new token("String", $"\"{str}\"", line, startPos));
                    }
                    else throw new Exception("字符串错误");
                    cur++;
                }
                else
                {
                    //("包含非法字符：" + lua[cur].ToString() + " " + cur)
                    cur++;
                }
            }
            tokens.Add(new token("EOF", lua.Length.ToString(), line, startPos));
            return tokens;
        }
        public static (string str, node program) parser(string filePath, string type = "json", int tab = 4, bool fileCreate = true)
        {
            var lua = "";
            if (File.Exists(filePath)) lua = File.ReadAllText(filePath);
            else if (fileCreate) File.Create(filePath);
            return parser(lua, type, tab);
        }
        public static (string str, node program) parser(string lua, string type = "json", int tab = 4)
        {
            var tokens = lex(lua);
            var errors = new List<error>();
            var cur = 0;
            var rootNode = new node("Program", 0, int.Parse(tokens[tokens.Count - 1].Value));
            (string str, node program) ret = ("", new node());
            while (cur < tokens.Count)
            {
                switch (tokens[cur].Type)
                {
                    case "var":
                        var result = VarState(ref cur, ref tokens, ref errors);
                        rootNode.Body.Value.Add(result);
                        break;
                    case "function":
                        var func = FuncState(ref cur, ref tokens, ref errors);
                        rootNode.Body.Value.Add(func);
                        break;
                    case "EOF":
                        if (type == "json") ret.str = JsonConvert.SerializeObject(rootNode);
                        else if (type == "node") ret.program = rootNode;
                        break;
                    default:
                        if (AdditionalKeywords.ContainsKey(tokens[cur].Type))
                        {
                            var node = AdditionalKeywords[tokens[cur].Type].KeywordState(ref cur, ref tokens, ref errors);
                            rootNode.Body.Value.Add(node);
                        }
                        else errors.Add(new error(tokens[cur].Line, tokens[cur].StartPos, "未实现的关键字"));
                        break;
                }
            }
            return ret;
        }
        public static string coder((string str, node program) AST)
        {
            return "";
        }
        public static int GetPreLen(int cur, List<token> tokens, int tab = 4)
        {
            var len = 0;
            for(var i = 0; i < cur; i++)
            {
                if (new Regex(" \\*[0-9]").IsMatch(tokens[i].Value)) len += int.Parse(tokens[i].Value.Split("*")[1]);
                else if (tokens[i].Value == "\t") len += tab;
                else len += tokens[i].Value.Length;
            }
            return len;
        }
        public static int GetPreTabs(int cur, List<token> tokens, int tab = 4)
        {
            if (cur == 0) return 0;
            var tabs = 0;
            while (tokens[cur].Value != "\n")
            {
                //Console.WriteLine(value);
                cur--;
            }
            cur++;
            while (tokens[cur].Value == "\t" || tokens[cur].Value == "*" + tabs.ToString())
            {
                tabs++;
                cur++;
            }
            return tabs;
        }
        public static VarNode VarState(ref int cur, ref List<token> tokens, ref List<error> errors)
        {
            var len = GetPreLen(cur, tokens);
            var node = new VarNode(len + 1, 0, tokens[cur].Value);
            var varDeclarator = new Declarator("VariableDeclarator", new IdentifierNode(tokens[cur].Value));
            var init = new InitNode("", "");
            cur++;
            cur = ignoreSpace(cur, tokens);
            if (tokens[cur].Type == "Identifier")
            {
                varDeclarator.ID = new IdentifierNode(tokens[cur].Value);
                cur++;
            }
            else errors.Add(new error(tokens[cur].Line, tokens[cur].StartPos, "声明变量时未定义变量名"));
            cur = ignoreSpace(cur, tokens);
            if (tokens[cur].Value == "=")
            {
                cur++;
                cur = ignoreSpace(cur, tokens);
                if (new List<string>() { "Boolean", "Number", "String", "Identifier" }.Contains(tokens[cur].Type)) init = new InitNode(tokens[cur].Type, tokens[cur].Value);
            }
            varDeclarator.Init = init;
            node.Declarations.Add(varDeclarator);
            node.EndPos = GetPreLen(cur, tokens) + tokens[cur].Value.Length;
            cur++;
            return node;
        }
        public static FuncNode FuncState(ref int cur, ref List<token> tokens, ref List<error> errors, int funcTabs = 0)
        {
            var len = GetPreLen(cur, tokens);
            if(funcTabs == 0) funcTabs = GetPreTabs(cur, tokens);
            var node = new FuncNode("FunctionDeclaration", len + 1, 0);
            cur++;
            cur = ignoreSpace(cur, tokens);
            var id = new IdentifierNode();
            if (tokens[cur].Type == "Identifier") id = new IdentifierNode(tokens[cur].Value);
            else errors.Add(new error(tokens[cur].Line, tokens[cur].StartPos, "声明函数时未定义函数名"));
            cur++;
            cur = ignoreSpace(cur, tokens);
            if (tokens[cur].Value == "(")
            {
                cur++;
                while (tokens[cur].Value != ")")
                {
                    node.Parameters.Add(new IdentifierNode(tokens[cur].Value));
                    cur++;
                    cur = ignoreSpace(cur, tokens);
                    if (tokens[cur].Value == ",")
                    {
                        cur++;
                        cur = ignoreSpace(cur, tokens);
                        if (tokens[cur].Type != "Identifier") errors.Add(new error(tokens[cur].Line, tokens[cur].StartPos, "函数的参数缺失"));
                        else continue;
                    }
                    else if (tokens[cur].Value != ")") errors.Add(new error(tokens[cur].Line, tokens[cur].StartPos, "缺少符号 )"));
                    cur++;
                    cur = ignoreSpace(cur, tokens);
                }
            }
            else errors.Add(new error(tokens[cur].Line, tokens[cur].StartPos, "缺少符号 ("));
            if (tokens[cur++].Value != "\n") errors.Add(new error(tokens[cur].Line, tokens[cur].StartPos, "方法的实现应在方法声明的下一行"));
            var bodyStart = GetPreLen(cur, tokens);
            var tab = 0;
            if (tokens[cur].Type != "WhiteSpace") errors.Add(new error(tokens[cur].Line, tokens[cur].StartPos, "方法未实现"));
            else if (tokens[cur + 1].Type != "WhiteSpace")
            {
                if (tokens[cur].Value == "\t") tab = 4;
                else if (new Regex(" \\*[0-9]").IsMatch(tokens[cur].Value)) tab = int.Parse(tokens[cur].Value.Split("*")[1]);
                else if (tokens[cur].Value == "\n") errors.Add(new error(tokens[cur].Line, tokens[cur].StartPos, "方法未实现"));
            }
            var body = new List<node>();
            while((tokens[cur].Value == "\t" || new Regex(" \\*[0-9]").IsMatch(tokens[cur].Value) || GetPreTabs(cur, tokens, tab) > funcTabs && cur < tokens.Count))
            {
                switch(tokens[cur].Type)
                {
                    case "var":
                        var result = VarState(ref cur, ref tokens, ref errors);
                        body.Add(result);
                        break;
                    case "Identifier":
                        cur++;
                        break;
                    default:
                        cur++;
                        break;
                }
            }
            node.ID = id;
            node.Body.Value = body;
            node.Body.StartPos = bodyStart;
            node.Body.EndPos = GetPreLen(cur, tokens) + tokens[cur].Value.Length;
            return node;
        }
        public static int ignoreSpace(int cur, List<token> tokens)
        {
            if (new Regex(" \\*[0-9]").IsMatch((tokens[cur].Value))) cur++;
            return cur;
        }
    }
    public class token
    {
        public string Type;
        public string Value;
        public int Line;
        public int StartPos;
        public token(string type, string value, int line, int startPos)
        {
            Type = type;
            Value = value;
            Line = line;
            StartPos = startPos;
        }
    }
    public class node
    {
        public string Type;
        public NodeBody Body;
        public int StartPos;
        public int EndPos;
        public node() { }
        public node(string type, int startPos, int endPos)
        {
            Type = type;
            StartPos = startPos;
            EndPos = endPos;
            Body = new NodeBody();
        }
    }
    public class IdentifierNode : node
    {
        public string Name;
        public IdentifierNode() { }
        public IdentifierNode(string name)
        {
            Name = name;
            Type = "Identifier";
        }
    }
    public class Declarator : node
    {
        public IdentifierNode ID;
        public InitNode Init;
        public Declarator() {
            Init = new InitNode();
        }
        public Declarator(string type, IdentifierNode id)
        {
            ID = id;
            Type = type;
            Init = new InitNode();
        }
    }
    public class InitNode : node
    {
        public string Value;
        public InitNode() { }
        public InitNode(string type, string value)
        {
            Type = type;
            Value = value;
        }
    }
    public class VarNode : node
    {
        public List<Declarator> Declarations;
        public string Kind;
        public VarNode() {
            Declarations = new List<Declarator>();
        }
        public VarNode(int startPos, int endPos, string kind)
        {
            Type = "VariableDeclaration";
            Declarations = new List<Declarator>();
            StartPos = startPos;
            EndPos = endPos;
            Kind = kind;
        }
    }
    public class FuncNode : node
    {
        public List<IdentifierNode> Parameters;
        public IdentifierNode ID;
        public List<string> KeyWords;
        public FuncNode() {
            Parameters = new List<IdentifierNode>();
            KeyWords = new List<string>();
        }
        public FuncNode(string type, int startPos, int endPos)
        {
            Type = type;
            Parameters = new List<IdentifierNode>();
            KeyWords = new List<string>();
            StartPos = startPos;
            EndPos = endPos;
        }
    }
    public class NodeBody : node
    {
        public List<node> Value;
        public NodeBody() {
            Value = new List<node>();
        }
        public NodeBody(int startPos, int endPos)
        {
            StartPos = startPos;
            EndPos = endPos;
            Value = new List<node>();
        }
    }
    public class error
    {
        public int Line;
        public int StartPos;
        public string Value;
        public error(int line, int startPos, string value)
        {
            Line = line;
            StartPos = startPos;
            Value = value;
        }
    }
}
