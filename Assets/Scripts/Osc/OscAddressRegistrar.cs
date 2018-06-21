using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

public class OscAddressRegistrar : MonoBehaviour
{
    public List<Component> TargetComponents;

    string RootAddress { get; set; }

    /// <summary>
    /// Members of Types in this list shouldn't be given an OSC address
    /// </summary>
    readonly List<Type> IgnoreMemberTypes = new List<Type>()
    {
        typeof(GameObject),
        typeof(HideFlags)
    };

    void Start()
    {
        // Set the root address ("BaseAddress/.../...")
        // to the name of the GameObject this script is attached to (without spaces)
        RootAddress = gameObject.name.Replace(" ", "");
        // then, run initialisation asynchronously since it might take a while
        //Task.Run(() => RegisterAddresses());
        RegisterAddresses();
    }

    void RegisterAddresses()
    {
        // Register every component in the TargetComponents list
        // Use the Component's Type name for the address node
        TargetComponents.ForEach(targetComponent => RegisterObject(targetComponent, RootAddress + "/" + targetComponent.GetType().Name));
    }

    void RegisterObject(object targetObject, string address)
    {
        if (OscRouter.Instance.RegisteredMembers.Contains(targetObject)) return;

        // Get all the publicly accesible members of the Type
        MemberInfo[] objectMemberInfos = targetObject.GetType().GetMembers();

        // Register each member to an OSC address
        objectMemberInfos.ToList().ForEach(member => RegisterObjectMember(targetObject, member, address));
    }

    void RegisterObjectMember(object targetObject, MemberInfo memberInfo, string addressPrefix)
    {
        if (OscRouter.Instance.RegisteredMembers.Contains(targetObject)) return;

        Type targetObjectType = targetObject.GetType();

        var isProperty = (targetObjectType.GetProperty(memberInfo.Name) != null);
        var isField = (targetObjectType.GetField(memberInfo.Name) != null);
        // only address properties and fields... ignore methods, enums etc.
        if (!isProperty && !isField) return;

        var memberAddress = addressPrefix + "/" + memberInfo.Name;

        bool isComponent, isClass, isStruct;
        Type memberType = GetUnderlyingType(memberInfo);

        PropertyInfo property = null;
        FieldInfo field = null;

        if (isProperty)
        {
            property = (targetObjectType.GetProperty(memberInfo.Name));

            isComponent = memberType.IsSubclassOf(typeof(Component));
            isClass = memberType.IsClass;
            isStruct = (memberType.IsValueType && !memberType.IsPrimitive && !memberType.IsEnum);
        }
        else
        {
            // if it's not a property it's a field
            field = (targetObjectType.GetField(memberInfo.Name));

            isComponent = memberType.IsSubclassOf(typeof(Component));
            isClass = memberType.IsClass;
            isStruct = (memberType.IsValueType && !memberType.IsPrimitive && !memberType.IsEnum);
        }

        // Ignore any members whose types are in the IgnoreTypes list
        if (IgnoreMemberTypes.Contains(memberType)) return;

        // if the type isn't a primitive value type,
        // we need to add addresses to it's own members recursively
        bool isPrimitive = !(isComponent || isClass || isStruct);
        // if it's a String, also treat it as a primitive
        if (memberType == typeof(String)) isPrimitive = true;

        object member = null;
        try
        {
            if (isProperty) member = property.GetValue(targetObject);
            else member = field.GetValue(targetObject);
        }
        catch
        {
            // log the exception as a warning
            Debug.LogWarningFormat("Registering OSC address for '{0}' skipped due to get member object failure.\nTarget: {1}", memberType, memberAddress);
            return;
        }
        if (member == null) return;

        if (isPrimitive)
        {
            // if it is a primitive, register an OSC address for it
            if (isProperty) OscRouter.Instance.RegisterMember(targetObject, property, memberAddress);
            else OscRouter.Instance.RegisterMember(targetObject, field, memberAddress);
        }
        else
        {
            // If it's not a primitive, traverse through its own child members to register OSC addresses for them

            // TODO: Why does traversing through the members and registering them not work!?
            //RegisterObject(member, memberAddress);
        }
    }

    public static Type GetUnderlyingType(MemberInfo member)
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
}
