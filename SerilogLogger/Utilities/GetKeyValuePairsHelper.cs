using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SerilogLogger.Utilities;

public static class GetKeyValuePairsHelper
{
    public static List<KeyValuePair<string, object>> GetKeyValuePairs(
        this object obj,
        string? baseName = null,
        bool prettyWriteValue = true
    )
    {
        baseName ??= obj.GetType().Name;

        return GetSerializablePublicMember(obj, baseName, prettyWriteValue);


    }


    private static List<KeyValuePair<string, object>> GetKeyValuePairsList(object obj, string baseName, bool prettyWriteValue = true)
    {
        var jsonString = JsonSerializer.Serialize(obj,
            new JsonSerializerOptions()
            { WriteIndented = prettyWriteValue, ReferenceHandler = ReferenceHandler.IgnoreCycles });

        var jsonDocument = JsonDocument.Parse(jsonString);

        switch (jsonDocument.RootElement.ValueKind)
        {
            case JsonValueKind.Array:
                return GetArrayObjectKeyValuePairs(baseName, jsonDocument);

            case JsonValueKind.Number:
                return new List<KeyValuePair<string, object>>()
                {
                    new ($"{baseName}", jsonDocument.RootElement.GetInt32())
                };

            case JsonValueKind.False:
            case JsonValueKind.True:
                return new List<KeyValuePair<string, object>>()
                {
                    new ($"{baseName}", jsonDocument.RootElement.GetBoolean())
                };

            case JsonValueKind.Null:
                return new List<KeyValuePair<string, object>>()
                {
                    new ($"{baseName}", "null")
                };

            case JsonValueKind.String:
                return new List<KeyValuePair<string, object>>()
                {
                    new ($"{baseName}", jsonDocument.RootElement.GetString())
                };
            
            case JsonValueKind.Object:
                return jsonDocument.RootElement
                    .EnumerateObject()
                    .Select(jsonProperty =>
                        new KeyValuePair<string,object>($"{baseName}.{jsonProperty.Name}", jsonProperty.Value)
                    )
                    .ToList();

            default:
                return new List<KeyValuePair<string, object>>();

        }
    }

    private static List<KeyValuePair<string, object>> GetSerializablePublicMember(object obj, string baseName, bool prettyWriteValue = true)
    {
        var bindingFlags = BindingFlags.Public | BindingFlags.Instance;

        //var memberTypes = (MemberTypes.Property | MemberTypes.Field);

        var members = obj.GetType().GetMembers(bindingFlags);

        var keyValuePairs = new List<KeyValuePair<string, object>>();


        foreach (var memberInfo in members)
        {
            try
            {
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Field:
                        var fieldInfo = (FieldInfo)memberInfo;
                        keyValuePairs.AddRange(GetKeyValuePairsList(fieldInfo.GetValue, $"{baseName}.{fieldInfo.Name}"));
                        break;

                    case MemberTypes.Property:
                        var propertyInfo = (PropertyInfo)memberInfo;
                       var propertyInfoObject= propertyInfo.GetValue(obj);
                        keyValuePairs.AddRange(GetKeyValuePairsList(propertyInfoObject, $"{baseName}.{propertyInfo.Name}"));
                        break;


                    /******************This is for using other type if needs.******************/
                  
                    //case MethodInfo:
                    //case EventInfo:
                    //case ConstructorInfo:
                    //    continue;
                }

            }
            catch (Exception exception)
            {
                // ignored exception occurred 
            }
        }

        return keyValuePairs;


    }

    private static List<KeyValuePair<string, object>> GetArrayObjectKeyValuePairs(string baseName, JsonDocument jsonDocument)
    {
        var items = jsonDocument.RootElement.EnumerateArray();

        var i = 0;

        List<KeyValuePair<string, object>> keyValuePairs = new List<KeyValuePair<string, object>>();

        while (items.MoveNext())
        {
            keyValuePairs.Add(new($"{baseName}[{i}]", items.Current));

            i++;
        }

        return keyValuePairs;
    }


    public static List<KeyValuePair<string, object>> UnionKeyValuePairs
    (
        this List<KeyValuePair<string, object>> firstKeyValuePairs,
        object obj,
        string? baseName = null,
        bool prettyWriteValue = true)
    {
        return firstKeyValuePairs.Union(obj.GetKeyValuePairs(baseName, prettyWriteValue)).ToList();
    }
}