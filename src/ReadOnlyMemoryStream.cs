using System;
using System.IO;

namespace Apophysis
{
    public sealed class ReadOnlyMemoryStream : Stream
    {
        private readonly Stream _baseStream;

        public ReadOnlyMemoryStream(byte[] data)
        {
            this._baseStream = new MemoryStream(data);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _baseStream.Dispose();
            base.Dispose(disposing);
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;

        public override void Flush()
        {
            throw new NotSupportedException();
        }
        public override long Length => _baseStream.Length;

        public override long Position
        {
            get => _baseStream.Position;
            set => _baseStream.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _baseStream.Read(buffer, offset, count);
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _baseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}