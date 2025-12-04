// Decompiled with JetBrains decompiler
// Type: SonicOrca.Resources.ResourceSession
// Assembly: SonicOrca.Resources, Version=2.0.1012.10517, Culture=neutral, PublicKeyToken=null
// MVID: B73309CE-1E69-41CD-B190-BBA8714165BD
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Resources.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SonicOrca.Resources
{

    public class ResourceSession : IDisposable
    {
      private readonly ResourceTree _resourceTree;
      private readonly HashSet<Resource> _unloadedResources = new HashSet<Resource>();
      private readonly HashSet<Resource> _intermediateResources = new HashSet<Resource>();
      private readonly HashSet<Resource> _loadedResources = new HashSet<Resource>();
      private bool _disposed;
      private object _unloadedResourcesSync = new object();

      public ResourceTree ResourceTree => this._resourceTree;

      public ResourceSession(ResourceTree tree) => this._resourceTree = tree;

      public void Dispose()
      {
        this.Unload();
        this._disposed = true;
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
        Resource resource;
        if (node == null)
          resource = fullKeyPath.StartsWith("$") ? Resource.FromFile(fullKeyPath.Substring(1)) : throw new ResourceException($"Resource node not found, {fullKeyPath}.");
        else
          resource = node.Resource;
        if (resource == null)
          throw new ResourceException($"Resource node has no resource or has missing resource type loader, {fullKeyPath}.");
        this.PushDependency(resource);
      }

      public void PushDependency(Resource resource)
      {
        this.CheckDisposed();
        lock (this._unloadedResourcesSync)
        {
          if (this._unloadedResources.Contains(resource) || this._intermediateResources.Contains(resource) || this._loadedResources.Contains(resource))
            return;
          this._unloadedResources.Add(resource);
        }
      }

      public async Task LoadAsync(CancellationToken ct = default (CancellationToken), bool serial = false)
      {
        ResourceSession session1 = this;
        ResourceSession session = session1;
        CancellationToken ct1 = ct;
        session1.CheckDisposed();
        HashSet<Resource> localLoaded = new HashSet<Resource>();
        object localLoadedSync = new object();
        try
        {
          while (session1._unloadedResources.Count > 0)
          {
            session1._intermediateResources.Clear();
            session1._intermediateResources.UnionWith((IEnumerable<Resource>) session1._unloadedResources);
            session1._unloadedResources.Clear();
            if (serial)
            {
              foreach (Resource resource in session1._intermediateResources)
              {
                await resource.LoadAsync(session1, ct1);
                localLoaded.Add(resource);
              }
            }
            else
              await Task.WhenAll(session1._intermediateResources.Select<Resource, Task>((Func<Resource, Task>) (resource => Task.Run((Func<Task>) (async () =>
              {
                ct1.ThrowIfCancellationRequested();
                await resource.LoadAsync(session, ct1);
                lock (localLoadedSync)
                  localLoaded.Add(resource);
              })))).ToArray<Task>());
          }
          foreach (Resource resource in localLoaded.Where<Resource>((Func<Resource, bool>) (r => r.DependencyCount == 1)))
            resource.LoadedResource.OnLoaded();
          session1._loadedResources.UnionWith((IEnumerable<Resource>) localLoaded);
        }
        catch (Exception ex)
        {
          foreach (Resource resource in localLoaded)
            resource.Unload();
          throw;
        }
        finally
        {
          session1._intermediateResources.Clear();
        }
      }

      public void Unload()
      {
        this.CheckDisposed();
        foreach (Resource resource in this._loadedResources.Where<Resource>((Func<Resource, bool>) (r => r.LoadedResource != null)))
          resource.Unload();
        this._loadedResources.Clear();
      }

      private void CheckDisposed()
      {
        if (this._disposed)
          throw new ObjectDisposedException(typeof (ResourceSession).Name);
      }
    }
}
