using Godot;
using System.Collections.Generic;

using Logger = Game.Utils.Logger;


namespace Game.Autoloads;
#nullable enable
/// <summary>
/// Manages scene navigation with back/forward history support.
/// Provides browser-like navigation functionality for game scenes.
/// </summary>
public partial class Navigator : Autoload<Navigator>
{
    private readonly Stack<string> backHistory = new();
    private readonly Stack<string> forwardHistory = new();

    /// <summary>
    /// The file path of the currently active scene.
    /// </summary>
    public string? CurrentScene { get; private set; }

    /// <summary>
    /// Indicates whether backward navigation is possible.
    /// </summary>
    public bool CanGoBack => backHistory.Count > 0;

    /// <summary>
    /// Indicates whether forward navigation is possible.
    /// </summary>
    public bool CanGoForward => forwardHistory.Count > 0;

    /// <summary>
    /// Event triggered when navigation occurs. Provides the new scene path.
    /// </summary>
    public static event System.Action<string>? SceneChanged;

    /// <summary>
    /// Event triggered when navigation history changes (affects back/forward availability).
    /// </summary>
    public static event System.Action? HistoryChanged;

    public override void _Ready()
    {
        // Wait for the scene tree to be ready before getting the current scene
        CallDeferred(nameof(InitializeCurrentScene));
        
        // Connect to tree changes to keep CurrentScene in sync
        GetTree().TreeChanged += OnTreeChanged;
    }

    private void InitializeCurrentScene()
    {
        CurrentScene = GetTree().CurrentScene?.SceneFilePath;
        Logger.Info($"Navigator: Initialized with current scene: {(CurrentScene ?? "none")}");
    }

    public override void _ExitTree()
    {
        GetTree().TreeChanged -= OnTreeChanged;
    }

    private void OnTreeChanged()
    {
        var newScene = GetTree().CurrentScene?.SceneFilePath;
        if (newScene != CurrentScene)
        {
            CurrentScene = newScene;
        }
    }

    // --- Static API ---

    /// <summary>
    /// Navigates to a new scene, adding the current scene to back history.
    /// </summary>
    /// <param name="scenePath">Path to the scene file to load</param>
    /// <returns>True if navigation was successful, false otherwise</returns>
    public static bool Push(string scenePath) => Instance.PushImpl(scenePath);

    /// <summary>
    /// Navigates back to the previous scene in history.
    /// </summary>
    /// <returns>True if backward navigation occurred, false if no history exists</returns>
    public static bool Back() => Instance.BackImpl();

    /// <summary>
    /// Navigates forward to the next scene in history.
    /// </summary>
    /// <returns>True if forward navigation occurred, false if no forward history exists</returns>
    public static bool Forward() => Instance.ForwardImpl();

    /// <summary>
    /// Gets the current scene path.
    /// </summary>
    /// <returns>Current scene file path, or null if no scene is loaded</returns>
    public static string? GetCurrentScene() => Instance.CurrentScene;

    /// <summary>
    /// Checks if backward navigation is available.
    /// </summary>
    public static bool GetCanGoBack() => Instance.CanGoBack;

    /// <summary>
    /// Checks if forward navigation is available.
    /// </summary>
    public static bool GetCanGoForward() => Instance.CanGoForward;

    /// <summary>
    /// Clears all navigation history.
    /// </summary>
    public static void ClearHistory() => Instance.ClearHistoryImpl();

    /// <summary>
    /// Gets a copy of the back history for debugging or UI purposes.
    /// </summary>
    /// <returns>Array of scene paths in back history (most recent first)</returns>
    public static string[] GetBackHistory() => Instance.GetBackHistoryImpl();

    /// <summary>
    /// Gets a copy of the forward history for debugging or UI purposes.
    /// </summary>
    /// <returns>Array of scene paths in forward history (next up first)</returns>
    public static string[] GetForwardHistory() => Instance.GetForwardHistoryImpl();

    /// <summary>
    /// Gets the previous scene path from the back history.
    /// </summary>
    /// <returns>The most recent scene in back history, or null if no history exists</returns>
    public static string? GetPreviousScene() => Instance.GetPreviousSceneImpl();

    // --- Internal instance logic ---

    private bool PushImpl(string scenePath)
    {
        if (string.IsNullOrWhiteSpace(scenePath))
        {
            Logger.Error("Navigator: Scene path cannot be null or empty");
            return false;
        }

        // Don't navigate to the same scene
        if (scenePath == CurrentScene)
        {
            Logger.Info($"Navigator: Already at scene {scenePath}");
            return false;
        }

        if (!ResourceLoader.Exists(scenePath))
        {
            Logger.Error($"Navigator: Scene not found: {scenePath}");
            return false;
        }

        // Add current scene to back history before navigating
        if (CurrentScene is not null)
        {
            backHistory.Push(CurrentScene);
        }

        // Clear forward history when pushing a new scene
        forwardHistory.Clear();

        return ChangeScene(scenePath);
    }

    private bool BackImpl()
    {
        if (backHistory.Count == 0)
        {
            Logger.Info("Navigator: No back history available");
            return false;
        }

        // Add current scene to forward history
        if (CurrentScene is not null)
        {
            forwardHistory.Push(CurrentScene);
        }

        var targetScene = backHistory.Pop();
        return ChangeScene(targetScene);
    }

    private bool ForwardImpl()
    {
        if (forwardHistory.Count == 0)
        {
            Logger.Info("Navigator: No forward history available");
            return false;
        }

        // Add current scene to back history
        if (CurrentScene is not null)
        {
            backHistory.Push(CurrentScene);
        }

        var targetScene = forwardHistory.Pop();
        return ChangeScene(targetScene);
    }

    private bool ChangeScene(string path)
    {
        var previousScene = CurrentScene ?? GetTree().CurrentScene?.SceneFilePath;
        var error = GetTree().ChangeSceneToFile(path);

        if (error == Error.Ok)
        {
            CurrentScene = path;

            // Trigger events
            SceneChanged?.Invoke(path);
            HistoryChanged?.Invoke();

            Logger.Info($"Navigator: Changed scene from '{previousScene ?? "none"}' to '{path}' (back history: {backHistory.Count}, forward history: {forwardHistory.Count})");
            return true;
        }

        Logger.Error($"Navigator: Failed to change scene to '{path}': {error}");
        return false;
    }

    private void ClearHistoryImpl()
    {
        backHistory.Clear();
        forwardHistory.Clear();
        HistoryChanged?.Invoke();
        Logger.Info("Navigator: History cleared");
    }

    private string[] GetBackHistoryImpl()
    {
        return backHistory.ToArray();
    }

    private string[] GetForwardHistoryImpl()
    {
        return forwardHistory.ToArray();
    }

    private string? GetPreviousSceneImpl()
    {
        return backHistory.Count > 0 ? backHistory.Peek() : null;
    }
}