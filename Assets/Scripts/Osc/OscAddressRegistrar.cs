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

    List<string> ComponentAddresses { get; set; } = new List<string>();

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
        // For every component added to the public TargetComponents list
        TargetComponents.ForEach(targetComponent => RegisterComponentAdresses(targetComponent, targetComponent.GetType().Name, RootAddress));
    }

    void RegisterComponentAdresses(Component targetComponent, string targetComponentName, string parentAddress)
    {
        // Set the address of each component to its Component type name...
        // Add it to the ComponentAddresses list so that we don't double up
        var componentTypeIndex = 0;
        var componentAddress = parentAddress + "/" + targetComponentName;

        while (ComponentAddresses.Contains(componentAddress))
        {
            componentTypeIndex++;
            componentAddress = parentAddress + "/" + targetComponentName + componentTypeIndex;
        }
        ComponentAddresses.Add(componentAddress);

        // Find the Type, and get all the publicly accesible members of the Type
        Type targetComponentType = targetComponent.GetType();
        MemberInfo[] componentMembers = targetComponentType.GetMembers();

        componentMembers.ToList().ForEach(member =>
        {
            var isProperty = (targetComponentType.GetProperty(member.Name) != null);
            var isField = (targetComponentType.GetField(member.Name) != null);
            // only address properties and fields... ignore methods, enums etc.
            if (!isProperty && !isField) return;

            RegisterObjectMemberAddress(targetComponent, targetComponentType, member, componentAddress, isProperty);
        });
    }

    void RegisterObjectMemberAddress(object targetObject, Type targetObjectType, MemberInfo member, string parentAddress, bool isProperty)
    {
        var memberAddress = parentAddress + "/" + member.Name;

        bool isComponent, isClass, isStruct;
        Type memberType;

        PropertyInfo property = null;
        FieldInfo field = null;

        if (isProperty)
        {
            property = (targetObjectType.GetProperty(member.Name));
            memberType = property.PropertyType;

            isComponent = memberType.IsSubclassOf(typeof(Component));
            isClass = memberType.IsClass;
            isStruct = (memberType.IsValueType && !memberType.IsPrimitive && !memberType.IsEnum);
        }
        else
        {
            // if it's not a property it's a field
            field = (targetObjectType.GetField(member.Name));
            memberType = field.FieldType;

            isComponent = memberType.IsSubclassOf(typeof(Component));
            isClass = memberType.IsClass;
            isStruct = (memberType.IsValueType && !memberType.IsPrimitive && !memberType.IsEnum);
        }

        // Ignore any members whose types are in the IgnoreTypes list
        if (IgnoreMemberTypes.Contains(memberType)) return;

        // if the type isn't a primitive value type,
        // we need to add addresses to it's own members recursively
        bool isPrimitive = !(isComponent || isClass || isStruct);
        // if it's a String (as opposed to a string), also treat it as a primitive
        if (memberType == typeof(String)) isPrimitive = true;

        if (isPrimitive)
        {
            // if it is a primitive, register an OSC address for it
            if (isProperty)
            {
                // create Property setter Action and register it with the OscRouter
                OscRouter.Instance.MemberSetters.Add(memberAddress, (object value) =>
                {
                    property.SetValue(targetObject, value);
                });
            }
            else
            {
                // create Field setter Action and register it with the OscRouter
                OscRouter.Instance.MemberSetters.Add(memberAddress, (object value) =>
                {
                    field.SetValue(targetObject, value);
                });
            }
            Debug.Log(memberAddress + "\n");
        }
        else
        {
            // TODO: figure out child members!!!

            var addressValues = memberAddress.Split('/');
            var lastAddressValue = addressValues[addressValues.Length - 2];

            // if the lastAddressValue is the same as the name of this member,
            // it probably refers to itself.
            // Skip it so we dont recurse infinitely

            // e.g. ParticleSystem contains a member called particleSystem,
            // which refers to itself -
            // so it would loop through particleSystem.particleSystem.particleSystem...

            if (lastAddressValue == member.Name) return;

            MemberInfo[] childMembers = memberType.GetMembers();

            childMembers.ToList().ForEach(childMember =>
            {
                var childMemberIsProperty = (memberType.GetProperty(childMember.Name) != null);
                var childMemberIsField = (memberType.GetField(childMember.Name) != null);
                // only address properties and fields... ignore methods, enums etc.
                if (!childMemberIsProperty && !childMemberIsField) return;

                if (childMemberIsProperty)
                {
                    //var memberObject = targetObjectType.GetProperty(member.Name).GetValue(targetObject);
                    //var memberObjectType = memberObject.GetType();

                    //RegisterObjectMemberAddress(memberObject, memberObjectType, childMember, memberAddress, childMemberIsProperty);
                }
            });

    }
    }
}
