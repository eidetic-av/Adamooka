using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class OscAddressRegistrar : MonoBehaviour
{
    public List<Component> TargetComponents;

    string RootAddress { get; set; }
    List<string> ComponentAddresses { get; set; } = new List<string>();

    void Start()
    {
        // Set the root address ("BaseAddress/.../...")
        // to the name of the GameObject this script is attached to (without spaces)
        RootAddress = gameObject.name.Replace(" ", "");

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

            // Find the Type, and get all the publicly accesible properties of the Type
            Type targetComponentType = targetComponent.GetType();
            PropertyInfo[] componentProperties = targetComponentType.GetProperties();

            componentProperties.ToList().ForEach(property =>
            {
                var isComponent = property.PropertyType.IsSubclassOf(typeof(Component));
                var isClass = property.PropertyType.IsClass;
                var isStruct = (property.PropertyType.IsValueType && !property.PropertyType.IsPrimitive && !property.PropertyType.IsEnum);

                if (property.Name == "emission")
                {
                    var emissionProperties = property.PropertyType.GetProperties();
                    //emissionProperties.ToList().ForEach(p => Debug.Log(p.Name));
                }

            });
        });
    }
}
