using System;

namespace Apophysis
{
    [Flags] public enum EventLevel {
        Silent = 0,
        Info = 1,
        Warning = 2,
        Error = 4,
        Default = Warning | Error,
        Verbose = Info | Warning | Error
    }
}