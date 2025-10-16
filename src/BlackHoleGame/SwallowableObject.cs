#if CLIENT
using System;
using System.Numerics;
using GameCore;
using GameCorePhysics.Actor;
using EngineInterface.Urho3DInterface;

namespace GameEntry.BlackHoleGame;

/// <summary>
/// Component for objects that can be swallowed by the black hole
/// Handles swallow animation and destruction
/// </summary>
public class SwallowableObject : ScriptComponent
{
    // Object properties
    public float Size { get; private set; }
    public float GrowthValue { get; private set; }
    public PhysicsActor Actor { get; private set; }

    // Swallowing state
    private bool isBeingSwallowed = false;
    private float swallowProgress = 0f;
    private Vector3 swallowTargetPos;
    private Vector3 initialPos;
    private Vector3 initialScale;
    private const float SWALLOW_DURATION = 0.15f;

    private RigidBody rigidBody;

    public void Initialize(PhysicsActor actor, float size, float growthValue)
    {
        Actor = actor;
        Size = size;
        GrowthValue = growthValue;

        rigidBody = node.GetComponent<RigidBody>();
        initialScale = node.localScale;
    }

    public void Swallow(BlackHoleController blackHole)
    {
        if (isBeingSwallowed) return;

        isBeingSwallowed = true;
        swallowProgress = 0f;
        initialPos = node.position;
        swallowTargetPos = blackHole.Position;

        // Disable physics during swallow
        if (rigidBody != null)
        {
            rigidBody.SetUseGravity(false);
            rigidBody.SetLinearVelocity(Vector3.Zero);
        }

        // Apply growth to black hole immediately
        blackHole.Grow(GrowthValue);

        Game.Logger.LogInformation("🌀 Swallowing object of size {Size}, growth value {Growth}", Size, GrowthValue);
    }

    public override void OnUpdate(float timeStep)
    {
        if (!isBeingSwallowed) return;

        swallowProgress += timeStep / SWALLOW_DURATION;

        if (swallowProgress >= 1f)
        {
            // Swallow complete - destroy object
            CompleteSwallow();
        }
        else
        {
            // Animate swallow
            UpdateSwallowAnimation();
        }
    }

    private void UpdateSwallowAnimation()
    {
        // Ease-in quad for smooth animation
        float t = swallowProgress * swallowProgress;

        // Move toward black hole center
        node.position = Vector3.Lerp(initialPos, swallowTargetPos, t);

        // Scale down to zero
        node.localScale = Vector3.Lerp(initialScale, Vector3.Zero, t);

        // Note: Node.rotation property not available in WasiCore API
        // Visual rotation effect removed for Phase 1
    }

    private void CompleteSwallow()
    {
        // Mark for respawn
        if (Actor != null)
        {
            ObjectSpawner.MarkForRespawn(Actor);
        }
    }
}
#endif
