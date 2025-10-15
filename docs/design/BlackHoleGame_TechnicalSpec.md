# Black Hole Game - Technical Implementation Specification

**Project:** Black Hole Swallower
**Engine:** WasiCore/SCE
**Document Type:** Technical Specification
**Version:** 1.0
**Date:** 2025-10-15

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Class Structure](#2-class-structure)
3. [Data Structures](#3-data-structures)
4. [Implementation Patterns](#4-implementation-patterns)
5. [Physics Implementation](#5-physics-implementation)
6. [Performance Optimization](#6-performance-optimization)
7. [Code Templates](#7-code-templates)

---

## 1. Architecture Overview

### 1.1 System Diagram

```
BlackHoleGame (Main Game Class)
├── GameState (State Management)
├── BlackHoleController (Player Control)
│   ├── InputHandler (Mouse/Touch)
│   ├── MovementSystem (Physics Movement)
│   └── GrowthManager (Size Progression)
├── ObjectManager (Object Lifecycle)
│   ├── ObjectPool (Pooling System)
│   ├── SpawnManager (Spawning Logic)
│   └── ObjectDatabase (Data Configuration)
├── PhysicsManager (Physics Simulation)
│   ├── CollisionHandler (Trigger Detection)
│   ├── AttractionSystem (Force Application)
│   └── SwallowAnimator (Consumption Animation)
├── ProgressionManager (Game Progress)
│   ├── LevelSystem (Level Tracking)
│   ├── ScoreManager (Score Calculation)
│   └── ComboTracker (Combo System)
├── UIManager (User Interface)
│   ├── HUDController (In-Game UI)
│   ├── MenuController (Start/End Screens)
│   └── FeedbackSystem (Visual Feedback)
└── CameraController (Camera Follow)
```

### 1.2 Component Hierarchy

```
Scene: BlackHoleScene
├── Ground (Static Environment)
├── BlackHole (Player)
│   ├── PhysicsActor
│   ├── BlackHoleComponent (Script)
│   ├── RigidBody (Trigger)
│   └── VisualEffect (Glow)
├── Objects Container
│   ├── Object_001 (Cone)
│   │   ├── PhysicsActor
│   │   ├── SwallowableComponent (Script)
│   │   └── RigidBody (Solid)
│   ├── Object_002 (Cube)
│   └── ... (up to 200)
└── Camera (Follow Camera)
```

### 1.3 Data Flow

```
Input → InputHandler → BlackHoleController → RigidBody.SetLinearVelocity()
                                            ↓
                                    CameraController.Update()

Physics Step → CollisionHandler → OnTriggerEnter()
                                       ↓
                            BlackHoleComponent.CheckSwallowable()
                                       ↓
                              SwallowAnimator.StartSwallow()
                                       ↓
                              GrowthManager.ApplyGrowth()
                                       ↓
                              ScoreManager.AddScore()
                                       ↓
                              UIManager.UpdateDisplay()
```

---

## 2. Class Structure

### 2.1 Main Game Class

```csharp
#if CLIENT
using System;
using System.Collections.Generic;
using System.Numerics;
using GameCorePhysics.Actor;
using EngineInterface.Urho3DInterface;

public class BlackHoleGame : IGameClass
{
    // Core Systems
    private GameCore.SceneSystem.Scene scene;
    private BlackHoleController blackHoleController;
    private ObjectManager objectManager;
    private PhysicsManager physicsManager;
    private ProgressionManager progressionManager;
    private UIManager uiManager;
    private CameraController cameraController;

    // Game State
    private GameState currentState;
    private float gameTimer;
    private const float GAME_DURATION = 120f;

    // Singleton
    private static BlackHoleGame instance;
    public static BlackHoleGame Instance => instance;

    public BlackHoleGame()
    {
        instance = this;
        Initialize();
    }

    private void Initialize()
    {
        // Get scene
        scene = GameCore.SceneSystem.Scene.Get(
            GameEntry.PhysicsGameData.Scene.BlackHoleScene
        );

        // Initialize systems
        objectManager = new ObjectManager(scene);
        physicsManager = new PhysicsManager();
        progressionManager = new ProgressionManager();
        uiManager = new UIManager();
        cameraController = new CameraController();

        // Create black hole
        blackHoleController = new BlackHoleController(scene);

        // Setup UI
        uiManager.CreateHUD();
        uiManager.ShowStartScreen(OnGameStart);

        currentState = GameState.Menu;
    }

    public void OnGameStart()
    {
        currentState = GameState.Playing;
        gameTimer = GAME_DURATION;

        // Reset systems
        blackHoleController.Reset();
        objectManager.SpawnInitialObjects(blackHoleController.CurrentLevel);
        progressionManager.Reset();
        uiManager.ShowHUD();

        // Start game loop
        Game.RegisterThinker(this);
    }

    // IThinker implementation
    public void Think(float deltaTime)
    {
        if (currentState != GameState.Playing) return;

        // Update timer
        gameTimer -= deltaTime;
        uiManager.UpdateTimer(gameTimer);

        if (gameTimer <= 0f)
        {
            OnGameEnd();
            return;
        }

        // Update systems
        blackHoleController.Update(deltaTime);
        objectManager.Update(deltaTime, blackHoleController.CurrentLevel);
        physicsManager.Update(deltaTime);
        cameraController.Follow(blackHoleController.Position, deltaTime);
        uiManager.Update(deltaTime);
    }

    private void OnGameEnd()
    {
        currentState = GameState.GameOver;
        Game.UnregisterThinker(this);

        // Calculate final score
        int finalScore = progressionManager.CalculateFinalScore(gameTimer);
        uiManager.ShowEndScreen(finalScore, progressionManager.GetStats());
    }

    // Static initialization
    public static void OnRegisterGameClass()
    {
        Game.OnGameDataInitialization += () =>
        {
            new BlackHoleGame();
        };
    }
}

public enum GameState
{
    Menu,
    Playing,
    Paused,
    GameOver
}
#endif
```

### 2.2 BlackHoleController

```csharp
public class BlackHoleController
{
    private PhysicsActor blackHoleActor;
    private BlackHoleComponent blackHoleComponent;
    private RigidBody rigidBody;
    private Node node;

    // Properties
    public float CurrentSize { get; private set; }
    public int CurrentLevel { get; private set; }
    public Vector3 Position => node.position;

    // Constants
    private const float BASE_SIZE = 50f;
    private const float BASE_SPEED = 400f;
    private const float ACCELERATION = 8f;

    public BlackHoleController(GameCore.SceneSystem.Scene scene)
    {
        // Create black hole as main unit
        var mainUnit = Player.LocalPlayer.MainUnit;
        blackHoleActor = new PhysicsActor(mainUnit);

        node = blackHoleActor.GetNode();
        rigidBody = node.GetComponent<RigidBody>();

        // Configure physics
        rigidBody.SetUseGravity(false);
        rigidBody.SetCollisionLayer(1u);
        rigidBody.SetCollisionMask(0xFFFFFFFF);

        // Ignore physical collisions (trigger only)
        rigidBody.SetCollisionFilter((RigidBody other, Vector3 contact) => true);

        // Add component
        blackHoleComponent = new BlackHoleComponent(this);
        node.AddComponent<BlackHoleComponent>(blackHoleComponent);

        // Set initial position
        node.position = new Vector3(0, 0, 100);

        Reset();
    }

    public void Reset()
    {
        CurrentSize = BASE_SIZE;
        CurrentLevel = 1;
        UpdateVisualSize();
    }

    public void Update(float deltaTime)
    {
        // Get input
        Vector3 targetPosition = GetInputPosition();

        // Calculate movement
        Vector3 currentPos = node.position;
        Vector3 direction = Vector3.Normalize(targetPosition - currentPos);

        float speedMultiplier = GetSpeedMultiplier();
        float speed = BASE_SPEED * speedMultiplier;

        // Apply velocity
        rigidBody.SetLinearVelocity(direction * speed);
    }

    private Vector3 GetInputPosition()
    {
        // Get mouse position in world space
        // Simplified - actual implementation needs ray casting
        var mousePos = Input.GetMousePosition();
        return new Vector3(mousePos.X, mousePos.Y, 100);
    }

    public void ApplyGrowth(float growthValue)
    {
        CurrentSize += growthValue;
        CheckLevelUp();
        UpdateVisualSize();
    }

    private void CheckLevelUp()
    {
        int newLevel = CalculateLevel(CurrentSize);
        if (newLevel > CurrentLevel)
        {
            CurrentLevel = newLevel;
            OnLevelUp();
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

    private void OnLevelUp()
    {
        // Visual effect
        BlackHoleGame.Instance.GetComponent<UIManager>()
            .ShowLevelUpEffect(CurrentLevel);

        // Update camera zoom
        BlackHoleGame.Instance.GetComponent<CameraController>()
            .UpdateZoom(CurrentLevel);
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

    private void UpdateVisualSize()
    {
        float visualRadius = 50f + (CurrentLevel * 15f);
        float scale = visualRadius / 50f;
        node.localScale = new Vector3(scale, scale, scale);

        // Update trigger collider size
        blackHoleComponent.UpdateTriggerRadius(visualRadius * 1.1f);
    }

    public bool CanSwallow(SwallowableObject obj)
    {
        return obj.Size <= (CurrentSize * 0.85f);
    }
}
```

### 2.3 BlackHoleComponent

```csharp
public class BlackHoleComponent : ScriptComponent
{
    private BlackHoleController controller;
    private float attractionRadius;
    private float triggerRadius;
    private const float ATTRACTION_STRENGTH = 500f;

    public BlackHoleComponent(BlackHoleController controller)
    {
        this.controller = controller;
    }

    public override void OnStart()
    {
        UpdateTriggerRadius(50f * 1.1f);
    }

    public void UpdateTriggerRadius(float radius)
    {
        triggerRadius = radius;
        attractionRadius = radius * 2.0f;

        // Update collider size (implementation depends on engine)
        // CollisionShape.SetRadius(triggerRadius);
    }

    public override void OnFixedUpdate(float timeStep)
    {
        ApplyAttractionForces();
    }

    private void ApplyAttractionForces()
    {
        // Get all swallowable objects in range
        var objects = BlackHoleGame.Instance.GetComponent<ObjectManager>()
            .GetActiveObjects();

        Vector3 blackHolePos = node.position;

        foreach (var obj in objects)
        {
            if (obj.IsBeingSwallowed) continue;

            float distance = Vector3.Distance(obj.Position, blackHolePos);

            if (distance < attractionRadius && controller.CanSwallow(obj))
            {
                // Calculate attraction force
                Vector3 direction = Vector3.Normalize(blackHolePos - obj.Position);
                float t = 1.0f - (distance / attractionRadius);
                float forceMagnitude = ATTRACTION_STRENGTH * (t * t);

                // Apply force
                RigidBody rb = obj.GetRigidBody();
                rb.ApplyForce(direction * forceMagnitude);
            }
        }
    }

    public override void OnTriggerEnter(Node otherNode)
    {
        // Get object component
        var objComponent = otherNode.GetComponent<SwallowableComponent>();
        if (objComponent == null) return;

        SwallowableObject obj = objComponent.ObjectData;

        // Check if can swallow
        if (controller.CanSwallow(obj))
        {
            StartSwallow(objComponent);
        }
    }

    private void StartSwallow(SwallowableComponent objComponent)
    {
        // Start swallow animation
        objComponent.StartSwallow(node.position);

        // Register callback for completion
        objComponent.OnSwallowComplete += () =>
        {
            OnObjectSwallowed(objComponent.ObjectData);
        };
    }

    private void OnObjectSwallowed(SwallowableObject obj)
    {
        // Apply growth
        float growthRate = GetGrowthRate(controller.CurrentLevel);
        controller.ApplyGrowth(obj.GrowthValue * growthRate);

        // Update score
        BlackHoleGame.Instance.GetComponent<ProgressionManager>()
            .OnObjectSwallowed(obj);

        // Visual feedback
        BlackHoleGame.Instance.GetComponent<UIManager>()
            .ShowSwallowFeedback(obj);
    }

    private float GetGrowthRate(int level)
    {
        return level switch
        {
            1 => 1.0f,
            2 => 1.0f,
            3 => 0.7f,
            4 => 0.7f,
            5 => 0.5f,
            6 => 0.5f,
            _ => 1.0f
        };
    }
}
```

### 2.4 SwallowableComponent

```csharp
public class SwallowableComponent : ScriptComponent
{
    public SwallowableObject ObjectData { get; private set; }
    public bool IsBeingSwallowed { get; private set; }

    private Vector3 swallowTargetPos;
    private float swallowProgress;
    private Vector3 initialScale;
    private Vector3 initialPos;
    private const float SWALLOW_DURATION = 0.15f;

    public event Action OnSwallowComplete;

    public void Initialize(SwallowableObject data)
    {
        ObjectData = data;
        IsBeingSwallowed = false;
    }

    public override void OnStart()
    {
        initialScale = node.localScale;
    }

    public void StartSwallow(Vector3 targetPos)
    {
        IsBeingSwallowed = true;
        swallowTargetPos = targetPos;
        swallowProgress = 0f;
        initialPos = node.position;

        // Disable physics
        RigidBody rb = node.GetComponent<RigidBody>();
        rb.SetUseGravity(false);
        rb.SetLinearVelocity(Vector3.Zero);
    }

    public override void OnUpdate(float timeStep)
    {
        if (!IsBeingSwallowed) return;

        swallowProgress += timeStep / SWALLOW_DURATION;

        if (swallowProgress >= 1f)
        {
            CompleteSwallow();
        }
        else
        {
            UpdateSwallowAnimation();
        }
    }

    private void UpdateSwallowAnimation()
    {
        // Ease-in quad
        float t = swallowProgress * swallowProgress;

        // Lerp position
        node.position = Vector3.Lerp(initialPos, swallowTargetPos, t);

        // Scale down
        node.localScale = Vector3.Lerp(initialScale, Vector3.Zero, t);

        // Rotate
        float rotationAngle = swallowProgress * 720f; // 2 full rotations
        node.rotation = Quaternion.CreateFromAxisAngle(
            Vector3.UnitZ,
            rotationAngle * (MathF.PI / 180f)
        );
    }

    private void CompleteSwallow()
    {
        OnSwallowComplete?.Invoke();

        // Return to pool
        BlackHoleGame.Instance.GetComponent<ObjectManager>()
            .ReturnObjectToPool(this);
    }

    public RigidBody GetRigidBody()
    {
        return node.GetComponent<RigidBody>();
    }
}
```

---

## 3. Data Structures

### 3.1 SwallowableObject Data Class

```csharp
public class SwallowableObject
{
    public string ObjectID { get; set; }
    public string DisplayName { get; set; }
    public ObjectCategory Category { get; set; }
    public PrimitiveShape Shape { get; set; }
    public float Size { get; set; }
    public int Score { get; set; }
    public float GrowthValue { get; set; }
    public int MinBlackHoleLevel { get; set; }
    public float SpawnWeight { get; set; }
    public Color TintColor { get; set; }
    public Vector3 ScaleMultiplier { get; set; }

    // Runtime data
    public Vector3 Position { get; set; }
    public PhysicsActor Actor { get; set; }
    public SwallowableComponent Component { get; set; }
}

public enum ObjectCategory
{
    Tiny = 0,
    Small = 1,
    Medium = 2,
    Large = 3,
    Huge = 4,
    Massive = 5
}
```

### 3.2 Object Database

```csharp
public class ObjectDatabase
{
    private Dictionary<string, SwallowableObject> objectsById;
    private Dictionary<ObjectCategory, List<SwallowableObject>> objectsByCategory;
    private Dictionary<int, SpawnDistribution> spawnDistributionsByLevel;

    public void LoadFromJson(string jsonPath)
    {
        // Load JSON data
        string json = File.ReadAllText(jsonPath);
        var data = JsonSerializer.Deserialize<ObjectDatabaseJson>(json);

        // Initialize dictionaries
        objectsById = new Dictionary<string, SwallowableObject>();
        objectsByCategory = new Dictionary<ObjectCategory, List<SwallowableObject>>();

        foreach (ObjectCategory cat in Enum.GetValues<ObjectCategory>())
        {
            objectsByCategory[cat] = new List<SwallowableObject>();
        }

        // Populate data
        foreach (var objData in data.Objects)
        {
            var obj = ConvertToSwallowableObject(objData);
            objectsById[obj.ObjectID] = obj;
            objectsByCategory[obj.Category].Add(obj);
        }

        // Load spawn distributions
        spawnDistributionsByLevel = data.SpawnDistributions;
    }

    public SwallowableObject GetRandomObjectForLevel(int level)
    {
        var distribution = spawnDistributionsByLevel[level];
        ObjectCategory category = SelectCategoryByWeight(distribution);

        var categoryObjects = objectsByCategory[category];
        return SelectObjectByWeight(categoryObjects);
    }

    private ObjectCategory SelectCategoryByWeight(SpawnDistribution dist)
    {
        float total = dist.GetTotalWeight();
        float random = Random.Range(0f, total);
        float cumulative = 0f;

        foreach (var kvp in dist.Weights)
        {
            cumulative += kvp.Value;
            if (random <= cumulative)
                return kvp.Key;
        }

        return ObjectCategory.Tiny;
    }

    private SwallowableObject SelectObjectByWeight(List<SwallowableObject> objects)
    {
        float totalWeight = objects.Sum(o => o.SpawnWeight);
        float random = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var obj in objects)
        {
            cumulative += obj.SpawnWeight;
            if (random <= cumulative)
                return obj;
        }

        return objects[0];
    }
}

public class SpawnDistribution
{
    public Dictionary<ObjectCategory, float> Weights { get; set; }

    public float GetTotalWeight()
    {
        return Weights.Values.Sum();
    }
}
```

### 3.3 Game Stats

```csharp
public class GameStats
{
    public int TotalScore { get; set; }
    public int ObjectsEaten { get; set; }
    public int MaxCombo { get; set; }
    public int MaxLevel { get; set; }
    public float TimePlayed { get; set; }

    public Dictionary<ObjectCategory, int> ObjectsByCategory { get; set; }

    public void Reset()
    {
        TotalScore = 0;
        ObjectsEaten = 0;
        MaxCombo = 0;
        MaxLevel = 1;
        TimePlayed = 0f;
        ObjectsByCategory = new Dictionary<ObjectCategory, int>();
    }
}
```

---

## 4. Implementation Patterns

### 4.1 Object Pool Pattern

```csharp
public class ObjectPool
{
    private Dictionary<ObjectCategory, Queue<PhysicsActor>> pools;
    private Dictionary<PhysicsActor, SwallowableObject> actorToData;
    private GameCore.SceneSystem.Scene scene;

    private const int INITIAL_POOL_SIZE = 30;

    public ObjectPool(GameCore.SceneSystem.Scene scene)
    {
        this.scene = scene;
        pools = new Dictionary<ObjectCategory, Queue<PhysicsActor>>();
        actorToData = new Dictionary<PhysicsActor, SwallowableObject>();

        InitializePools();
    }

    private void InitializePools()
    {
        foreach (ObjectCategory category in Enum.GetValues<ObjectCategory>())
        {
            pools[category] = new Queue<PhysicsActor>();
            PrewarmPool(category, INITIAL_POOL_SIZE);
        }
    }

    private void PrewarmPool(ObjectCategory category, int count)
    {
        for (int i = 0; i < count; i++)
        {
            PhysicsActor actor = CreateNewActor(category);
            actor.GetNode().SetEnabled(false);
            pools[category].Enqueue(actor);
        }
    }

    private PhysicsActor CreateNewActor(ObjectCategory category)
    {
        // Create using default data for category
        var tempData = GetDefaultDataForCategory(category);

        PhysicsActor actor = new PhysicsActor(
            Player.LocalPlayer,
            PhysicsActor.GetPrimitiveLink(tempData.Shape),
            scene,
            Vector3.Zero,
            Vector3.Zero
        );

        // Add component
        Node node = actor.GetNode();
        SwallowableComponent component = new SwallowableComponent();
        node.AddComponent<SwallowableComponent>(component);

        // Configure physics
        RigidBody rb = node.GetComponent<RigidBody>();
        rb.SetCollisionLayer(2u);
        rb.SetCollisionMask(0xFFFFFFFF);
        rb.SetUseGravity(true);

        return actor;
    }

    public PhysicsActor GetObject(SwallowableObject data)
    {
        Queue<PhysicsActor> pool = pools[data.Category];

        PhysicsActor actor;
        if (pool.Count > 0)
        {
            actor = pool.Dequeue();
        }
        else
        {
            actor = CreateNewActor(data.Category);
        }

        ConfigureActor(actor, data);
        actorToData[actor] = data;

        return actor;
    }

    private void ConfigureActor(PhysicsActor actor, SwallowableObject data)
    {
        Node node = actor.GetNode();

        // Set scale
        float scale = data.Size / 50f; // Normalize to base size
        node.localScale = data.ScaleMultiplier * scale;

        // Set color
        if (actor is IActorColorizable colorizable)
        {
            colorizable.InitializeTintColorAggregators();
            colorizable.SetTintColor(
                new HdrColor(data.TintColor),
                GameCore.ActorSystem.Enum.TintColorType.Override,
                "object_color"
            );
        }

        // Initialize component
        SwallowableComponent component = node.GetComponent<SwallowableComponent>();
        component.Initialize(data);

        data.Actor = actor;
        data.Component = component;

        node.SetEnabled(true);
    }

    public void ReturnObject(PhysicsActor actor)
    {
        if (!actorToData.TryGetValue(actor, out SwallowableObject data))
            return;

        Node node = actor.GetNode();
        node.SetEnabled(false);
        node.position = Vector3.Zero;
        node.localScale = Vector3.One;

        // Reset physics
        RigidBody rb = node.GetComponent<RigidBody>();
        rb.SetLinearVelocity(Vector3.Zero);
        rb.SetUseGravity(true);

        pools[data.Category].Enqueue(actor);
        actorToData.Remove(actor);
    }

    private SwallowableObject GetDefaultDataForCategory(ObjectCategory category)
    {
        // Return minimal data for pool prewarming
        return new SwallowableObject
        {
            Category = category,
            Shape = PrimitiveShape.Cube,
            Size = 50f,
            ScaleMultiplier = Vector3.One
        };
    }
}
```

### 4.2 Spawn Manager Pattern

```csharp
public class SpawnManager
{
    private ObjectDatabase database;
    private ObjectPool pool;
    private List<SwallowableObject> activeObjects;
    private GameCore.SceneSystem.Scene scene;

    private float spawnTimer;
    private float spawnInterval;
    private const int MIN_OBJECTS = 150;
    private const int MAX_OBJECTS = 200;

    private Vector2 spawnAreaMin;
    private Vector2 spawnAreaMax;
    private const float SPAWN_GRID_SIZE = 200f;

    public SpawnManager(ObjectDatabase db, ObjectPool pool, GameCore.SceneSystem.Scene scene)
    {
        this.database = db;
        this.pool = pool;
        this.scene = scene;

        activeObjects = new List<SwallowableObject>();

        spawnAreaMin = new Vector2(-1000f, -1000f);
        spawnAreaMax = new Vector2(1000f, 1000f);
    }

    public void SpawnInitialObjects(int blackHoleLevel)
    {
        for (int i = 0; i < MIN_OBJECTS; i++)
        {
            SpawnRandomObject(blackHoleLevel);
        }
    }

    public void Update(float deltaTime, int blackHoleLevel)
    {
        // Check if need immediate spawns
        if (activeObjects.Count < MIN_OBJECTS)
        {
            int toSpawn = MIN_OBJECTS - activeObjects.Count;
            for (int i = 0; i < toSpawn; i++)
            {
                SpawnRandomObject(blackHoleLevel);
            }
        }

        // Gradual spawning
        if (activeObjects.Count < MAX_OBJECTS)
        {
            spawnTimer -= deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnRandomObject(blackHoleLevel);
                spawnInterval = Random.Range(0.5f, 1.5f);
                spawnTimer = spawnInterval;
            }
        }
    }

    private void SpawnRandomObject(int blackHoleLevel)
    {
        // Get random object from database
        SwallowableObject data = database.GetRandomObjectForLevel(blackHoleLevel);

        // Find valid spawn position
        Vector3 position = FindValidSpawnPosition(data.Size);
        if (position == Vector3.Zero)
        {
            Console.WriteLine("Failed to find valid spawn position");
            return;
        }

        // Get actor from pool
        PhysicsActor actor = pool.GetObject(data);
        actor.GetNode().position = position;

        data.Position = position;
        activeObjects.Add(data);
    }

    private Vector3 FindValidSpawnPosition(float objectSize)
    {
        const int MAX_ATTEMPTS = 20;
        const float MIN_DISTANCE = 50f;

        for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
        {
            // Random position in spawn area
            float x = Random.Range(spawnAreaMin.X, spawnAreaMax.X);
            float y = Random.Range(spawnAreaMin.Y, spawnAreaMax.Y);
            Vector3 position = new Vector3(x, y, objectSize / 2f); // Z is half-height

            // Check distance from all active objects
            bool validPosition = true;
            foreach (var obj in activeObjects)
            {
                float distance = Vector3.Distance(
                    new Vector3(position.X, position.Y, 0),
                    new Vector3(obj.Position.X, obj.Position.Y, 0)
                );

                float minRequiredDistance = objectSize + obj.Size + MIN_DISTANCE;
                if (distance < minRequiredDistance)
                {
                    validPosition = false;
                    break;
                }
            }

            if (validPosition)
                return position;
        }

        return Vector3.Zero; // Failed to find position
    }

    public void RemoveObject(SwallowableObject obj)
    {
        activeObjects.Remove(obj);
        pool.ReturnObject(obj.Actor);
    }

    public List<SwallowableObject> GetActiveObjects()
    {
        return activeObjects;
    }
}
```

---

## 5. Physics Implementation

### 5.1 Collision Setup

```csharp
public static class PhysicsLayers
{
    public const uint BLACKHOLE = 1u;
    public const uint SWALLOWABLE = 2u;
    public const uint ENVIRONMENT = 4u;

    public static uint GetMaskForBlackHole()
    {
        return SWALLOWABLE | ENVIRONMENT;
    }

    public static uint GetMaskForSwallowable()
    {
        return SWALLOWABLE | ENVIRONMENT;
    }
}

public void SetupBlackHolePhysics(RigidBody rb)
{
    rb.SetCollisionLayer(PhysicsLayers.BLACKHOLE);
    rb.SetCollisionMask(PhysicsLayers.GetMaskForBlackHole());
    rb.SetUseGravity(false);

    // Ignore all physical collisions
    rb.SetCollisionFilter((RigidBody other, Vector3 contact) => true);
}

public void SetupObjectPhysics(RigidBody rb, float objectSize)
{
    rb.SetCollisionLayer(PhysicsLayers.SWALLOWABLE);
    rb.SetCollisionMask(PhysicsLayers.GetMaskForSwallowable());
    rb.SetUseGravity(true);

    // Calculate mass based on size
    float mass = objectSize * objectSize * 0.1f;

    // Pass through black hole, collide with everything else
    rb.SetCollisionFilter((RigidBody other, Vector3 contact) =>
    {
        uint otherLayer = other.GetCollisionLayer();
        return otherLayer == PhysicsLayers.BLACKHOLE; // true = ignore
    });
}
```

### 5.2 Attraction Force System

```csharp
public class AttractionForceSystem
{
    public static void ApplyAttractionForce(
        Vector3 blackHolePos,
        float attractionRadius,
        SwallowableObject obj,
        bool canSwallow)
    {
        if (!canSwallow) return;

        float distance = Vector3.Distance(obj.Position, blackHolePos);
        if (distance >= attractionRadius) return;

        // Calculate force direction
        Vector3 direction = Vector3.Normalize(blackHolePos - obj.Position);

        // Calculate force magnitude (quadratic falloff)
        float t = 1.0f - (distance / attractionRadius);
        float forceMagnitude = 500f * (t * t);

        // Apply force
        RigidBody rb = obj.Component.GetRigidBody();
        rb.ApplyForce(direction * forceMagnitude);
    }
}
```

---

## 6. Performance Optimization

### 6.1 Update Optimization

```csharp
public class OptimizedPhysicsManager
{
    private List<SwallowableObject> activeObjects;
    private Vector3 blackHolePos;
    private float attractionRadiusSq; // Squared for faster distance check

    // Spatial partitioning
    private SpatialGrid<SwallowableObject> spatialGrid;

    public OptimizedPhysicsManager()
    {
        spatialGrid = new SpatialGrid<SwallowableObject>(200f); // 200 unit cells
    }

    public void Update(float deltaTime)
    {
        // Only check objects in nearby grid cells
        var nearbyObjects = spatialGrid.GetNearby(blackHolePos, attractionRadiusSq);

        foreach (var obj in nearbyObjects)
        {
            if (obj.IsBeingSwallowed) continue;

            // Fast distance check (squared)
            float distanceSq = Vector3.DistanceSquared(obj.Position, blackHolePos);
            if (distanceSq < attractionRadiusSq)
            {
                ApplyAttraction(obj, MathF.Sqrt(distanceSq));
            }
        }
    }
}

public class SpatialGrid<T> where T : class
{
    private Dictionary<Vector2Int, List<T>> grid;
    private float cellSize;

    public SpatialGrid(float cellSize)
    {
        this.cellSize = cellSize;
        grid = new Dictionary<Vector2Int, List<T>>();
    }

    private Vector2Int GetCell(Vector3 position)
    {
        return new Vector2Int(
            (int)(position.X / cellSize),
            (int)(position.Y / cellSize)
        );
    }

    public void Insert(T item, Vector3 position)
    {
        Vector2Int cell = GetCell(position);
        if (!grid.ContainsKey(cell))
            grid[cell] = new List<T>();
        grid[cell].Add(item);
    }

    public List<T> GetNearby(Vector3 position, float radius)
    {
        List<T> result = new List<T>();
        Vector2Int center = GetCell(position);

        int cellRadius = (int)(radius / cellSize) + 1;

        for (int x = -cellRadius; x <= cellRadius; x++)
        {
            for (int y = -cellRadius; y <= cellRadius; y++)
            {
                Vector2Int cell = new Vector2Int(center.X + x, center.Y + y);
                if (grid.TryGetValue(cell, out List<T> items))
                {
                    result.AddRange(items);
                }
            }
        }

        return result;
    }
}
```

### 6.2 Memory Optimization

```csharp
public class MemoryOptimizations
{
    // Reuse lists instead of allocating new ones
    private List<SwallowableObject> reusableObjectList = new List<SwallowableObject>(200);

    // Pool for score popups
    private ObjectPool<ScorePopup> scorePopupPool;

    // Cache frequently used values
    private Dictionary<int, float> speedMultiplierCache = new Dictionary<int, float>();

    public void PrecomputeValues()
    {
        // Precompute speed multipliers
        for (int level = 1; level <= 6; level++)
        {
            speedMultiplierCache[level] = CalculateSpeedMultiplier(level);
        }
    }

    public float GetSpeedMultiplier(int level)
    {
        return speedMultiplierCache.TryGetValue(level, out float value) ? value : 1.0f;
    }
}
```

---

## 7. Code Templates

### 7.1 Scene Setup Template

```csharp
// In ScopeData.cs
public static class BlackHoleGameData
{
    public static readonly GameDataScene BlackHoleScene = new GameDataScene(
        new GameDataSceneLink("BlackHoleScene"u8))
    {
        DisplayName = "Black Hole Scene",
        PlacedPlayerObjects = Array.Empty<GameDataPlacedObject>(), // Empty for dynamic objects
        HostedSceneTag = new HostedSceneTag(
            "blackhole_scene"u8,
            "new_scene_1"u8
        )
    };

    public static readonly GameDataGameMode BlackHoleMode = new GameDataGameMode(
        new GameDataGameModeLink("BlackHoleGameMode"u8))
    {
        DisplayName = "Black Hole Swallower",
        Scene = BlackHoleScene
    };
}
```

### 7.2 Main Game Registration Template

```csharp
// In game file (e.g., BlackHoleGame.cs)
public static void OnRegisterGameClass()
{
    Game.OnGameDataInitialization += () =>
    {
        // Register game mode
        RegisterGameMode();

        // Create game instance
        new BlackHoleGame();
    };
}

private static void RegisterGameMode()
{
    // Configuration happens in ScopeData.cs
    // This just initializes the game
}
```

### 7.3 Input Handler Template

```csharp
public class InputHandler
{
    private Camera camera;

    public InputHandler(Camera camera)
    {
        this.camera = camera;
    }

    public Vector3 GetTargetPositionInWorld()
    {
        // Get mouse position in screen space
        Vector2 mousePos = GetMouseScreenPosition();

        // Convert to world space using camera
        Ray ray = camera.ScreenPointToRay(mousePos);
        Plane groundPlane = new Plane(Vector3.UnitZ, 100f); // Z=100 plane

        if (ray.Intersects(groundPlane, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.Zero;
    }

    private Vector2 GetMouseScreenPosition()
    {
        // Engine-specific mouse input
        // This is a placeholder
        return new Vector2(Input.MouseX, Input.MouseY);
    }
}
```

---

## Implementation Checklist

### Phase 1: Foundation
- [ ] Create project structure
- [ ] Setup scene and game mode in ScopeData
- [ ] Create main game class with IThinker
- [ ] Implement basic black hole actor
- [ ] Add camera follow system

### Phase 2: Objects
- [ ] Create object database (JSON)
- [ ] Implement object pool system
- [ ] Create spawn manager
- [ ] Add physics components
- [ ] Test object creation and pooling

### Phase 3: Gameplay
- [ ] Implement swallow detection (OnTriggerEnter)
- [ ] Add attraction force system
- [ ] Create swallow animation
- [ ] Implement growth system
- [ ] Add level progression

### Phase 4: Polish
- [ ] Create UI system (HUD, menus)
- [ ] Add visual effects (particles, glow)
- [ ] Implement score and combo system
- [ ] Add sound effects
- [ ] Performance optimization

### Phase 5: Testing
- [ ] Balance tuning
- [ ] Bug fixing
- [ ] Performance profiling
- [ ] User testing

---

**END OF TECHNICAL SPECIFICATION**
