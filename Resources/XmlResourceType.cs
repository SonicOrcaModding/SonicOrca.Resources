// Decompiled with JetBrains decompiler
// Type: SonicOrca.Resources.XmlResourceType
// Assembly: SonicOrca.Resources, Version=2.0.1012.10517, Culture=neutral, PublicKeyToken=null
// MVID: B73309CE-1E69-41CD-B190-BBA8714165BD
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Resources.dll

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SonicOrca.Resources
{

    public class XmlResourceType : ResourceType
    {
      public XmlResourceType()
        : base(ResourceTypeIdentifier.Xml)
      {
      }

      public override string Name => "xml";

      public override string DefaultExtension => ".xml";

      public override bool CompressByDefault => true;

      public override Task<ILoadedResource> LoadAsync(ResourceLoadArgs e, CancellationToken ct = default (CancellationToken))
      {
        return Task.Run<ILoadedResource>((Func<ILoadedResource>) (() => (ILoadedResource) new XmlLoadedResource(e.InputStream)
        {
          Resource = e.Resource
        }));
      }
    }
}
