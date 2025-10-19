#if CLIENT
using System;
using System.Numerics;
using GameCore;
using GameCore.PlayerAndUsers;
using GameCore.SceneSystem;
using GameCore.EntitySystem;
using GameCorePhysics.Actor;
using EngineInterface.Urho3DInterface;
using GameUI.Device;
using GameUI.TriggerEvent;

namespace GameEntry.BlackHoleGame;

/// <summary>
/// Controls the player's black hole unit and movement
/// Uses WasiCore Unit system for proper camera follow and player control
/// </summary>
public class BlackHoleController
{
    // Physics components
    private PhysicsActor blackHoleActor;
    private Node blackHoleNode;
    private RigidBody rigidBody;
    private BlackHoleComponent blackHoleComponent;

    // Current state
    public float CurrentSize { get; private set; } = 50f;  // Starting size
    public int CurrentLevel { get; private set; } = 1;     // Starting level
    public Vector3 Position => blackHoleNode?.position ?? Vector3.Zero;

    // Movement
    private const float BASE_SPEED = 400f;
    private Vector3 targetWorldPosition;

    public void Initialize()
    {
        Game.Logger.LogInformation("🕳️ Initializing black hole PhysicsActor (client-only, no Unit)...");

        // Get scene
        var scene = Scene.Get("blackhole_scene"u8);

        // Create PhysicsActor directly using PrimitiveShape (not Unit!)
        // Units cannot be created on client - use standalone PhysicsActor
        blackHoleActor = new PhysicsActor(
            Player.LocalPlayer,
            PhysicsActor.GetPrimitiveLink(PrimitiveShape.Sphere),  // Primitive, not UnitLink
            scene,
            new Vector3(0, 0, 25),  // Start above ground
            Vector3.Zero
        );

        // Get node and setup physics
        blackHoleNode = blackHoleActor.GetNode();
        blackHoleNode.localScale = new Vector3(3, 3, 3);  // Make it bigger for visibility

        rigidBody = blackHoleNode?.GetComponent<RigidBody>();

        if (rigidBody != null)
        {
            rigidBody.SetUseGravity(false);
            rigidBody.SetCollisionLayer(1u);  // Main unit default layer = 1
            rigidBody.SetCollisionMask(0xFFFFFFFF); // Main unit default mask = all

            // Make it pass through all objects using collision filter
            rigidBody.SetCollisionFilter(new RigidBody.CollisionFilter((RigidBody other, Vector3 contact) =>
            {
                return true; // Pass through all, but still trigger OnTriggerEnter
            }));
        }

        // Add custom component for trigger detection
        if (blackHoleNode != null)
        {
            blackHoleComponent = blackHoleNode.CreateComponent<BlackHoleComponent>();
            blackHoleComponent.SetController(this);
        }

        // Register for mouse input
        RegisterInputHandler();

        // Initialize target position
        targetWorldPosition = Position;

        Game.Logger.LogInformation("🕳️ Black hole PhysicsActor created at {Pos}, size: {Size}", Position, CurrentSize);
    }

    private void RegisterInputHandler()
    {
        // Track mouse movement for black hole control
        DeviceInfo.PrimaryInputManager.OnPointerButtonMove += UpdateTargetPosition;

        // Also handle clicks/touches
        DeviceInfo.PrimaryInputManager.OnPointerButtonDown += UpdateTargetPosition;
    }

    private void UpdateTargetPosition(GameUI.TriggerEvent.EventGamePointerButtonMove e)
    {
        if (!e.PointerPosition.HasValue) return;

        var viewport = DeviceInfo.PrimaryViewport;
        var pointerPos = e.PointerPosition.Value;

        // Convert screen position to world position using raycast
        var rayResult = viewport.RaycastTerrainOrWorldPanelXY(pointerPos);
        if (rayResult.IsHit)
        {
            targetWorldPosition = rayResult.Position;
            // Keep Z at 0 (ground level)
            targetWorldPosition = new Vector3(targetWorldPosition.X, targetWorldPosition.Y, 25);
        }
    }

    private void UpdateTargetPosition(GameUI.TriggerEvent.EventGamePointerButtonDown e)
    {
        if (!e.PointerPosition.HasValue) return;

        var viewport = DeviceInfo.PrimaryViewport;
        var pointerPos = e.PointerPosition.Value;

        // Convert screen position to world position using raycast
        var rayResult = viewport.RaycastTerrainOrWorldPanelXY(pointerPos);
        if (rayResult.IsHit)
        {
            targetWorldPosition = rayResult.Position;
            // Keep Z at 0 (ground level)
            targetWorldPosition = new Vector3(targetWorldPosition.X, targetWorldPosition.Y, 25);
        }
    }

    public void Update(float deltaTime)
    {
        if (blackHoleNode == null || rigidBody == null) return;

        // Calculate movement direction
        Vector3 currentPos = Position;
        Vector3 direction = targetWorldPosition - currentPos;
        float distance = direction.Length();

        // Only move if there's significant distance
        if (distance > 5f)
        {
            direction = Vector3.Normalize(direction);

            // Apply speed multiplier based on level
            float speedMultiplier = GetSpeedMultiplier();
            float speed = BASE_SPEED * speedMultiplier;

            // Set velocity to move toward target (physics-based)
            rigidBody.SetLinearVelocity(direction * speed);
        }
        else
        {
            // Stop when close to target
            rigidBody.SetLinearVelocity(Vector3.Zero);
        }
    }

    private float GetSpeedMultiplier()
    {
        return CurrentLevel switch
        {
            1 => 1.2f,
            2 => 1.0f,
            3 => 0.85f,
            4 => 0.7f,
            5 => 0.6f,
            6 => 0.5f,
            _ => 1.0f
        };
    }

    /// <summary>
    /// Grow the black hole by swallowing an object
    /// </summary>
    public void Grow(float growthValue)
    {
        CurrentSize += growthValue;
        UpdateVisualSize();
        CheckLevelUp();

        Game.Logger.LogInformation("🕳️ Black hole grew to size: {Size}", CurrentSize);
    }

    private void UpdateVisualSize()
    {
        if (blackHoleNode == null) return;

        // Scale based on current size (base size is 50, initial scale is 3)
        float scale = (CurrentSize / 50f) * 3f;
        blackHoleNode.localScale = new Vector3(scale, scale, scale);
    }

    private void CheckLevelUp()
    {
        int newLevel = CalculateLevel(CurrentSize);
        if (newLevel > CurrentLevel)
        {
            CurrentLevel = newLevel;
            Game.Logger.LogInformation("⬆️ Level up! Now level {Level}", CurrentLevel);
        }
    }

    private int CalculateLevel(float size)
    {
        if (size >= 1500) return 6;
        if (size >= 1000) return 5;
        if (size >= 600) return 4;
        if (size >= 300) return 3;
        if (size >= 100) return 2;
        return 1;
    }

    /// <summary>
    /// Check if an object can be swallowed
    /// Swallow threshold: black hole must be 85% or larger than object
    /// </summary>
    public bool CanSwallow(float objectSize)
    {
        return objectSize <= (CurrentSize * 0.85f);
    }
}

/// <summary>
/// Script component attached to black hole for trigger detection
/// </summary>
public class BlackHoleComponent : ScriptComponent
{
    private BlackHoleController controller;

    public BlackHoleComponent()
    {
    }

    public void SetController(BlackHoleController ctrl)
    {
        controller = ctrl;
    }

    public override void OnTriggerEnter(Node otherNode)
    {
        if (controller == null) return;

        // Get swallowable object component
        var swallowable = otherNode.GetComponent<SwallowableObject>();
        if (swallowable == null) return;

        // Check if we can swallow this object
        if (controller.CanSwallow(swallowable.Size))
        {
            swallowable.Swallow(controller);
        }
    }
}
#endif
