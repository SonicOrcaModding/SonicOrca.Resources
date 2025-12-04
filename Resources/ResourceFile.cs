// Decompiled with JetBrains decompiler
// Type: SonicOrca.Resources.ResourceFile
// Assembly: SonicOrca.Resources, Version=2.0.1012.10517, Culture=neutral, PublicKeyToken=null
// MVID: B73309CE-1E69-41CD-B190-BBA8714165BD
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Resources.dll

using SonicOrca.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SonicOrca.Resources
{

    public class ResourceFile
    {
      private static readonly char[] MagicNumberChars = new char[4]
      {
        'S',
        'O',
        'R',
        'F'
      };
      private const int LatestSupportedVersion = 2;
      private readonly string _path;

      private static int MagicNumber
      {
        get => BitConverter.ToInt32(Encoding.ASCII.GetBytes(ResourceFile.MagicNumberChars), 0);
      }

      public string Filename => this._path;

      public ResourceFile(string path) => this._path = path;

      public override string ToString() => this._path;

      public ResourceTree Scan()
      {
        try
        {
          using (FileStream input = new FileStream(this._path, FileMode.Open, FileAccess.Read))
          {
            BinaryReader binaryReader = new BinaryReader((System.IO.Stream) input);
            if (binaryReader.ReadInt32() != ResourceFile.MagicNumber)
              throw new ResourceException("Invalid resource file.");
            if (binaryReader.ReadByte() > (byte) 2)
              throw new ResourceException("Unsupport resouce file version.");
            long dataOffset = binaryReader.ReadInt64() + 13L;
            ResourceTree resourceTree = new ResourceTree();
            this.ScanTableEntry(resourceTree.Head, (System.IO.Stream) input, dataOffset);
            return resourceTree;
          }
        }
        catch (IOException ex)
        {
          throw new ResourceException(ex.Message, (Exception) ex);
        }
      }

      private void ScanTableEntry(
        ResourceTree.Node resourceNode,
        System.IO.Stream stream,
        long dataOffset,
        string fullKeyPath = null)
      {
        BinaryReader br = new BinaryReader(stream);
        ResourceFile.NodeFlags nodeFlags = (ResourceFile.NodeFlags) br.ReadByte();
        ushort num = 0;
        if (nodeFlags.HasFlag((Enum) ResourceFile.NodeFlags.HasChildren))
          num = br.ReadUInt16();
        string key = br.ReadNullTerminatedString();
        if (!string.IsNullOrEmpty(fullKeyPath))
          fullKeyPath += "/";
        fullKeyPath += key;
        Resource resource = (Resource) null;
        if (nodeFlags.HasFlag((Enum) ResourceFile.NodeFlags.HasResource))
        {
          ResourceTypeIdentifier identifier = (ResourceTypeIdentifier) br.ReadUInt16();
          string path = nodeFlags.HasFlag((Enum) ResourceFile.NodeFlags.External) ? br.ReadNullTerminatedString() : this._path;
          long offset = (long) br.ReadUInt32();
          long size = (long) br.ReadUInt32();
          if (!nodeFlags.HasFlag((Enum) ResourceFile.NodeFlags.External))
            offset += dataOffset;
          resource = new Resource(fullKeyPath, identifier, (ResourceSource) new FileResourceSource(path, offset, size, nodeFlags.HasFlag((Enum) ResourceFile.NodeFlags.Compressed)));
        }
        if (!string.IsNullOrEmpty(key))
          resourceNode = resourceNode.Add(key, resource);
        for (uint index = 0; index < (uint) num; ++index)
          this.ScanTableEntry(resourceNode, stream, dataOffset, fullKeyPath);
      }

      public void Write(ResourceTree tree)
      {
        MemoryStream tableStream = new MemoryStream();
        MemoryStream dataStream = new MemoryStream();
        try
        {
          this.WriteTableEntry(tree.Head, (System.IO.Stream) tableStream, (System.IO.Stream) dataStream);
          using (FileStream output = new FileStream(this._path, FileMode.Create, FileAccess.Write))
          {
            BinaryWriter binaryWriter = new BinaryWriter((System.IO.Stream) output);
            binaryWriter.Write(ResourceFile.MagicNumber);
            binaryWriter.Write((byte) 2);
            binaryWriter.Write(tableStream.Position);
            binaryWriter.Write(tableStream.ToArray());
            binaryWriter.Write(dataStream.ToArray());
          }
        }
        catch (IOException ex)
        {
          throw new ResourceException(ex.Message, (Exception) ex);
        }
        finally
        {
          tableStream.Dispose();
          dataStream.Dispose();
        }
      }

      private void WriteTableEntry(
        ResourceTree.Node resourceNode,
        System.IO.Stream tableStream,
        System.IO.Stream dataStream)
      {
        BinaryWriter bw = new BinaryWriter(tableStream);
        BinaryWriter binaryWriter = new BinaryWriter(dataStream);
        ResourceFile.NodeFlags nodeFlags1 = (ResourceFile.NodeFlags) 0;
        if (resourceNode.Resource != null)
        {
          ResourceType resourceType = ResourceType.FromIdentifier(resourceNode.Resource.Identifier);
          if (resourceType != null && resourceType.CompressByDefault)
            nodeFlags1 |= ResourceFile.NodeFlags.Compressed;
        }
        if (resourceNode.Children.Count > 0)
          nodeFlags1 |= ResourceFile.NodeFlags.HasChildren;
        if (resourceNode.Resource != null)
        {
          ResourceFile.NodeFlags nodeFlags2 = nodeFlags1 | ResourceFile.NodeFlags.HasResource;
          if (resourceNode.Resource.Source is FileResourceSource source && string.Compare(source.Path, this._path, true) != 0)
            nodeFlags2 |= ResourceFile.NodeFlags.External;
          nodeFlags1 = nodeFlags2 & ~ResourceFile.NodeFlags.External;
        }
        bw.Write((byte) nodeFlags1);
        if (nodeFlags1.HasFlag((Enum) ResourceFile.NodeFlags.HasChildren))
          bw.Write((short) resourceNode.Children.Count);
        bw.WriteNullTerminatedString(resourceNode.Key);
        if (nodeFlags1.HasFlag((Enum) ResourceFile.NodeFlags.HasResource))
        {
          bw.Write((ushort) resourceNode.Resource.Identifier);
          if (nodeFlags1.HasFlag((Enum) ResourceFile.NodeFlags.External))
          {
            FileResourceSource source = resourceNode.Resource.Source as FileResourceSource;
            bw.WriteNullTerminatedString(source.Path);
            bw.Write((uint) source.Offset);
            bw.Write((uint) resourceNode.Resource.Source.Size);
          }
          else
          {
            bw.Write((uint) dataStream.Position);
            long position = dataStream.Position;
            using (System.IO.Stream stream = nodeFlags1.HasFlag((Enum) ResourceFile.NodeFlags.Compressed) ? resourceNode.Resource.Source.ReadCompressed() : resourceNode.Resource.Source.ReadUncompressed())
              stream.CopyTo(dataStream);
            long num = dataStream.Position - position;
            bw.Write((uint) num);
          }
        }
        foreach (ResourceTree.Node child in (IEnumerable<ResourceTree.Node>) resourceNode.Children)
          this.WriteTableEntry(child, tableStream, dataStream);
      }

      public static void GetResourcesFromDirectory(
        ResourceTree tree,
        string path,
        string currentFullKeyPath = null)
      {
        if (!string.IsNullOrEmpty(currentFullKeyPath))
        {
          if (!currentFullKeyPath.EndsWith("/"))
            currentFullKeyPath += "/";
        }
        else
          currentFullKeyPath = string.Empty;
        currentFullKeyPath += Path.GetFileName(path).ToUpper();
        foreach (string file in Directory.GetFiles(path))
        {
          FileInfo fileInfo = new FileInfo(file);
          if (fileInfo.Name.Contains("."))
          {
            string upper = fileInfo.Name.Remove(fileInfo.Name.IndexOf('.')).Replace('_', '/').ToUpper();
            string extension = fileInfo.Name.Substring(fileInfo.Name.IndexOf('.'));
            string fullKeyPath = upper == "/" ? currentFullKeyPath : $"{currentFullKeyPath}/{upper}";
            ResourceType resourceType = ResourceType.RegisteredResourceTypes.FirstOrDefault<ResourceType>((Func<ResourceType, bool>) (x => x.DefaultExtension == extension));
            if (resourceType != null)
            {
              Resource resource = new Resource(fullKeyPath, resourceType.Identifier, (ResourceSource) new FileResourceSource(file, 0L, fileInfo.Length));
              tree.GetOrAdd(fullKeyPath).Resource = resource;
            }
          }
        }
        foreach (string directory in Directory.GetDirectories(path))
          ResourceFile.GetResourcesFromDirectory(tree, directory, currentFullKeyPath);
      }

      [Flags]
      private enum NodeFlags : byte
      {
        HasChildren = 2,
        HasResource = 4,
        External = 8,
        Compressed = 16, // 0x10
      }
    }
}
