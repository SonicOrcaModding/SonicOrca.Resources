// Decompiled with JetBrains decompiler
// Type: SonicOrca.Resources.ResourceType
// Assembly: SonicOrca.Resources, Version=2.0.1012.10517, Culture=neutral, PublicKeyToken=null
// MVID: B73309CE-1E69-41CD-B190-BBA8714165BD
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Resources.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SonicOrca.Resources
{

    public abstract class ResourceType
    {
      private static readonly Dictionary<ResourceTypeIdentifier, ResourceType> RegisteredResourceTypeDictionary = new Dictionary<ResourceTypeIdentifier, ResourceType>();
      private readonly ResourceTypeIdentifier _identifier;

      static ResourceType()
      {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
          foreach (Type type in assembly.GetTypes())
          {
            if (!(typeof (ResourceType) == type) && typeof (ResourceType).IsAssignableFrom(type))
              Activator.CreateInstance(type);
          }
        }
      }

      public static ResourceType FromIdentifier(ResourceTypeIdentifier identifier)
      {
        return ResourceType.RegisteredResourceTypeDictionary.ContainsKey(identifier) ? ResourceType.RegisteredResourceTypeDictionary[identifier] : throw new ResourceException(identifier.ToString() + " is not a registered resource type.");
      }

      public static ResourceType FromPath(string path)
      {
        int startIndex = path.IndexOf('.');
        return ResourceType.RegisteredResourceTypes.FirstOrDefault<ResourceType>((Func<ResourceType, bool>) (x => x.DefaultExtension == ((startIndex != -1 ? path.Substring(startIndex) : (string) null) ?? throw new NotImplementedException()))) ?? throw new ResourceException("No registered resource type for this extension.");
      }

      public static IEnumerable<ResourceType> RegisteredResourceTypes
      {
        get => (IEnumerable<ResourceType>) ResourceType.RegisteredResourceTypeDictionary.Values;
      }

      protected ResourceType(ResourceTypeIdentifier identifier)
      {
        this._identifier = identifier;
        ResourceType.RegisteredResourceTypeDictionary.Add(identifier, this);
      }

      public ResourceTypeIdentifier Identifier => this._identifier;

      public virtual bool CompressByDefault => false;

      public abstract string Name { get; }

      public abstract string DefaultExtension { get; }

      public abstract Task<ILoadedResource> LoadAsync(ResourceLoadArgs e, CancellationToken ct = default (CancellationToken));
    }
}
