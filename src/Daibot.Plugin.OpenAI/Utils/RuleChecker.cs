using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daibot.Plugin.OpenAI.Utils
{
    public class RuleChecker
    {
        public string CheckContent { get; set; }

        readonly Dictionary<string, Func<List<Exp>, Exp>> funcs = new();

        public RuleChecker(string content)
        {
            CheckContent = content;
            funcs.Add("and", AndFunction);
            funcs.Add("or", OrFunction);
            funcs.Add("not", NotFunction);
            funcs.Add("!", NotFunction);
            funcs.Add("eq", EqFunction);
            funcs.Add("=", NotFunction);
        }

        public bool Check(string rule)
        {
            Exp exp = Parse(rule);
            bool? result = ExecExp(exp).Result;
            if (result == null)
            {
                throw new Exception("表达式计算没有结果:" + rule);
            }
            return (bool)result;
        }

        static Exp Parse(string rule)
        {
            var expStack = new Stack<Exp>();
            var sb = new StringBuilder();
            bool added = false;
            for (int i = 0; i < rule.Length; i++)
            {
                char c = rule[i];
                switch (c)
                {
                    case '[':
                    case '(':
                        if (sb.Length == 0)
                        {
                            expStack.Push(new Exp() { Name = c == '[' ? "or" : "and" });
                        }
                        else
                        {
                            expStack.Push(new Exp() { Name = sb.ToString() });
                            sb.Clear();
                        }
                        break;
                    case ']':
                    case ')':
                        List<Exp> args = new();
                        if (sb.Length > 0)
                        {
                            args.Add(new Exp() { Value = sb.ToString() });
                            sb.Clear();
                        }
                        while (true)
                        {
                            var exp = expStack.Pop();
                            if (exp.Name == null || exp.Complete)
                            {
                                args.Insert(0, exp);
                            }
                            else
                            {
                                exp.Args = args;
                                exp.Complete = true;
                                expStack.Push(exp);
                                added = true;
                                break;
                            }
                        }
                        break;
                    case ',':
                        if (added)
                        {
                            added = false;
                            break;
                        }
                        expStack.Push(new Exp() { Value = sb.ToString() });
                        sb.Clear();
                        break;
                    default:
                        added = false;
                        sb.Append(c);
                        break;
                }
            }
            var result = expStack.Pop();
            if (expStack.Count > 0)
            {
                throw new Exception("规则没有正确结束");
            }
            return result;
        }

        Exp ExecExp(Exp exp)
        {
            if (exp.Name == null)
            {
                string? value = exp.Value;
                if (exp.Result != null)
                {
                    return exp;
                }
                if (value == null)
                {
                    throw new Exception("解析异常");
                }
                if (value.StartsWith('%'))
                {
                    if (int.TryParse(value[1..], out int rate))
                    {
                        int seed = (int)(DateTime.Now.Ticks % int.MaxValue);
                        return new Random(seed).Next(100) < rate ? Exp.TrueExp : Exp.FalseExp;
                    }
                    else
                    {
                        throw new Exception("解析异常:" + value);
                    }
                }
                return CheckContent.Contains(value, StringComparison.OrdinalIgnoreCase) ? Exp.TrueExp : Exp.FalseExp;
            }
            if (funcs.TryGetValue(exp.Name.ToLower(), out var func))
            {
                for (int i = 0; i < exp.Args.Count; i++)
                {
                    if (exp.Args[i].Name != null)
                    {
                        exp.Args[i] = ExecExp(exp.Args[i]);
                    }
                }
                return func.Invoke(exp.Args);
            }
            else
            {
                throw new Exception("函数未注册：" + exp.Name);
            }
        }

        Exp AndFunction(List<Exp> args)
        {
            foreach (Exp item in args)
            {
                if (!ExecExp(item).Result ?? false) return Exp.FalseExp;
            }
            return Exp.TrueExp;
        }

        Exp OrFunction(List<Exp> args)
        {
            foreach (Exp item in args)
            {
                if (ExecExp(item).Result ?? false) return Exp.TrueExp;
            }
            return Exp.FalseExp;
        }

        Exp NotFunction(List<Exp> args)
        {
            foreach (Exp item in args)
            {
                return (ExecExp(item).Result ?? false) ? Exp.TrueExp : Exp.FalseExp;
            }
            return Exp.TrueExp;
        }

        Exp EqFunction(List<Exp> args)
        {
            for (int i = 1; i < args.Count; i++)
            {
                if (args[i - 1].Value != args[i].Value)
                {
                    return Exp.FalseExp;
                }
            }
            return Exp.TrueExp;
        }

        public void AddFunction(string name, Func<List<Exp>, Exp> func)
        {
            funcs.Add(name, func);
        }

        public class Exp
        {
            public string? Name { get; set; }
            public List<Exp> Args { get; set; } = new();
            public string? Value { get; set; }

            public bool? Result { get; set; }

            public bool Complete { get; set; } = false;

            public static Exp TrueExp { get; } = new() { Result = true };
            public static Exp FalseExp { get; } = new() { Result = false };

            public override string ToString()
            {
                if (Name != null)
                {
                    return Name;
                }
                return Value ?? "<null>";
            }
        }

        struct Token
        {
            public bool IsFunc;
            public string Value;

            public Token(string value, bool isFunc)
            {
                this.Value = value;
                this.IsFunc = isFunc;
            }
        }

    }
}
