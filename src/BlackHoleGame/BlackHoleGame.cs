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
        // Setup camera
        SetupCamera();

        // Create black hole controller
        blackHoleController = new BlackHoleController();
        blackHoleController.Initialize();

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
    /// Setup camera for 3D physics view
    /// </summary>
    private void SetupCamera()
    {
        var camera = GameUI.Device.DeviceInfo.PrimaryViewport.Camera;

        // Position: Behind and above (0, -30, 25) for top-down isometric view
        camera.SetPosition(
            new System.Numerics.Vector3(0, -800, 600),  // Perspective view from above
            TimeSpan.FromSeconds(0.1)
        );

        // Rotation: Looking down at angle
        camera.SetRotation(
            new GameCore.CameraSystem.Struct.CameraRotation(
                yaw: -90f,    // Face forward (north)
                pitch: -60f,  // Look down at angle
                roll: 0f
            ),
            TimeSpan.FromSeconds(0.1)
        );

        Game.Logger.LogInformation("📷 Camera setup complete");
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
#endif
    }
}
