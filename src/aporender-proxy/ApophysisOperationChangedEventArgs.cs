namespace Apophysis
{
    public class ApophysisOperationChangedEventArgs
    {
        public ApophysisOperationChangedEventArgs(int dwOperation)
        {
            CurrentOperation = (ApophysisOperation)dwOperation;
        }

        public ApophysisOperation CurrentOperation { get; }
    }
}