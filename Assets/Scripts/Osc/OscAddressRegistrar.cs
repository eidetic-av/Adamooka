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
        if (!IsValidObject(targetObject)) return;

        // Get all the publicly accesible members of the Type
        MemberInfo[] objectMemberInfos = targetObject.GetType().GetMembers();

        // Register each member to an OSC address
        objectMemberInfos.ToList().ForEach(memberInfo => RegisterObjectMember(targetObject, memberInfo, address));
    }

    void RegisterObjectMember(object targetObject, MemberInfo memberInfo, string addressPrefix)
    {
        if (!IsValidMember(memberInfo)) return;

        // the address node is the name of the member
        var memberAddress = addressPrefix + "/" + memberInfo.Name;

        if (memberInfo.IsPrimitive())
        {
            // if it is a primitive, register an OSC address for it
            if (memberInfo.IsProperty())
            {
                PropertyInfo propertyInfo = memberInfo.ToPropertyInfo(targetObject.GetType());
                OscRouter.Instance.RegisterMember(targetObject, propertyInfo, memberAddress);
            }
            else if (memberInfo.IsField())
            {
                FieldInfo fieldInfo = memberInfo.ToFieldInfo(targetObject.GetType());
                OscRouter.Instance.RegisterMember(targetObject, fieldInfo, memberAddress);
            }
        }
        else
        {
            RegisterChildMembers(targetObject, memberInfo, memberAddress);
        }
    }

    void RegisterChildMembers(object parentObject, MemberInfo memberInfoToTraverse, string addressPrefix)
    {
        var memberType = memberInfoToTraverse.GetPropertyOrFieldType();

        memberType.GetMembers().ToList().ForEach(childMemberInfo =>
        {
            if (!IsValidMember(childMemberInfo)) return;

            var childMemberAddress = addressPrefix + "/" + childMemberInfo.Name;

            if (childMemberInfo.IsPrimitive())
            {
                if (childMemberInfo.IsField())
                {
                    Debug.Log(childMemberInfo.Name);
                    var parentPropertyInfo = memberInfoToTraverse as PropertyInfo;
                    var member = parentPropertyInfo.GetValue(parentObject);
                    var childMember = (childMemberInfo as FieldInfo).GetValue(member);
                    Debug.Log(childMember == null);
                }
                //OscRouter.Instance.AddMemberSetter(childMemberAddress, childMemberInfo, (object value) =>
                //{
                    
                //});
            }
            else
            {
                // TODO: traversing further is still causing a stack overflow...
                //RegisterChildMembers(childMemberInfo, childMemberAddress);
            }
        });
    }

    bool IsValidObject(object objectToCheck)
    {
        // if it's already registered to an OSC address, skip the object
        if (OscRouter.Instance.RegisteredMembers.Contains(objectToCheck)) return false;

        // otherwise it's valid
        return true;
    }

    bool IsValidMember(MemberInfo memberInfoToCheck)
    {
        // if it's already registered to an OSC address, skip the object
        if (OscRouter.Instance.RegisteredMembers.Contains(memberInfoToCheck)) return false;

        Type memberType = memberInfoToCheck.GetPropertyOrFieldType();

        // if above returns null, it's not a property or a field, so ignore it
        if (memberType == null) return false;

        // Ignore any members whose types are in the IgnoreTypes list
        if (IgnoreMemberTypes.Contains(memberType)) return false;

        // otherwise it's valid
        return true;
    }
}
