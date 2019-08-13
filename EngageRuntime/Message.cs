using System;

namespace EngageRuntime
{
    public class Message
    {
        public const int Misfire = 0b1000; // did not apply / did not consume / do not remove
        //public const int Neglect = 0b1101; //         apply / did not consume /        remove
        public const int Consume = 0b1110; //         apply /         consume / do not remove
        public const int Perfect = 0b1111; //         apply /         consume /        remove

        public System.Type ExpectedType;

        public Func<object, int> Handler;

        public Message(System.Type type, Func<object, int> handler)
        {
            ExpectedType = type;
            Handler = handler;
        }

        public bool IsWanted(System.Type type)
            => type == ExpectedType || type.IsSubclassOf(ExpectedType);
    }
}