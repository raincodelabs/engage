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
                return false;
            return Terminal == other.Terminal
                   && NonTerminal == other.NonTerminal
                   && EOF == other.EOF
                   && Flag == other.Flag
                ;
        }
    }
}