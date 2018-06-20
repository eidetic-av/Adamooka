using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class OscAddressRegistrar : MonoBehaviour
{
    public List<Component> TargetComponents;
    
    void Start()
    {
        // For every component added to the public TargetComponents list
        TargetComponents.ForEach(targetComponent =>
        {
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
                    emissionProperties.ToList().ForEach(p => Debug.Log(p.Name));
                }

            });
        });
    }
}
