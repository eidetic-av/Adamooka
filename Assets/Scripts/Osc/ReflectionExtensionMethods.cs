using System;
using System.Reflection;
using UnityEngine;

public static class ReflectionExtensionMethods
{
    public static PropertyInfo ToPropertyInfo(this MemberInfo member, Type parentObjectType)
    {
        if (member.IsProperty())
        {
            return parentObjectType.GetProperty(member.Name);
        }
        else
        {
            return null;
        }
    }
    public static FieldInfo ToFieldInfo(this MemberInfo member, Type parentObjectType)
    {
        if (member.IsField())
        {
            return parentObjectType.GetField(member.Name);
        }
        else
        {
            return null;
        }
    }

    public static Type GetPropertyOrFieldType(this MemberInfo member)
    {
        switch (member.MemberType)
        {
            case MemberTypes.Field:
                return ((FieldInfo)member).FieldType;
            case MemberTypes.Property:
                return ((PropertyInfo)member).PropertyType;
            default:
                return null;
        }
    }

    public static Boolean IsPrimitive(this MemberInfo member)
    {
        if (!(member.IsComponent() || member.IsClass() || member.IsStruct()) ||
            member.GetPropertyOrFieldType() == typeof(string))
        {
            return true;
        }
        else return false;
    }

    public static Boolean IsProperty(this MemberInfo member)
    {
        if (member.MemberType == MemberTypes.Property)
            return true;
        else return false;
    }

    public static bool IsField(this MemberInfo member)
    {
        if (member.MemberType == MemberTypes.Field)
            return true;
        else return false;
    }

    public static bool IsClass(this MemberInfo member)
    {
        var type = GetPropertyOrFieldType(member);
        if (type == null) return false;
        return type.IsClass;
    }

    public static bool IsComponent(this MemberInfo member)
    {
        var type = member.GetPropertyOrFieldType();
        if (type == null) return false;
        return type.IsSubclassOf(typeof(Component));
    }

    public static bool IsStruct(this MemberInfo member)
    {
        var type = member.GetPropertyOrFieldType();
        if (type == null) return false;
        return (type.IsValueType && !type.IsPrimitive && !type.IsEnum);
    }
}
