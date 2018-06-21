using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class OscRouter : MonoBehaviour
{
    public static OscRouter Instance;

    public string TestPropertyAddress = "";
    public float TestPropertyValue = 5.0f;
    public Boolean SetTestProperty = false;

    public Boolean PrintKeys = false;
    
    public List<object> RegisteredMembers = new List<object>();

    /// <summary>
    /// Dictionary containing the methods to invoke when attempting to set Component properties or fields.
    /// They key is the OSC Address as a string; the value is the setter method as an Action.
    /// </summary>
    Dictionary<string, Action<object>> MemberSetters = new Dictionary<string, Action<object>>();

    void Awake()
    {
        if (Instance != null)
            throw (new UnityException("Only use one OscRouter in a scene!"));
        Instance = this;
    }

    void Update()
    {
        if (SetTestProperty)
        {
            if (MemberSetters.ContainsKey(TestPropertyAddress))
            {
                MemberSetters[TestPropertyAddress].Invoke(TestPropertyValue);
            }
            SetTestProperty = false;
        }
        if (PrintKeys)
        {
            MemberSetters.Keys.ToList().ForEach(k => Debug.Log(k));
            PrintKeys = false;
        }
    }

    public bool RegisterMember(object targetObject, PropertyInfo property, string address)
    {
        if (RegisteredMembers.Contains(property)) return false;

        // create Property setter Action and register it with the OscRouter
        AddMemberSetter(address, property, (object value) =>
        {
            property.SetValue(targetObject, value);
        });
        return true;
    }

    public bool RegisterMember(object targetObject, FieldInfo field, string address)
    {
        if (RegisteredMembers.Contains(field)) return false;

        // create Property setter Action and register it with the OscRouter
        AddMemberSetter(address, field, (object value) =>
        {
            field.SetValue(targetObject, value);
        });
        return true;
    }

    public void AddMemberSetter(string address, MemberInfo memberInfo, Action<object> setterAction)
    {
        MemberSetters.Add(address, setterAction);
        RegisteredMembers.Add(memberInfo);
        Debug.Log("Registered setter Action to address:\n" + address);
    }
}
