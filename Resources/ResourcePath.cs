// Decompiled with JetBrains decompiler
// Type: SonicOrca.Resources.ResourcePath
// Assembly: SonicOrca.Resources, Version=2.0.1012.10517, Culture=neutral, PublicKeyToken=null
// MVID: B73309CE-1E69-41CD-B190-BBA8714165BD
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Resources.dll

using System.IO;

namespace SonicOrca.Resources
{

    public static class ResourcePath
    {
      public static string GetPathWithoutExtension(string path)
      {
        string directoryName = Path.GetDirectoryName(path);
        string fileName = Path.GetFileName(path);
        int length = fileName.IndexOf('.');
        return length == -1 ? string.Empty : Path.Combine(directoryName, fileName.Substring(0, length));
      }

      public static string GetExtension(string path)
      {
        string fileName = Path.GetFileName(path);
        int startIndex = fileName.IndexOf('.');
        return startIndex == -1 ? string.Empty : fileName.Substring(startIndex);
      }

      internal static string GetRelativeFileResourceFromAbsolute(string parent, string relative)
      {
        if (parent.StartsWith("$"))
          parent = parent.Substring(1);
        if (relative.StartsWith("/"))
          relative = relative.Substring(1);
        string directoryName = Path.GetDirectoryName(parent);
        string searchPattern = ResourcePath.GetPathWithoutExtension(Path.GetFileName(parent)) + "_" + relative + ".*";
        string[] files = Directory.GetFiles(directoryName, searchPattern, SearchOption.TopDirectoryOnly);
        if (files.Length == 0)
          throw new ResourceException($"There are one or more resources with the same key in {directoryName}.");
        return "$" + files[0];
      }
    }
}
