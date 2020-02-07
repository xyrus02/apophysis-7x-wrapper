using System;

namespace Apophysis
{
    public struct Event
    {
        public DateTime TimeStamp;
        public EventLevel Level;
        public string Message;

        public override bool Equals(object obj)
        {
            return obj?.GetHashCode() == GetHashCode();
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return $"{TimeStamp.ToLongTimeString()} - {Level} {Message}";
        }
    }
}