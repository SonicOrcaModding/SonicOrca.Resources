// Decompiled with JetBrains decompiler
// Type: SonicOrca.Resources.ILoadedResource
// Assembly: SonicOrca.Resources, Version=2.0.1012.10517, Culture=neutral, PublicKeyToken=null
// MVID: B73309CE-1E69-41CD-B190-BBA8714165BD
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Resources.dll

using System;

namespace SonicOrca.Resources
{

    public interface ILoadedResource : IDisposable
    {
      Resource Resource { get; set; }

      void OnLoaded();
    }
}
