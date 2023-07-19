namespace EaxOpenClose
{
    public class TagOpen2 : TagEvent
    {
        public string n;

        public TagOpen2(string _n)
        {
            n = _n;
        }
    }

    public class TagClose2 : TagEvent
    {
        public string n;

        public TagClose2(string _n)
        {
            n = _n;
        }
    }
}