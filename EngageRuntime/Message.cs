using System;

namespace EngageRuntime
{
    public class Message
    {
        public const int Misfire = 1; // keep looking / did not consume anything
        public const int Consume = 2; // success / keep me for future
        public const int Perfect = 3; // termination / remove m

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