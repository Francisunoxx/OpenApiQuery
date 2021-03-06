using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OpenApiQuery.Utils
{
    internal static class ReflectionHelper
    {
        public static MemberInfo GetMember<T, TRet>(Expression<Func<T, TRet>> expr)
        {
            return ((MemberExpression)expr.Body).Member;
        }

        public static MethodInfo GetMethod<T, TRet>(Expression<Func<T, TRet>> expr)
        {
            var method = ((MethodCallExpression)expr.Body).Method;
            return method.IsGenericMethod ? method.GetGenericMethodDefinition() : method;
        }

        public static MethodInfo GetMethod<TRet>(Expression<Func<TRet>> expr)
        {
            var method = ((MethodCallExpression)expr.Body).Method;
            return method.IsGenericMethod ? method.GetGenericMethodDefinition() : method;
        }


        public static bool ImplementsEnumerable(Type type, out Type itemType)
        {
            // 1:many
            var enumerable = type.GetInterfaces().FirstOrDefault(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            if (enumerable != null)
            {
                itemType = enumerable.GetGenericArguments()[0];
                return true;
            }

            itemType = null;
            return false;
        }

        public static bool IsEnumerable(Type type, out Type itemType)
        {
            // handle string as non enumerable even though it is enumerable<char>
            if (type == typeof(string))
            {
                itemType = null;
                return false;
            }

            // 1:many
            if (type.IsGenericType &&
                typeof(IEnumerable<>).MakeGenericType(type.GetGenericArguments()[0])
                    .IsAssignableFrom(type))
            {
                itemType = type.GetGenericArguments()[0];
                return true;
            }

            if (type.IsArray)
            {
                itemType = type.GetElementType();
                return true;
            }

            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
            {
                itemType = typeof(object);
                return true;
            }

            itemType = null;
            return false;
        }

        public static bool ImplementsDictionary(Type type, out Type keyType, out Type valueType)
        {
            if (type.IsGenericType)
            {
                var args = type.GetGenericArguments();
                if(args.Length == 2 &&
                   typeof(IDictionary<,>).MakeGenericType(args[0], args[1])
                    .IsAssignableFrom(type))
                {
                    keyType = args[0];
                    valueType = args[1];
                    return true;
                }
            }

            keyType = null;
            valueType = null;
            return false;
        }
    }
}
