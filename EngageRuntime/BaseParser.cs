using System;
using System.Collections.Generic;

namespace EngageRuntime
{
    public class BaseParser
    {
        protected readonly string Input;
        protected int Pos;

        protected readonly List<Message> Pending = new List<Message>();
        protected readonly Stack<object> Main = new Stack<object>();

        public BaseParser(string input)
        {
            //Log($"init with '{_input}'");
            this.Input = input;
            Pos = 0;
        }

        protected void Log(string message)
            => Console.WriteLine("[LOG] " + message);

        protected void Push(object _x)
        {
            //Log($"        push {_x} :: {_x.GetType()}");
            if (!Trigger(_x))
                Main.Push(_x);
        }

        protected void Schedule(Type type, Func<object, int> action)
        {
            //Log($"SCHEDULE for {_type}");
            Message message = new Message(type, action);
            Pending.Add(message);
        }

        protected void Trim(Type type)
        {
            for (int i = Pending.Count - 1; i >= 0; i--)
                if (Pending[i].IsWanted(type))
                {
                    var p = Pending[i];
                    Pending.Remove(p);
                    var code = p.Handler(null);
                    //Log($"TRIM for {_type} returned the code {Convert.ToString(code, 2)}");
                    return;
                }
            //Log($"TRIM for {_type} unsuccessful!");
        }

        protected void Flush()
        {
            //Log("FLUSH");
            foreach (var msg in Pending.ToArray())
                Log($"forced a {msg.ExpectedType} handler: return code {Convert.ToString(msg.Handler(null), 2)}");
        }

        private bool Trigger(object x)
        {
            //Log($"APPLY to {_x} :: {_x.GetType()}");
            Type t = x.GetType();
            for (int i = Pending.Count - 1; i >= 0; i--)
            {
                var candidate = Pending[i];
                if (candidate.IsWanted(t))
                {
                    int code = candidate.Handler(x);
                    if (code == Message.Misfire)
                        continue;
                    if (code == Message.Perfect)
                        Pending.Remove(candidate);
                    //Log($"trigger {_x} :: {_x.GetType()} with return code 0b{code}");
                    return true; // Consume or Perfect
                }
            }
            return false; // nothing or only misfires
        }
    }
}