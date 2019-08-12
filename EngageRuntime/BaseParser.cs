using System;
using System.Collections.Generic;

namespace EngageRuntime
{
    public class BaseParser
    {
        protected string input;
        protected int pos;

        protected bool exec = false;

        protected List<Message> Pending = new List<Message>();
        protected Stack<object> Main = new Stack<object>();

        public BaseParser(string _input)
        {
            input = _input;
            pos = 0;
        }

        protected void Log(string message)
            => Console.WriteLine("[LOG] " + message);

        protected void Push(object _x)
        {
            //Log($"        push {_x} :: {_x.GetType()}");
            Main.Push(_x);
            if (!exec)
                Trigger();
        }

        protected void Schedule(Type _type, Func<object, int> _action)
        {
            //Log($"SCHEDULE for {_type}");
            Message message = new Message(_type, _action);
            Pending.Add(message);
            if (!exec)
                Trigger();
        }

        protected void Trim(Type _type)
        {
            // reverse order to remove latest added handlers first
            // (cf. dangling else)
            //for (int i = Pending.Count - 1; i >= 0; i--)
            for (int i = 0; i < Pending.Count; i++)
                if (Pending[i].IsWanted(_type))
                {
                    var p = Pending[i];
                    var code = p.Handler(null);
                    //Log($"TRIM for {_type} returned the code {Convert.ToString(code, 2)}");
                    Pending.Remove(p);
                    if (!exec)
                        Trigger();
                    return;
                }
            //Log($"TRIM for {_type} unsuccessful!");
        }

        protected void Flush()
        {
            //Log("FLUSH");
            if (!exec)
                Trigger();
            exec = true;
            foreach (var msg in Pending.ToArray())
                Log($"forced a {msg.ExpectedType} handler: return code {Convert.ToString(msg.Handler(null), 2)}");
            exec = false;
        }

        protected void Trigger()
        {
            //Log("TRIGGER");
            exec = true;
            while (Main.Count > 0)
            {
                var top = Main.Pop();
                if (!ApplyOne(top))
                {
                    Main.Push(top); // put it back
                    exec = false;
                    return; // all currently pending actions fail at the moment
                }
            }
            exec = false;
        }

        private bool ApplyOne(object _x)
        {
            //Log($"APPLY to {_x} :: {_x.GetType()}");
            Type _t = _x.GetType();
            foreach (var candidate in Pending)
            {
                if (candidate.IsWanted(_t))
                {
                    int code = candidate.Handler(_x);
                    if (code == Message.Misfire)
                        continue;
                    if (code == Message.Perfect || code == Message.Neglect)
                        Pending.Remove(candidate);
                    if (code == Message.Neglect)
                        return ApplyOne(_x); // better luck next time?
                    //Log($"APPLY to {_x} :: {_x.GetType()} SUCCESS");
                    return true; // Consume or Perfect
                }
            }
            return false; // nothing or only misfires
        }
    }
}