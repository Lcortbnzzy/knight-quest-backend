using System;
using System.Collections.Generic;
using FuzzySharp;
using Game.Utils;
using Godot;

namespace Game.Data;

#nullable enable
public abstract partial class Registry<T, TRegistry> : RefCounted
    where T : Resource
    where TRegistry : Registry<T, TRegistry>,
    new()
{
    protected static readonly Lazy<TRegistry> Instance = new(() => new TRegistry());

    protected abstract string ResourcePath { get; }

    public static readonly Dictionary<string, T> Resources = [];

    static Registry()
    {
        Instance.Value.LoadResources();
    }

    public static T? Get(string id)
    {
        Resources.TryGetValue(id, out var resource);

        if (resource != null) return (T)resource.Duplicate();

        var matches = Process.ExtractOne(id, [.. Resources.Keys]);

        if (matches == null || matches.Score < 80) return null;

        Resources.TryGetValue(matches.Value, out resource);

        return resource?.Duplicate() as T;
    }

    public static bool Get(string id, out T? resource)
    {
        resource = Get(id);

        return resource != null;
    }

    protected virtual void LoadResources()
    {
        var files = DirAccessUtils.GetFilesRecursively(Instance.Value.ResourcePath);

        foreach (var path in files)
        {
            if (!path.EndsWith(".tres") && !path.EndsWith(".tres.remap")) continue;

            var loaded = ResourceLoader.Load(path);
            if (loaded is not T resource || resource == null) continue;

            var fileName = path.GetFile();
            var id = fileName.GetBaseName();

            Resources[id] = resource;
        }
    }
}