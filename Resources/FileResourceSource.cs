// Decompiled with JetBrains decompiler
// Type: SonicOrca.Resources.FileResourceSource
// Assembly: SonicOrca.Resources, Version=2.0.1012.10517, Culture=neutral, PublicKeyToken=null
// MVID: B73309CE-1E69-41CD-B190-BBA8714165BD
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Resources.dll

using System;
using System.IO;

namespace SonicOrca.Resources
{

    public class FileResourceSource : ResourceSource
    {
      private readonly string _path;
      private readonly long _offset;
      private readonly long _size;

      public string Path => this._path;

      public long Offset => this._offset;

      public override long Size => this._size;

      public FileResourceSource(string path, long offset, long size, bool compressed = false)
        : base(compressed)
      {
        this._path = path;
        this._offset = offset;
        this._size = size;
      }

      public override System.IO.Stream Read() => (System.IO.Stream) new FileResourceSource.Stream(this);

      public class Stream : System.IO.Stream
      {
        private readonly FileStream _fileStream;
        private readonly long _offset;
        private readonly long _size;

        public Stream(FileResourceSource source)
        {
          this._offset = source._offset;
          this._size = source._size;
          this._fileStream = new FileStream(source._path, FileMode.Open, FileAccess.Read);
          if (this._fileStream.Length < this._offset + this._size)
          {
            this._fileStream.Dispose();
            throw new FileLoadException("File not large enough to contain resource.");
          }
          this._fileStream.Position = source._offset;
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override void Flush() => throw new NotImplementedException();

        public override long Length => this._size;

        public override long Position
        {
          get => this._fileStream.Position - this._offset;
          set
          {
            if (value > this._size)
              throw new ArgumentException("Position greater than resource size.");
            this._fileStream.Position = value - this._offset;
          }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
          count = (int) Math.Min((long) count, this._size - this.Position);
          return count > 0 ? this._fileStream.Read(buffer, offset, count) : 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
          long num;
          switch (origin)
          {
            case SeekOrigin.Begin:
              num = this._offset + offset;
              break;
            case SeekOrigin.Current:
              num = this.Position + offset;
              break;
            case SeekOrigin.End:
              num = this._offset + this._size - offset;
              break;
            default:
              throw new ArgumentException("Invalid seek origin", nameof (origin));
          }
          if (num < this._offset || num > this._offset + this._size)
            throw new IOException("Invalid position to seek to");
          this.Position = num;
          return num;
        }

        public override void SetLength(long value) => throw new NotImplementedException();

        public override void Write(byte[] buffer, int offset, int count)
        {
          throw new NotImplementedException();
        }
      }
    }
}
