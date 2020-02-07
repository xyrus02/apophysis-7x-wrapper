using System;

namespace Apophysis
{
    [Flags] 
    public enum EventSource
    {
        None = 0,
        General = 1,
        Parser = 2,
        Render = 4,
        All = General | Render | Parser,
        Default = All
    }
}