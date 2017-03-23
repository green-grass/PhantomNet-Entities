using System;
using System.Reflection;

namespace PhantomNet.Entities
{
    public static class EntitiesServiceCollectionHelpers
    {
        public static Type GetKeyType(Type currentType, Type genericBaseType, int keyTypeIndex)
        {
            return FindGenericBaseType(currentType, genericBaseType)?.GenericTypeArguments[keyTypeIndex];
        }

        public static TypeInfo FindGenericBaseType(Type currentType, Type genericBaseType)
        {
            var type = currentType.GetTypeInfo();
            while (type.BaseType != null)
            {
                type = type.BaseType.GetTypeInfo();
                var genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
                if (genericType != null && genericType == genericBaseType)
                {
                    return type;
                }
            }

            return null;
        }
    }
}
