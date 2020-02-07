using System;

namespace Apophysis
{
    public class EventParams : EventArgs
    {
        public EventParams(Event item)
        {
            TimeStamp = item.TimeStamp;
            Type = item.Level;
            Message = item.Message;
        }

        public DateTime TimeStamp { get; }
        public EventLevel Type { get; }
        public string Message { get; }

        public override bool Equals(object obj)
        {
            return obj?.GetHashCode() == this.GetHashCode();
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return $"{TimeStamp.ToLongTimeString()} - {Type} {Message}";
        }
    }
}