using System;
using System.Collections.Generic;

namespace EngageRuntime
{
    public class BaseParser
    {
        protected string input;
        protected int pos;

        protected List<Message> Pending = new List<Message>();
        private Stack<object> Main = new Stack<object>();

        public BaseParser(string _input)
        {
            input = _input;
            pos = 0;
        }

        protected void Log(string message)
            => Console.WriteLine("[LOG] " + message);

        protected void Push(object _x)
        {
            Main.Push(_x);
            Trigger();
        }

        protected void Schedule(Type _type, Func<object, int> _action)
        {
            Message message = new Message(_type, _action);
            Pending.Add(message);
            Trigger();
        }

        private void Trigger()
        {
            while (Main.Count > 0)
                if (ApplyOne(Main.Peek()))
                    Main.Pop(); // discard and continue
                else
                    return; // all currently pending actions fail at the moment
        }

        private bool ApplyOne(object _x)
        {
            Type _t = _x.GetType();
            foreach (var candidate in Pending)
            {
                if (candidate.IsWanted(_t))
                {
                    int code = candidate.Handler(_x);
                    if (code == Message.Misfire)
                        continue;
                    if (code == Message.Perfect)
                        Pending.Remove(candidate);
                    return true; // Consume or Perfect
                }
            }
            return false;
        }
    }
}