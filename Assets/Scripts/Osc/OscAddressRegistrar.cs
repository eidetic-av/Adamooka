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
    List<string> ComponentAddresses { get; set; } = new List<string>();

    /// <summary>
    /// Members of Types in this list shouldn't be given an OSC address
    /// </summary>
    readonly List<Type> IgnoreTypes = new List<Type>()
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
        Task.Run(() => RegisterAddresses());
    }

    void RegisterAddresses()
    {
        // For every component added to the public TargetComponents list
        TargetComponents.ForEach(targetComponent =>
        {
            // Set the address of each component to its Component type name...
            // Add it to the ComponentAddresses list so that we don't double up
            // If there are multiple, add an index to the address
            // e.g. the second ParticleSystem on an object will be addressed as "RootAddress/ParticleSystem1"
            var componentTypeIndex = 0;
            var componentAddress = RootAddress + "/" + targetComponent.GetType().Name;

            while (ComponentAddresses.Contains(componentAddress))
            {
                componentTypeIndex++;
                componentAddress = RootAddress + "/" + targetComponent.GetType().Name + componentTypeIndex;
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

                RegisterComponentMemberAddress(targetComponent, targetComponentType, member, componentAddress, isProperty);
            });
        });
    }

    void RegisterComponentMemberAddress(Component component, Type componentType, MemberInfo member, string parentAddress, bool isProperty)
    {
        var memberAddress = parentAddress + "/" + member.Name;

        bool isComponent, isClass, isStruct;
        Type memberType;

        PropertyInfo property = null;
        FieldInfo field = null;

        if (isProperty)
        {
            property = (componentType.GetProperty(member.Name));
            memberType = property.PropertyType;

            isComponent = memberType.IsSubclassOf(typeof(Component));
            isClass = memberType.IsClass;
            isStruct = (memberType.IsValueType && !memberType.IsPrimitive && !memberType.IsEnum);
        }
        else
        {
            // if it's not a property it's a field
            field = (componentType.GetField(member.Name));
            memberType = field.FieldType;

            isComponent = memberType.IsSubclassOf(typeof(Component));
            isClass = memberType.IsClass;
            isStruct = (memberType.IsValueType && !memberType.IsPrimitive && !memberType.IsEnum);
        }

        // Ignore any members whose types are in the IgnoreTypes list
        if (IgnoreTypes.Contains(memberType)) return;

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
                    property.SetValue(component, value);
                });
            }
            else
            {
                // create Field setter Action and register it with the OscRouter
                OscRouter.Instance.MemberSetters.Add(memberAddress, (object value) =>
                {
                    field.SetValue(component, value);
                });
            }
            Debug.Log(memberAddress + "\n" + memberType.Name);
        }
        else
        {

        }
    }
}
