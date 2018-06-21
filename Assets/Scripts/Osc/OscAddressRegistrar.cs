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
        TargetComponents.ForEach(targetComponent =>
        {
            var componentAddress = RootAddress + "/" + targetComponent.GetType().Name;
            RegisterObject(targetComponent, componentAddress);
        });
    }

    void RegisterObject(object targetObject, string address)
    {
        if (OscRouter.Instance.RegisteredMembers.Contains(targetObject)) return;

        // Get all the publicly accesible members of the Type
        MemberInfo[] objectMemberInfos = targetObject.GetType().GetMembers();

        // Register each member to an OSC address
        objectMemberInfos.ToList().ForEach(memberInfo => RegisterObjectMember(targetObject, memberInfo, address));
    }

    void RegisterObjectMember(object targetObject, MemberInfo memberInfo, string addressPrefix)
    {
        if (OscRouter.Instance.RegisteredMembers.Contains(targetObject)) return;

        Type targetObjectType = targetObject.GetType();

        // only address properties and fields... ignore methods, enums etc.
        if (!memberInfo.IsProperty() && !memberInfo.IsField()) return;

        var memberAddress = addressPrefix + "/" + memberInfo.Name;

        Type memberType = memberInfo.GetPropertyOrFieldType();

        // Ignore any members whose types are in the IgnoreTypes list
        if (IgnoreMemberTypes.Contains(memberType)) return;

        if (memberInfo.IsPrimitive())
        {
            // if it is a primitive, register an OSC address for it
            if (memberInfo.IsProperty())
            {
                PropertyInfo propertyInfo = memberInfo.ToPropertyInfo(targetObjectType);
                OscRouter.Instance.RegisterMember(targetObject, propertyInfo, memberAddress);
            }
            else if (memberInfo.IsField())
            {
                FieldInfo fieldInfo = memberInfo.ToFieldInfo(targetObjectType);
                OscRouter.Instance.RegisterMember(targetObject, fieldInfo, memberAddress);
            }
        }
        else
        {
            RegisterChildMembers(targetObject, memberInfo, memberAddress);
        }
    }

    bool skip = false;

    void RegisterChildMembers(object parentTargetObject, MemberInfo memberInfoToTraverse, string addressPrefix)
    {
        if (skip) return;
        Debug.Log("Traversing: \n" + addressPrefix);
        skip = true;

        var memberType = memberInfoToTraverse.GetPropertyOrFieldType();
        Debug.Log(memberType.Name);

        memberType.GetMembers().ToList().ForEach(childMemberInfo =>
        {
            var childMemberName = childMemberInfo.Name;
            var childMemberType = childMemberInfo.GetPropertyOrFieldType();

            if (childMemberType == null) return;

            Debug.Log(" -> " + childMemberName + "\n      " + childMemberType.Name);
        });
    }
}
