// Decompiled with JetBrains decompiler
// Type: SonicOrca.Resources.ResourceTree
// Assembly: SonicOrca.Resources, Version=2.0.1012.10517, Culture=neutral, PublicKeyToken=null
// MVID: B73309CE-1E69-41CD-B190-BBA8714165BD
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Resources.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SonicOrca.Resources
{

    public class ResourceTree : IEnumerable<ResourceTree.Node>, IEnumerable
    {
      private readonly ResourceTree.Node _head = new ResourceTree.Node();

      public ResourceTree.Node Head => this._head;

      public ResourceTree.Node this[string fullKeyPath]
      {
        get
        {
          if (string.IsNullOrEmpty(fullKeyPath))
            return (ResourceTree.Node) null;
          ResourceTree.Node head = this._head;
          string str = fullKeyPath;
          char[] chArray = new char[1]{ '/' };
          foreach (string key in str.Split(chArray))
          {
            if ((head = head[key]) == null)
              break;
          }
          return head;
        }
      }

      public ResourceTree.Node GetOrAdd(string fullKeyPath)
      {
        ResourceTree.Node orAdd = this._head;
        string str = fullKeyPath;
        char[] chArray = new char[1]{ '/' };
        foreach (string key in str.Split(chArray))
          orAdd = orAdd.GetOrAdd(key);
        return orAdd;
      }

      public ResourceTree.Node SetOrAdd(string fullKeyPath, Resource resource)
      {
        ResourceTree.Node orAdd = this.GetOrAdd(fullKeyPath);
        orAdd.Resource = resource;
        return orAdd;
      }

      public ResourceTree.Node SetOrAddFromFile(string resourcePath)
      {
        return this.SetOrAdd("$" + resourcePath, Resource.FromFile(resourcePath));
      }

      public ResourceTree.Node SetOrAddFromFile(string fullKeyPath, string resourcePath)
      {
        return this.SetOrAdd(fullKeyPath, Resource.FromFile(resourcePath, fullKeyPath));
      }

      public ILoadedResource GetLoadedResource(string fullKeyPath)
      {
        ResourceTree.Node node = this[fullKeyPath];
        if (node == null)
          throw new ResourceException(fullKeyPath + " doesn't exist.");
        if (node.Resource == null)
          throw new ResourceException(fullKeyPath + " has no registered resource.");
        return node.Resource.LoadedResource ?? throw new ResourceException(fullKeyPath + " has not been loaded.");
      }

      public T GetLoadedResource<T>(string fullKeyPath) where T : ILoadedResource
      {
        return this.GetLoadedResource(fullKeyPath) is T loadedResource ? loadedResource : throw new ResourceException(fullKeyPath + " doesn't have the correct resource type.");
      }

      public T GetLoadedResource<T>(ILoadedResource parentLoadedResource, string fullKeyPath) where T : ILoadedResource
      {
        fullKeyPath = parentLoadedResource.Resource.GetAbsolutePath(fullKeyPath);
        return this.GetLoadedResource<T>(fullKeyPath);
      }

      public bool TryGetLoadedResource(string fullKeyPath, out ILoadedResource loadedResource)
      {
        loadedResource = (ILoadedResource) null;
        ResourceTree.Node node = this[fullKeyPath];
        if (node == null || node.Resource == null)
          return false;
        loadedResource = node.Resource.LoadedResource;
        return loadedResource != null;
      }

      public bool TryGetLoadedResource<T>(string fullKeyPath, out T loadedResource) where T : class, ILoadedResource
      {
        ILoadedResource loadedResource1;
        if (this.TryGetLoadedResource(fullKeyPath, out loadedResource1))
        {
          loadedResource = loadedResource1 as T;
          return (object) loadedResource != null;
        }
        loadedResource = default (T);
        return false;
      }

      public void RemoveEmptyNodes() => this.RemoveEmptyNodes(this._head);

      private void RemoveEmptyNodes(ResourceTree.Node node)
      {
        foreach (ResourceTree.Node node1 in node.ToArray<ResourceTree.Node>())
          this.RemoveEmptyNodes(node1);
        if (node.Parent == null || node.Resource != null || node.Children.Count != 0)
          return;
        node.Parent.Remove(node.Key);
      }

      public void MergeWith(ResourceTree tree)
      {
        foreach (KeyValuePair<string, ResourceTree.Node> keyValuePair in (IEnumerable<KeyValuePair<string, ResourceTree.Node>>) tree.GetNodeListing())
        {
          ResourceTree.Node orAdd = this.GetOrAdd(keyValuePair.Key);
          if (keyValuePair.Value.Resource != null)
            orAdd.Resource = keyValuePair.Value.Resource;
        }
      }

      public IDictionary<string, ResourceTree.Node> GetNodeListing()
      {
        Dictionary<string, ResourceTree.Node> nodeListing = new Dictionary<string, ResourceTree.Node>();
        Stack<Tuple<string, ResourceTree.Node>> tupleStack = new Stack<Tuple<string, ResourceTree.Node>>();
        tupleStack.Push(new Tuple<string, ResourceTree.Node>(string.Empty, this._head));
        do
        {
          Tuple<string, ResourceTree.Node> tuple = tupleStack.Pop();
          string key = (string.IsNullOrEmpty(tuple.Item1) ? string.Empty : tuple.Item1 + "/") + tuple.Item2.Key;
          nodeListing[key] = tuple.Item2;
          foreach (ResourceTree.Node node in tuple.Item2)
            tupleStack.Push(new Tuple<string, ResourceTree.Node>(key, node));
        }
        while (tupleStack.Count > 0);
        return (IDictionary<string, ResourceTree.Node>) nodeListing;
      }

      public IDictionary<string, Resource> GetResourceListing()
      {
        Dictionary<string, Resource> resourceListing = new Dictionary<string, Resource>();
        foreach (KeyValuePair<string, ResourceTree.Node> keyValuePair in this.GetNodeListing().Where<KeyValuePair<string, ResourceTree.Node>>((Func<KeyValuePair<string, ResourceTree.Node>, bool>) (x => x.Value.Resource != null)))
          resourceListing[keyValuePair.Key] = keyValuePair.Value.Resource;
        return (IDictionary<string, Resource>) resourceListing;
      }

      public IEnumerator<ResourceTree.Node> GetEnumerator()
      {
        Stack<ResourceTree.Node> stack = new Stack<ResourceTree.Node>();
        stack.Push(this._head);
        do
        {
          ResourceTree.Node topNode = stack.Pop();
          yield return topNode;
          foreach (ResourceTree.Node node in topNode)
            stack.Push(node);
          topNode = (ResourceTree.Node) null;
        }
        while (stack.Count > 0);
      }

      IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this.GetEnumerator();

      public class Node : IEnumerable<ResourceTree.Node>, IEnumerable
      {
        private readonly ResourceTree.Node _parent;
        private readonly Dictionary<string, ResourceTree.Node> _children = new Dictionary<string, ResourceTree.Node>();
        private readonly string _key;

        public ResourceTree.Node Parent => this._parent;

        public ICollection<ResourceTree.Node> Children
        {
          get => (ICollection<ResourceTree.Node>) this._children.Values;
        }

        public string Key => this._key;

        public Resource Resource { get; set; }

        public Node()
        {
        }

        public Node(ResourceTree.Node parent, string key, Resource resource = null)
        {
          this._parent = parent;
          this._key = key;
          this.Resource = resource;
        }

        public ResourceTree.Node this[string key]
        {
          get => !this._children.ContainsKey(key) ? (ResourceTree.Node) null : this._children[key];
        }

        public ResourceTree.Node Add(string key, Resource resource)
        {
          ResourceTree.Node orAdd = this.GetOrAdd(key);
          orAdd.Resource = resource;
          return orAdd;
        }

        public ResourceTree.Node GetOrAdd(string key)
        {
          if (this._children.ContainsKey(key))
            return this._children[key];
          ResourceTree.Node orAdd = new ResourceTree.Node(this, key);
          this._children.Add(key, orAdd);
          return orAdd;
        }

        public void Remove(string key) => this._children.Remove(key);

        public IEnumerator<ResourceTree.Node> GetEnumerator()
        {
          return (IEnumerator<ResourceTree.Node>) this._children.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this.GetEnumerator();

        public override string ToString() => this._key;
      }
    }
}
