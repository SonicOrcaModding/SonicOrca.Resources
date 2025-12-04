// Decompiled with JetBrains decompiler
// Type: SonicOrca.Resources.ResourceLoadArgs
// Assembly: SonicOrca.Resources, Version=2.0.1012.10517, Culture=neutral, PublicKeyToken=null
// MVID: B73309CE-1E69-41CD-B190-BBA8714165BD
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Resources.dll

using System.Collections.Generic;

namespace SonicOrca.Resources
{

    public sealed class ResourceLoadArgs
    {
      private readonly ResourceTree _resourceTree;
      private readonly Resource _resource;
      private readonly System.IO.Stream _inputStream;
      private readonly Lockable<List<Resource>> _dependencies = new Lockable<List<Resource>>(new List<Resource>());

      public ResourceTree ResourceTree => this._resourceTree;

      public Resource Resource => this._resource;

      public System.IO.Stream InputStream => this._inputStream;

      public IReadOnlyCollection<Resource> Dependencies
      {
        get => (IReadOnlyCollection<Resource>) this._dependencies.Instance;
      }

      public ResourceLoadArgs(ResourceTree resourceTree, Resource resource, System.IO.Stream inputStream)
      {
        this._resourceTree = resourceTree;
        this._resource = resource;
        this._inputStream = inputStream;
      }

      public void PushDependencies(IEnumerable<string> fullKeyPaths)
      {
        foreach (string fullKeyPath in fullKeyPaths)
          this.PushDependency(fullKeyPath);
      }

      public void PushDependencies(params string[] fullKeyPaths)
      {
        foreach (string fullKeyPath in fullKeyPaths)
          this.PushDependency(fullKeyPath);
      }

      public void PushDependency(string fullKeyPath)
      {
        ResourceTree.Node node = this._resourceTree[fullKeyPath];
        if (node == null)
          node = fullKeyPath.StartsWith("$") ? this._resourceTree.SetOrAddFromFile(fullKeyPath.Substring(1)) : throw new ResourceException($"Resource node not found, {fullKeyPath}.");
        this.PushDependency(node.Resource ?? throw new ResourceException($"Resource node has no resource or has missing resource type loader, {fullKeyPath}."));
      }

      private void PushDependency(Resource resource)
      {
        lock (this._dependencies.Sync)
          this._dependencies.Instance.Add(resource);
      }

      public string GetAbsolutePath(string keyPath) => this._resource.GetAbsolutePath(keyPath);
    }
}
