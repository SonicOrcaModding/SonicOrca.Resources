// Decompiled with JetBrains decompiler
// Type: SonicOrca.Resources.XmlLoadedResource
// Assembly: SonicOrca.Resources, Version=2.0.1012.10517, Culture=neutral, PublicKeyToken=null
// MVID: B73309CE-1E69-41CD-B190-BBA8714165BD
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Resources.dll

using System;
using System.Xml;

namespace SonicOrca.Resources
{

    public class XmlLoadedResource : ILoadedResource, IDisposable
    {
      private readonly XmlDocument _xmlDocument;

      public Resource Resource { get; set; }

      public XmlDocument XmlDocument => this._xmlDocument;

      public XmlLoadedResource(System.IO.Stream stream)
      {
        this._xmlDocument = new XmlDocument();
        this._xmlDocument.Load(stream);
      }

      public virtual void OnLoaded()
      {
      }

      public virtual void Dispose()
      {
      }
    }
}
