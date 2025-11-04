using System.Collections.ObjectModel;
using Godot;
using Godot.Collections;

namespace Game.Utils;

public partial class DeepLinkUrl(RefCounted variant) : RefCounted
{
    private const string SchemeProperty = "scheme";
    private const string UserProperty = "user";
    private const string PasswordProperty = "password";
    private const string HostProperty = "host";
    private const string PortProperty = "port";
    private const string PathProperty = "path";
    private const string PathExtensionProperty = "pathExtension";
    private const string PathComponentsProperty = "pathComponents";
    private const string ParameterStringProperty = "parameterString";
    private const string QueryProperty = "query";
    private const string FragmentProperty = "fragment";

    public ReadOnlyCollection<Dictionary> Data => new([GetData()]);

    public Dictionary GetData()
    {
        return variant.Call("get_data").AsGodotDictionary();
    }

    public string GetScheme()
    {
        return GetData()[SchemeProperty].AsString();
    }

    public string GetUser()
    {
        return GetData()[UserProperty].AsString();
    }

    public string GetPassword()
    {
        return GetData()[PasswordProperty].AsString();
    }

    public string GetHost()
    {
        return GetData()[HostProperty].AsString();
    }

    public int GetPort()
    {
        return GetData()[PortProperty].AsInt32();
    }

    public string GetPath()
    {
        return GetData()[PathProperty].AsString();
    }

    public string GetPathExtension()
    {
        return GetData()[PathExtensionProperty].AsString();
    }

    public Array GetPathComponents()
    {
        return GetData()[PathComponentsProperty].AsGodotArray();
    }

    public string GetParameterString()
    {
        return GetData()[ParameterStringProperty].AsString();
    }

    public string GetQuery()
    {
        return GetData()[QueryProperty].AsString();
    }

    public string GetFragment()
    {
        return GetData()[FragmentProperty].AsString();
    }
}