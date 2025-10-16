#if CLIENT
using System;
using System.Collections.Generic;
using System.Numerics;
using GameCore;
using GameCore.PlayerAndUsers;
using GameCore.SceneSystem;
using GameCorePhysics.Actor;
using EngineInterface.Urho3DInterface;

namespace GameEntry.BlackHoleGame;

/// <summary>
/// Manages spawning of swallowable objects
/// Phase 1: Simple spawning of 10 objects (5 tiny, 5 small)
/// </summary>
public class ObjectSpawner
{
    // Spawn configuration
    private const int INITIAL_OBJECT_COUNT = 10;
    private const float SPAWN_AREA_SIZE = 1000f;  // Spawn within 1000 units of center

    // Object definitions (hardcoded for Phase 1)
    private static readonly ObjectDefinition[] OBJECT_TYPES = new[]
    {
        // Tiny objects
        new ObjectDefinition("traffic_cone", PrimitiveShape.Cone, 15f, 2f, new Vector3(1f, 1f, 1.5f)),
        new ObjectDefinition("small_box", PrimitiveShape.Cube, 18f, 3f, new Vector3(1f, 1f, 1f)),
        new ObjectDefinition("ball", PrimitiveShape.Sphere, 12f, 2f, new Vector3(0.8f, 0.8f, 0.8f)),
        new ObjectDefinition("bottle", PrimitiveShape.Cylinder, 20f, 3f, new Vector3(0.5f, 0.5f, 1.2f)),
        new ObjectDefinition("rock", PrimitiveShape.Sphere, 14f, 2.5f, new Vector3(0.9f, 0.9f, 0.9f)),

        // Small objects
        new ObjectDefinition("barrel", PrimitiveShape.Cylinder, 45f, 7f, new Vector3(1.2f, 1.2f, 1.5f)),
        new ObjectDefinition("crate", PrimitiveShape.Cube, 40f, 6f, new Vector3(1.5f, 1.5f, 1.5f)),
        new ObjectDefinition("bench", PrimitiveShape.Cube, 55f, 8f, new Vector3(2f, 0.5f, 0.8f)),
        new ObjectDefinition("trash_can", PrimitiveShape.Cylinder, 38f, 5.5f, new Vector3(0.8f, 0.8f, 1.8f)),
        new ObjectDefinition("sign_post", PrimitiveShape.Cone, 32f, 5f, new Vector3(0.5f, 0.5f, 2f)),
    };

    private BlackHoleController blackHoleController;
    private Scene scene;
    private List<PhysicsActor> activeObjects = new();
    private static List<PhysicsActor> objectsToRespawn = new();

    public ObjectSpawner(BlackHoleController controller)
    {
        blackHoleController = controller;
        scene = Scene.Get("blackhole_scene"u8);
    }

    /// <summary>
    /// Spawn initial set of objects
    /// </summary>
    public void SpawnInitialObjects()
    {
        Game.Logger.LogInformation("🌍 Spawning {Count} initial objects...", INITIAL_OBJECT_COUNT);

        // Create ground plane for objects to rest on
        CreateGroundPlane();

        // Spawn 5 tiny and 5 small objects
        for (int i = 0; i < INITIAL_OBJECT_COUNT; i++)
        {
            var objDef = OBJECT_TYPES[i % OBJECT_TYPES.Length];
            SpawnObject(objDef);
        }

        Game.Logger.LogInformation("✅ Spawned {Count} objects", activeObjects.Count);
    }

    private void CreateGroundPlane()
    {
        // Create static ground for physics objects
        var ground = new PhysicsActor(
            Player.LocalPlayer,
            PhysicsActor.GetPrimitiveLink(PrimitiveShape.Cube),
            scene,
            new Vector3(0, 0, -50),  // Below Z=0 ground level
            Vector3.Zero
        );

        var groundNode = ground.GetNode();
        groundNode.localScale = new Vector3(100, 100, 1);  // 10000x10000 area, 100 units thick

        var groundRB = groundNode.GetComponent<RigidBody>();
        if (groundRB != null)
        {
            // Make it static by using very high mass (no SetKinematic available)
            groundRB.SetMass(1000000f);  // Effectively immovable
            groundRB.SetUseGravity(false);
            groundRB.SetLinearDamping(999f);  // Prevent any movement
            groundRB.SetAngularDamping(999f);
            groundRB.SetCollisionLayer(4u);  // LAYER_ENVIRONMENT
            groundRB.SetCollisionMask(0xFFFFFFFF);
        }

        Game.Logger.LogInformation("🌍 Ground plane created: 10000x10000 at Z=-50");
    }

    private void SpawnObject(ObjectDefinition def)
    {
        // Find valid spawn position
        Vector3 position = FindValidSpawnPosition(def.Size);

        // Create physics actor
        var actor = new PhysicsActor(
            Player.LocalPlayer,
            PhysicsActor.GetPrimitiveLink(def.Shape),
            scene,
            position,
            Vector3.Zero
        );

        var node = actor.GetNode();
        node.localScale = def.Scale;

        // Setup physics
        var rb = node.GetComponent<RigidBody>();
        if (rb != null)
        {
            rb.SetUseGravity(true);
            rb.SetCollisionLayer(2u);  // LAYER_SWALLOWABLE
            rb.SetCollisionMask(2u | 4u);  // Collide with swallowable and environment

            // Mass based on size
            float mass = def.Size * def.Size * 0.1f;
            rb.SetMass(mass);

            // Add damping for stability
            rb.SetLinearDamping(0.1f);
            rb.SetAngularDamping(0.2f);

            // Collision filter: ignore black hole physical collision
            rb.SetCollisionFilter(new RigidBody.CollisionFilter((RigidBody other, Vector3 contact) =>
            {
                return other.GetCollisionLayer() == 1u;  // true = ignore black hole
            }));

            // Note: SetFriction() and SetRestitution() are not available in WasiCore API
            // Using default physics properties
        }

        // Add SwallowableObject component
        var swallowable = node.CreateComponent<SwallowableObject>();
        swallowable.Initialize(actor, def.Size, def.GrowthValue);

        activeObjects.Add(actor);
    }

    private Vector3 FindValidSpawnPosition(float objectSize)
    {
        Random random = new Random();

        for (int attempt = 0; attempt < 20; attempt++)
        {
            float x = (float)(random.NextDouble() * SPAWN_AREA_SIZE * 2 - SPAWN_AREA_SIZE);
            float y = (float)(random.NextDouble() * SPAWN_AREA_SIZE * 2 - SPAWN_AREA_SIZE);
            float z = objectSize / 2f;  // Start at half-height above ground

            Vector3 position = new Vector3(x, y, z);

            // Check minimum distance from black hole (at least 200 units away)
            float distanceFromBlackHole = Vector3.Distance(
                new Vector3(position.X, position.Y, 0),
                new Vector3(blackHoleController.Position.X, blackHoleController.Position.Y, 0)
            );

            if (distanceFromBlackHole > 200f)
            {
                return position;
            }
        }

        // Fallback: random position far from center
        return new Vector3(
            (float)(random.NextDouble() * 500 + 300),
            (float)(random.NextDouble() * 500 + 300),
            objectSize / 2f
        );
    }

    public void Update(float deltaTime)
    {
        // Phase 1: Simple respawn logic
        if (objectsToRespawn.Count > 0)
        {
            foreach (var actor in objectsToRespawn)
            {
                activeObjects.Remove(actor);
                PhysicsActor.DestroyImmediately(actor);
            }
            objectsToRespawn.Clear();

            // Respawn objects to maintain count
            while (activeObjects.Count < INITIAL_OBJECT_COUNT)
            {
                var objDef = OBJECT_TYPES[Random.Shared.Next(OBJECT_TYPES.Length)];
                SpawnObject(objDef);
            }
        }
    }

    public static void MarkForRespawn(PhysicsActor actor)
    {
        if (!objectsToRespawn.Contains(actor))
        {
            objectsToRespawn.Add(actor);
        }
    }
}

/// <summary>
/// Simple object definition for Phase 1
/// </summary>
public record ObjectDefinition(
    string Name,
    PrimitiveShape Shape,
    float Size,
    float GrowthValue,
    Vector3 Scale
);
#endif
