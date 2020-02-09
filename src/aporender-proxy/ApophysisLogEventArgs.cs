namespace Apophysis
{
    public enum ApophysisLogType
    {
        Info = 0,
        Warning,
        Error
    }
    
    public class ApophysisLogEventArgs
    {
        public ApophysisLogEventArgs(string lpszFileName, string lpszMessage)
        {
            var tokens = (lpszMessage ?? "").Split('|');
            if (tokens.Length <= 1)
            {
                Message = lpszMessage;
            }
            else
            {
                switch (tokens[0].ToLowerInvariant())
                {
                    case "warn":
                    case "warning":
                        Type = ApophysisLogType.Warning;
                        break;
                    case "error":
                    case "fatal":
                        Type = ApophysisLogType.Error;
                        break;
                    default:
                        Type = ApophysisLogType.Info;
                        break;
                }

                Message = tokens[1];
            }
            
            FileName = lpszFileName;
        }

        public ApophysisLogType Type { get; }
        public string Message { get; }
        public string FileName { get; }
    }
}