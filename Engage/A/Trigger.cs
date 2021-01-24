using System;

namespace Engage.A
{
    public class Trigger
    {
        public string Terminal = "";
        public string NonTerminal = ""; // unused as of now
        public bool EOF = false;
        public string Flag = "";

        public override bool Equals(object obj)
        {
            var other = obj as Trigger;
            if (other == null)
            {
                Console.WriteLine("[x] Trigger compared to non-Trigger");
                return false;
            }

            if (Terminal != other.Terminal)
            {
                Console.WriteLine($"[x] Trigger: Terminal mismatch ({Terminal} vs {other.Terminal})");
                return false;
            }

            if (NonTerminal != other.NonTerminal)
            {
                Console.WriteLine("[x] Trigger: NonTerminal mismatch");
                return false;
            }

            if (EOF != other.EOF)
            {
                Console.WriteLine("[x] Trigger: EOF mismatch");
                return false;
            }

            if (Flag != other.Flag)
            {
                Console.WriteLine("[x] Trigger: Flag mismatch");
                return false;
            }

            Console.WriteLine("[√] Trigger == Trigger");
            return true;
        }
    }
}