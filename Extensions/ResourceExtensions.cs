// Decompiled with JetBrains decompiler
// Type: SonicOrca.Extensions.ResourceExtensions
// Assembly: SonicOrca.Resources, Version=2.0.1012.10517, Culture=neutral, PublicKeyToken=null
// MVID: B73309CE-1E69-41CD-B190-BBA8714165BD
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Resources.dll

using SonicOrca.Resources;
using System.Reflection;

namespace SonicOrca.Extensions
{

    public static class ResourceExtensions
    {
      public static string GetAbsolutePath(this ILoadedResource parentLoadedResource, string keyPath)
      {
        return parentLoadedResource.Resource.GetAbsolutePath(keyPath);
      }

      public static void FullfillLoadedResourcesByAttribute(this ResourceTree tree, object instance)
      {
        foreach (MemberInfo member in instance.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
          switch (member.MemberType)
          {
            case MemberTypes.Field:
              ResourcePathAttribute customAttribute1 = CustomAttributeExtensions.GetCustomAttribute<ResourcePathAttribute>(member);
              if (customAttribute1 != null)
              {
                FieldInfo fieldInfo = (FieldInfo) member;
                string path = customAttribute1.Path;
                ILoadedResource loadedResource = tree.GetLoadedResource(path);
                if (!fieldInfo.FieldType.IsAssignableFrom(loadedResource.GetType()))
                  throw new ResourceException(path + " doesn't have the correct resource type.");
                fieldInfo.SetValue(instance, (object) loadedResource);
                break;
              }
              break;
            case MemberTypes.Property:
              ResourcePathAttribute customAttribute2 = CustomAttributeExtensions.GetCustomAttribute<ResourcePathAttribute>(member);
              if (customAttribute2 != null)
              {
                PropertyInfo propertyInfo = (PropertyInfo) member;
                string path = customAttribute2.Path;
                ILoadedResource loadedResource = tree.GetLoadedResource(path);
                if (!propertyInfo.PropertyType.IsAssignableFrom(loadedResource.GetType()))
                  throw new ResourceException(path + " doesn't have the correct resource type.");
                propertyInfo.SetValue(instance, (object) loadedResource);
                break;
              }
              break;
          }
        }
      }

      public static void PushDependenciesByAttribute(this ResourceSession session, object instance)
      {
        foreach (MemberInfo member in instance.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
          ResourcePathAttribute customAttribute = CustomAttributeExtensions.GetCustomAttribute<ResourcePathAttribute>(member);
          if (customAttribute != null)
            session.PushDependency(customAttribute.Path);
        }
      }
    }
}
