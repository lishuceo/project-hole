using GameCore;
using GameCore.PlayerAndUsers;
using static GameCore.ScopeData;
#if CLIENT
using GameUI.TriggerEvent;
#endif

namespace GameEntry.BlackHoleGame;

/// <summary>
/// Main Black Hole Game Class - Phase 1 Core Prototype
///
/// Features:
/// - Black hole mouse control
/// - Basic object spawning (10 objects)
/// - Trigger detection and swallowing
/// - Simple growth system (size increase only)
///
/// Success Criteria: Black hole moves smoothly and can swallow objects
/// </summary>
public class BlackHoleGame : IGameClass, IThinker
{
#if CLIENT
    // System references
    private BlackHoleController blackHoleController;
    private ObjectSpawner objectSpawner;
#endif

    // Game state
    private bool isGameActive = false;

    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += RegisterAll;
    }

    public static void RegisterAll()
    {
        // Only register for BlackHoleGame mode
        if (Game.GameModeLink != GameEntry.ScopeData.GameMode.BlackHoleGame) return;

#if CLIENT
        Trigger<EventGameStart> trigger = new(async (s, d) =>
        {
            var game = new BlackHoleGame();
            game.Initialize();
            return true;
        });
        trigger.Register(Game.Instance);

        Game.Logger.LogInformation("🕳️ Black Hole Game registered");
#else
        Game.Logger.LogInformation("🕳️ Black Hole Game (server mode - no action needed)");
#endif
    }

    public void Initialize()
    {
        Game.Logger.LogInformation("🕳️ Initializing Black Hole Game...");

#if CLIENT
        // Create black hole controller first (creates the unit)
        blackHoleController = new BlackHoleController();
        blackHoleController.Initialize();

        // Setup camera to follow the black hole unit
        SetupCameraFollowUnit();

        // Create object spawner
        objectSpawner = new ObjectSpawner(blackHoleController);
        objectSpawner.SpawnInitialObjects();
#endif

        // Register as thinker for game loop
        Game.RegisterThinker(this);
        isGameActive = true;

        Game.Logger.LogInformation("🕳️ Black Hole Game initialized successfully!");
    }

#if CLIENT
    /// <summary>
    /// Setup camera for static top-down view
    /// Since we can't create Unit on client, camera won't auto-follow
    /// Will manually update camera in Think() loop to follow black hole
    /// </summary>
    private void SetupCameraFollowUnit()
    {
        var camera = GameUI.Device.DeviceInfo.PrimaryViewport.Camera;

        // Initial camera position (will update in Think loop to follow black hole)
        camera.SetPosition(
            new System.Numerics.Vector3(0, -150, 120),  // Initial position above origin
            TimeSpan.FromSeconds(0.1)
        );

        // Set camera rotation for isometric top-down view
        camera.SetRotation(
            new GameCore.CameraSystem.Struct.CameraRotation(
                yaw: 0f,      // Face north
                pitch: -50f,  // Look down at angle (50 degrees)
                roll: 0f
            ),
            TimeSpan.FromSeconds(0.1)
        );

        Game.Logger.LogInformation("📷 Camera setup complete at initial position (0, -150, 120)");
    }
#endif

    /// <summary>
    /// Game loop update (IThinker interface)
    /// deltaTime is in MILLISECONDS - must convert to seconds
    /// </summary>
    public void Think(int deltaTime)
    {
        if (!isGameActive) return;

        // Convert milliseconds to seconds
        float deltaSeconds = deltaTime / 1000f;

#if CLIENT
        // Update black hole (movement and growth)
        blackHoleController?.Update(deltaSeconds);

        // Update object spawner (respawn logic)
        objectSpawner?.Update(deltaSeconds);

        // Update camera to follow black hole (manual follow since no Unit.FollowTarget)
        UpdateCameraFollow();
#endif
    }

#if CLIENT
    /// <summary>
    /// Manually update camera to follow black hole position
    /// </summary>
    private void UpdateCameraFollow()
    {
        if (blackHoleController == null) return;

        var camera = GameUI.Device.DeviceInfo.PrimaryViewport.Camera;
        var blackHolePos = blackHoleController.Position;

        // Camera follows black hole with fixed offset
        var cameraOffset = new System.Numerics.Vector3(0, -150, 120);
        camera.SetPosition(
            blackHolePos + cameraOffset,
            TimeSpan.FromSeconds(0.1)
        );
    }
#endif
}
