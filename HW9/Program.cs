using System;
using System.Collections.Generic;
using System.Reflection;

[AttributeUsage(AttributeTargets.Field)]
public class CustomNameAttribute : Attribute
{
    public string CustomFieldName { get; set; }

    public CustomNameAttribute(string fieldName)
    {
        CustomFieldName = fieldName;
    }
}

public class CustomSerialization
{
    public static string ObjectToString<T>(T obj)
    {
        Type type = typeof(T);
        var fields = type.GetFields();

        List<string> fieldStrings = new List<string>();

        foreach (var field in fields)
        {
            string fieldName = field.Name;
            var customNameAttribute = (CustomNameAttribute)Attribute.GetCustomAttribute(field, typeof(CustomNameAttribute));
            if (customNameAttribute != null)
            {
                fieldName = customNameAttribute.CustomFieldName;
            }

            object value = field.GetValue(obj);
            fieldStrings.Add($"{fieldName}:{value}");
        }

        return string.Join(", ", fieldStrings);
    }

    public static void StringToObject<T>(T obj, string serializedData)
    {
        Type type = typeof(T);
        var fields = type.GetFields();

        string[] fieldData = serializedData.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var data in fieldData)
        {
            string[] parts = data.Split(':');
            string propertyName = parts[0];
            string propertyValue = parts[1];

            foreach (var field in fields)
            {
                string fieldName = field.Name;
                var customNameAttribute = (CustomNameAttribute)Attribute.GetCustomAttribute(field, typeof(CustomNameAttribute));
                if (customNameAttribute != null && customNameAttribute.CustomFieldName == propertyName)
                {
                    Type fieldType = field.FieldType;
                    object value;

                    if (fieldType == typeof(int))
                    {
                        value = int.Parse(propertyValue);
                    }
                    else if (fieldType == typeof(string))
                    {
                        value = propertyValue;
                    }
                    else
                    {
                        value = null;
                    }

                    field.SetValue(obj, value);
                }
            }
        }
    }
}

public class MyClass
{
    [CustomName("CustomFieldName")]
    public int I = 0;
    public string Name = "";
}

class Program
{
    static void Main()
    {
        MyClass myObj = new MyClass();
        myObj.I = 42;
        myObj.Name = "John";

        string serializedData = CustomSerialization.ObjectToString(myObj);
        Console.WriteLine(serializedData);

        MyClass newObj = new MyClass();
        CustomSerialization.StringToObject(newObj, serializedData);
        Console.WriteLine($"New object values: I={newObj.I}, Name={newObj.Name}");
    }
}
