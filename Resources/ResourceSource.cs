// Decompiled with JetBrains decompiler
// Type: SonicOrca.Resources.ResourceSource
// Assembly: SonicOrca.Resources, Version=2.0.1012.10517, Culture=neutral, PublicKeyToken=null
// MVID: B73309CE-1E69-41CD-B190-BBA8714165BD
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Resources.dll

using System.IO;
using System.IO.Compression;

namespace SonicOrca.Resources
{

    public abstract class ResourceSource
    {
      private readonly bool _compressed;

      public bool Compressed => this._compressed;

      public abstract long Size { get; }

      protected ResourceSource(bool compressed = false) => this._compressed = compressed;

      public abstract System.IO.Stream Read();

      private byte[] GetData()
      {
        using (MemoryStream destination = new MemoryStream())
        {
          this.Read().CopyTo((System.IO.Stream) destination);
          return destination.ToArray();
        }
      }

      public System.IO.Stream ReadCompressed()
      {
        if (this._compressed)
          return this.Read();
        using (MemoryStream memoryStream = new MemoryStream())
        {
          using (GZipStream destination = new GZipStream((System.IO.Stream) memoryStream, CompressionMode.Compress))
            this.Read().CopyTo((System.IO.Stream) destination);
          return (System.IO.Stream) new MemoryStream(memoryStream.ToArray());
        }
      }

      private static byte[] Compress(byte[] uncompressedData)
      {
        using (MemoryStream memoryStream1 = new MemoryStream())
        {
          using (MemoryStream memoryStream2 = new MemoryStream(uncompressedData))
          {
            using (GZipStream destination = new GZipStream((System.IO.Stream) memoryStream1, CompressionMode.Compress))
              memoryStream2.CopyTo((System.IO.Stream) destination);
          }
          return memoryStream1.ToArray();
        }
      }

      private static byte[] Uncompress(byte[] compressedData)
      {
        using (MemoryStream destination = new MemoryStream())
        {
          using (MemoryStream memoryStream = new MemoryStream(compressedData))
          {
            using (GZipStream gzipStream = new GZipStream((System.IO.Stream) memoryStream, CompressionMode.Decompress))
              gzipStream.CopyTo((System.IO.Stream) destination);
          }
          return destination.ToArray();
        }
      }

      public System.IO.Stream ReadUncompressed()
      {
        return this._compressed ? (System.IO.Stream) new GZipStream(this.Read(), CompressionMode.Decompress) : this.Read();
      }
    }
}
