// Decompiled with JetBrains decompiler
// Type: SonicOrca.Resources.Resource
// Assembly: SonicOrca.Resources, Version=2.0.1012.10517, Culture=neutral, PublicKeyToken=null
// MVID: B73309CE-1E69-41CD-B190-BBA8714165BD
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Resources.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SonicOrca.Resources
{

    public sealed class Resource : IDisposable
    {
      private readonly Lockable<List<Resource>> _childResources = new Lockable<List<Resource>>(new List<Resource>());
      private readonly Semaphore _loadSemaphore = new Semaphore(1, 1);
      private readonly string _fullKeyPath;
      private readonly ResourceTypeIdentifier _identifier;
      private readonly ResourceSource _source;

      public static int LoadedResourceCount { get; private set; }

      public string FullKeyPath => this._fullKeyPath;

      public ResourceTypeIdentifier Identifier => this._identifier;

      public ResourceSource Source => this._source;

      public ILoadedResource LoadedResource { get; private set; }

      public int DependencyCount { get; private set; }

      public Resource(string fullKeyPath, ResourceTypeIdentifier identifier, ResourceSource source)
      {
        this._fullKeyPath = fullKeyPath;
        this._identifier = identifier;
        this._source = source;
      }

      public void Dispose() => this._loadSemaphore.Dispose();

      public override string ToString()
      {
        return $"{this.Identifier} Loaded = {this.LoadedResource != null} DependencyCount = {this.DependencyCount}";
      }

      public async Task LoadAsync(ResourceSession session, CancellationToken ct = default (CancellationToken), int level = 0)
      {
        Resource resource1 = this;
        Resource resource = resource1;
        ResourceSession session1 = session;
        int level1 = level;
        CancellationToken ct1 = ct;
        resource1._loadSemaphore.WaitOne();
        if (resource1.LoadedResource == null)
        {
          ResourceType resourceType = ResourceType.FromIdentifier(resource1.Identifier);
          if (resourceType == null)
            throw new ResourceException($"No registered resource type, {resource1.Identifier}");
          using (System.IO.Stream uncompressedStream = resource1.Source.ReadUncompressed())
          {
            ResourceLoadArgs loadArguments = new ResourceLoadArgs(session1.ResourceTree, resource1, uncompressedStream);
            try
            {
              ILoadedResource loadedResource = await resourceType.LoadAsync(loadArguments, ct1);
              resource1.LoadedResource = loadedResource;
              if (!Environment.Is64BitProcess)
              {
                foreach (Resource dependency in (IEnumerable<Resource>) loadArguments.Dependencies)
                  resource1.LoadChildResourceAsync(session1, dependency, level1, ct1).Wait();
              }
              else
                await Task.WhenAll(((IEnumerable<Resource>) loadArguments.Dependencies).Select<Resource, Task>((Func<Resource, Task>) (x => resource.LoadChildResourceAsync(session1, x, level1, ct1))).ToArray<Task>());
            }
            catch (Exception ex)
            {
              throw;
            }
            loadArguments = (ResourceLoadArgs) null;
          }
          resource1.DependencyCount = 1;
          ++Resource.LoadedResourceCount;
        }
        else
          resource1.DependencyCount++;
        resource1._loadSemaphore.Release();
      }

      private async Task LoadChildResourceAsync(
        ResourceSession session,
        Resource resource,
        int level,
        CancellationToken ct = default (CancellationToken))
      {
        bool resourceWasLoaded = resource.LoadedResource != null;
        await resource.LoadAsync(session, ct, level + 1);
        if (!resourceWasLoaded)
          resource.LoadedResource.OnLoaded();
        lock (this._childResources.Sync)
          this._childResources.Instance.Add(resource);
      }

      public void Unload(int level = 0)
      {
        this._loadSemaphore.WaitOne();
        if (this.LoadedResource != null)
        {
          if (this.DependencyCount == 1)
          {
            foreach (Resource resource in this._childResources.Instance)
              resource.Unload(level + 1);
            this._childResources.Instance.Clear();
            this.LoadedResource.Dispose();
            this.LoadedResource = (ILoadedResource) null;
            this.DependencyCount = 0;
            --Resource.LoadedResourceCount;
          }
          else
            --this.DependencyCount;
        }
        this._loadSemaphore.Release();
      }

      public void Export(string path)
      {
        byte[] buffer = new byte[1024 /*0x0400*/];
        using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
        {
          using (System.IO.Stream stream = this.Source.ReadUncompressed())
          {
            int count;
            do
            {
              count = stream.Read(buffer, 0, buffer.Length);
              fileStream.Write(buffer, 0, count);
            }
            while (count != 0);
          }
        }
      }

      public static Resource FromFile(string path)
      {
        path = Path.GetFullPath(path);
        string fullKeyPath = "$" + path;
        ResourceTypeIdentifier identifier1 = ResourceType.FromPath(path).Identifier;
        ResourceSource resourceSource = (ResourceSource) new FileResourceSource(path, 0L, new FileInfo(path).Length);
        int identifier2 = (int) identifier1;
        ResourceSource source = resourceSource;
        return new Resource(fullKeyPath, (ResourceTypeIdentifier) identifier2, source);
      }

      public static Resource FromFile(string path, string fullResourceKeyPath)
      {
        return new Resource(fullResourceKeyPath, ResourceType.FromPath(path).Identifier, (ResourceSource) new FileResourceSource(path, 0L, new FileInfo(path).Length));
      }

      public string GetAbsolutePath(string keyPath)
      {
        if (keyPath.StartsWith("/"))
        {
          if (this._fullKeyPath.StartsWith("$"))
            return ResourcePath.GetRelativeFileResourceFromAbsolute(this._fullKeyPath, keyPath);
          keyPath = this._fullKeyPath + keyPath;
          Stack<string> stringStack = new Stack<string>();
          int length;
          while ((length = keyPath.IndexOf('/')) != -1)
          {
            if (length == 0)
            {
              keyPath = keyPath.Substring(1);
              if (stringStack.Count > 0)
                stringStack.Pop();
            }
            else
            {
              stringStack.Push(keyPath.Substring(0, length));
              keyPath = keyPath.Substring(length + 1);
            }
          }
          stringStack.Push(keyPath);
          keyPath = string.Join("/", ((IEnumerable<string>) stringStack.ToArray()).Reverse<string>());
        }
        return keyPath;
      }
    }
}
