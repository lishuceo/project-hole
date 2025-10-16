# Black Hole Game - Implementation Decisions

**Date:** 2025-10-16
**Status:** Approved for Implementation

---

## Technical Decisions Summary

This document records all technical decisions made during the design review phase. These decisions are final and should be followed during implementation.

---

## 1. Camera System

**Decision:** Use **Perspective Camera** with FOV 45°

**Implementation:**
```csharp
var camera = ScreenViewport.Primary.Camera;

// Position relative to black hole (dynamic follow)
Vector3 cameraOffset = new Vector3(0, -800, 600);
camera.SetPosition(blackHolePos + cameraOffset, TimeSpan.FromSeconds(0.1));

// Fixed isometric angle
CameraRotation rotation = new CameraRotation(
    yaw: -90f,   // Face forward
    pitch: -60f, // Look down at angle
    roll: 0f
);
camera.SetRotation(rotation, TimeSpan.FromSeconds(0.1));

// FOV for perspective
// camera.SetFieldOfView(45f); // If API exists
```

**Dynamic Zoom:**
```csharp
float GetZoomMultiplier(int level)
{
    return level switch
    {
        1 or 2 => 1.0f,
        3 or 4 => 1.3f,
        5 or 6 => 1.6f,
        _ => 1.0f
    };
}

Vector3 zoomedOffset = baseOffset * GetZoomMultiplier(currentLevel);
```

---

## 2. Input System

**Decision:** Use `PrimaryViewport.RaycastTerrainOrWorldPanelXY()` for screen-to-world conversion

**Implementation:**
```csharp
private Vector3 GetTargetWorldPosition()
{
    var viewport = DeviceInfo.PrimaryViewport;
    var pointerPos = viewport.GetPointerInputPosition(PointerButtons.Button1);

    if (pointerPos.HasValue)
    {
        var rayResult = viewport.RaycastTerrainOrWorldPanelXY(pointerPos.Value);
        if (rayResult.IsHit)
        {
            return rayResult.Position;
        }
    }

    return blackHolePosition; // Fallback
}
```

**Input Event Registration:**
```csharp
private void RegisterInputHandlers()
{
    var inputManager = DeviceInfo.PrimaryInputManager;

    inputManager.OnPointerButtonMove += (e) =>
    {
        UpdateTargetPosition();
    };

    // Optional: Handle touch press for mobile
    inputManager.OnPointerButtonDown += (e) =>
    {
        UpdateTargetPosition();
    };
}
```

---

## 3. Data Configuration

**Decision:** Embed JSON data as C# string constant (no external file loading)

**Implementation:**
```csharp
public static class ObjectDatabaseConfig
{
    public const string DATABASE_JSON = @"
    {
      ""objects"": [
        {
          ""id"": ""traffic_cone"",
          ""displayName"": ""Traffic Cone"",
          ""category"": ""Tiny"",
          ""shape"": ""Cone"",
          ""size"": 15.0,
          ""score"": 5,
          ""growthValue"": 2.0,
          ""minLevel"": 1,
          ""spawnWeight"": 1.0,
          ""color"": ""#FF6600"",
          ""scale"": [1.0, 1.0, 1.0]
        },
        {
          ""id"": ""small_box"",
          ""displayName"": ""Small Box"",
          ""category"": ""Tiny"",
          ""shape"": ""Cube"",
          ""size"": 18.0,
          ""score"": 8,
          ""growthValue"": 3.0,
          ""minLevel"": 1,
          ""spawnWeight"": 1.0,
          ""color"": ""#8B4513"",
          ""scale"": [1.0, 1.0, 1.0]
        }
      ],
      ""spawnDistributions"": {
        ""1"": { ""Tiny"": 0.8, ""Small"": 0.2 },
        ""2"": { ""Tiny"": 0.5, ""Small"": 0.4, ""Medium"": 0.1 },
        ""3"": { ""Tiny"": 0.3, ""Small"": 0.4, ""Medium"": 0.25, ""Large"": 0.05 },
        ""4"": { ""Tiny"": 0.15, ""Small"": 0.3, ""Medium"": 0.35, ""Large"": 0.15, ""Huge"": 0.05 },
        ""5"": { ""Tiny"": 0.1, ""Small"": 0.2, ""Medium"": 0.3, ""Large"": 0.25, ""Huge"": 0.12, ""Massive"": 0.03 },
        ""6"": { ""Tiny"": 0.05, ""Small"": 0.15, ""Medium"": 0.25, ""Large"": 0.3, ""Huge"": 0.18, ""Massive"": 0.07 }
      }
    }";

    public static ObjectDatabase Load()
    {
        using var doc = JsonDocument.Parse(DATABASE_JSON);
        var root = doc.RootElement;

        var database = new ObjectDatabase();

        // Parse objects
        foreach (var objElem in root.GetProperty("objects").EnumerateArray())
        {
            var obj = new SwallowableObjectData
            {
                ID = objElem.GetProperty("id").GetString(),
                DisplayName = objElem.GetProperty("displayName").GetString(),
                Category = Enum.Parse<ObjectCategory>(objElem.GetProperty("category").GetString()),
                Shape = Enum.Parse<PrimitiveShape>(objElem.GetProperty("shape").GetString()),
                Size = objElem.GetProperty("size").GetSingle(),
                Score = objElem.GetProperty("score").GetInt32(),
                GrowthValue = objElem.GetProperty("growthValue").GetSingle(),
                MinLevel = objElem.GetProperty("minLevel").GetInt32(),
                SpawnWeight = objElem.GetProperty("spawnWeight").GetSingle(),
                Color = ParseColor(objElem.GetProperty("color").GetString())
            };

            database.Objects.Add(obj);
        }

        // Parse spawn distributions
        var distributions = root.GetProperty("spawnDistributions");
        foreach (var prop in distributions.EnumerateObject())
        {
            int level = int.Parse(prop.Name);
            var dist = new Dictionary<ObjectCategory, float>();

            foreach (var catProp in prop.Value.EnumerateObject())
            {
                var category = Enum.Parse<ObjectCategory>(catProp.Name);
                dist[category] = catProp.Value.GetSingle();
            }

            database.SpawnDistributions[level] = dist;
        }

        return database;
    }

    private static Color ParseColor(string hex)
    {
        hex = hex.TrimStart('#');
        int r = Convert.ToInt32(hex.Substring(0, 2), 16);
        int g = Convert.ToInt32(hex.Substring(2, 2), 16);
        int b = Convert.ToInt32(hex.Substring(4, 2), 16);
        return new Color(r / 255f, g / 255f, b / 255f, 1f);
    }
}
```

**Reason:** Avoids uncertainty about external file loading APIs and paths.

---

## 4. Time Handling

**Decision:** `Think(int deltaTime)` receives **milliseconds**, convert to seconds

**Implementation:**
```csharp
public class BlackHoleGame : IGameClass, IThinker
{
    public void Think(int deltaTime)
    {
        // deltaTime is in MILLISECONDS
        float deltaSeconds = deltaTime / 1000f;

        UpdateGame(deltaSeconds);
    }

    private void UpdateGame(float deltaSeconds)
    {
        // Update game timer
        gameTimer -= deltaSeconds;

        // Update black hole movement
        blackHoleController.Update(deltaSeconds);

        // Update spawn manager
        spawnManager.Update(deltaSeconds, blackHoleController.CurrentLevel);

        // Update combo tracker
        comboTracker.Update(deltaSeconds);

        // Update UI
        uiManager.UpdateHUD(currentScore, gameTimer, blackHoleController.CurrentLevel);
    }
}
```

---

## 5. Physics Ground Plane

**Decision:** Create static PhysicsActor ground plane for objects to rest on

**Implementation:**
```csharp
#if CLIENT
private PhysicsActor groundActor;

private void CreateGroundPlane()
{
    groundActor = new PhysicsActor(
        Player.LocalPlayer,
        PhysicsActor.GetPrimitiveLink(PrimitiveShape.Cube),
        physicsScene,
        new Vector3(0, 0, -50), // Below Z=0 ground level
        Vector3.Zero
    );

    var groundNode = groundActor.GetNode();
    groundNode.localScale = new Vector3(100, 100, 1); // 10000x10000 area, 100 units thick

    var groundRB = groundNode.GetComponent<RigidBody>();
    groundRB.SetKinematic(true); // Static, doesn't move
    groundRB.SetUseGravity(false);
    groundRB.SetCollisionLayer(LAYER_ENVIRONMENT);
    groundRB.SetCollisionMask(0xFFFFFFFF);

    Game.Logger.LogInformation("Ground plane created: 10000x10000 at Z=-50");
}
#endif
```

**Collision Layers:**
```csharp
public const uint LAYER_BLACKHOLE = 1u;
public const uint LAYER_SWALLOWABLE = 2u;
public const uint LAYER_ENVIRONMENT = 4u;
```

---

## 6. Scope Reduction for First Version

**Decision:** Simplify initial implementation to reduce complexity

### 6.1 Object Database Size
**Original:** 110+ objects across all categories
**Simplified:** 40-50 objects (5-10 per category)

**Object Count by Category:**
- Tiny: 8-10 objects
- Small: 6-8 objects
- Medium: 6-8 objects
- Large: 5-6 objects
- Huge: 4-5 objects
- Massive: 3-4 objects

**Total:** ~40 objects (expandable later)

### 6.2 Combo UI
**Original:** Animated fade in/out with pulse effects
**Simplified:** Instant show/hide, no animations

**Implementation:**
```csharp
private void UpdateComboDisplay(int comboCount, float multiplier)
{
    if (comboCount >= 5)
    {
        comboPanel.Visible = true;
        comboLabel.Text = $"COMBO x{comboCount}";
        multiplierLabel.Text = $"{multiplier:F1}x MULTIPLIER";
    }
    else
    {
        comboPanel.Visible = false;
    }
}
```

### 6.3 Achievement System
**Decision:** **Not implemented in first version**

Achievements will be added in a future update after core gameplay is validated.

### 6.4 Game Modes
**Decision:** Implement **Classic Mode only** (120s timed gameplay)

Survival mode and Challenge mode deferred to future versions.

---

## 7. Physics System Configuration

### 7.1 Black Hole Physics
```csharp
#if CLIENT
private void SetupBlackHolePhysics()
{
    var blackHoleRB = blackHoleNode.GetComponent<RigidBody>();

    // No gravity, controlled movement
    blackHoleRB.SetUseGravity(false);
    blackHoleRB.SetKinematic(false); // Dynamic for velocity control

    // Collision setup
    blackHoleRB.SetCollisionLayer(LAYER_BLACKHOLE);
    blackHoleRB.SetCollisionMask(0xFFFFFFFF); // Detect all for triggers

    // Pass through everything (collision filter returns true = ignore)
    blackHoleRB.SetCollisionFilter((RigidBody other, Vector3 contact) => true);

    // Set as trigger for swallowing detection
    blackHoleRB.SetTrigger(true);

    Game.Logger.LogInformation("Black hole physics configured");
}
#endif
```

### 7.2 Object Physics
```csharp
#if CLIENT
private void SetupObjectPhysics(PhysicsActor actor, SwallowableObjectData data)
{
    var rb = actor.GetNode().GetComponent<RigidBody>();

    // Enable gravity
    rb.SetUseGravity(true);

    // Mass based on size
    float mass = data.Size * data.Size * 0.1f;
    rb.SetMass(mass);

    // Collision setup
    rb.SetCollisionLayer(LAYER_SWALLOWABLE);
    rb.SetCollisionMask(LAYER_SWALLOWABLE | LAYER_ENVIRONMENT); // Collide with objects and ground

    // Collision filter: ignore black hole physical collision
    rb.SetCollisionFilter((RigidBody other, Vector3 contact) =>
    {
        return other.GetCollisionLayer() == LAYER_BLACKHOLE; // true = ignore
    });

    // Physics properties
    rb.SetFriction(0.6f);
    rb.SetRestitution(0.3f); // Bounciness
    rb.SetAngularDamping(0.1f);
    rb.SetLinearDamping(0.05f);
}
#endif
```

---

## 8. Spatial Partitioning Grid

**Decision:** Use 200x200 unit grid cells for optimization

**Implementation:**
```csharp
public class SpatialGrid
{
    private const float CELL_SIZE = 200f;
    private const int GRID_SIZE = 10; // 2000 / 200 = 10x10 grid
    private Dictionary<(int, int), List<PhysicsActor>> grid;

    public SpatialGrid()
    {
        grid = new Dictionary<(int, int), List<PhysicsActor>>();
    }

    public void Add(PhysicsActor actor, Vector3 position)
    {
        var cell = GetCellCoords(position);
        if (!grid.ContainsKey(cell))
            grid[cell] = new List<PhysicsActor>();

        grid[cell].Add(actor);
    }

    public void Remove(PhysicsActor actor, Vector3 position)
    {
        var cell = GetCellCoords(position);
        if (grid.ContainsKey(cell))
            grid[cell].Remove(actor);
    }

    public List<PhysicsActor> GetNearby(Vector3 position, float radius)
    {
        var result = new List<PhysicsActor>();
        var centerCell = GetCellCoords(position);

        int radiusCells = (int)Math.Ceiling(radius / CELL_SIZE);

        for (int dx = -radiusCells; dx <= radiusCells; dx++)
        {
            for (int dy = -radiusCells; dy <= radiusCells; dy++)
            {
                var cell = (centerCell.x + dx, centerCell.y + dy);
                if (grid.ContainsKey(cell))
                    result.AddRange(grid[cell]);
            }
        }

        return result;
    }

    private (int x, int y) GetCellCoords(Vector3 position)
    {
        int x = (int)Math.Floor((position.X + 1000f) / CELL_SIZE);
        int y = (int)Math.Floor((position.Y + 1000f) / CELL_SIZE);
        return (x, y);
    }
}
```

---

## 9. Performance Targets

**Target Metrics:**
- FPS: **60 FPS** (minimum 30 FPS acceptable)
- Object Count: **150-200 simultaneous objects**
- Memory: **< 300MB** (physics actors use more than 2D panels)
- Physics Update: **60 Hz fixed timestep**
- Input Latency: **< 100ms**

**Optimization Strategies:**
1. Object pooling (pre-allocate 50 actors per category)
2. Spatial partitioning (200x200 grid)
3. Squared distance checks (avoid `Math.Sqrt()` until needed)
4. Update UI only on value change (dirty flag pattern)

---

## 10. Development Phases

### Phase 1: Core Prototype (Days 1-3)
- [ ] GameMode and Scene configuration
- [ ] Black hole movement (mouse control)
- [ ] Basic object spawning (10 objects)
- [ ] Trigger detection and swallowing
- [ ] Simple growth (size increase only)

**Success Criteria:** Black hole moves smoothly and can swallow objects

### Phase 2: Physics System (Days 4-6)
- [ ] Ground plane implementation
- [ ] Collision layers setup
- [ ] Attraction force system
- [ ] Swallow animation (0.15s scale-down)
- [ ] Object pooling (50 actors)
- [ ] Spawn 150 objects

**Success Criteria:** 60 FPS with 150 objects, smooth physics

### Phase 3: Progression (Days 7-9)
- [ ] Level system (6 levels)
- [ ] Score calculation
- [ ] Combo tracker (2s window)
- [ ] Timer (120s countdown)
- [ ] Spawn distribution by level
- [ ] Full object database (40-50 objects)

**Success Criteria:** Complete game loop works, 2-minute games are fun

### Phase 4: UI and Polish (Days 10-12)
- [ ] HUD (score, timer, level, progress bar)
- [ ] Main menu
- [ ] End game screen
- [ ] Camera zoom by level
- [ ] Visual feedback (particles if time permits)
- [ ] Sound effects (basic)

**Success Criteria:** Game is playable and polished

### Phase 5: Testing and Balance (Days 13-14)
- [ ] Playtest and balance tuning
- [ ] Bug fixes
- [ ] Performance optimization
- [ ] Final adjustments

**Success Criteria:** No critical bugs, fun gameplay

---

## 11. API Verification Checklist

**To verify before starting implementation:**

- [x] `DeviceInfo.PrimaryViewport.RaycastTerrainOrWorldPanelXY()` - ✅ Confirmed working
- [x] `DeviceInfo.PrimaryInputManager.OnPointerButtonMove` - ✅ Confirmed working
- [x] `ScreenViewport.Primary.Camera` - ✅ Available
- [x] `Think(int deltaTime)` is in milliseconds - ✅ Confirmed in AI_QUICK_RULES.md
- [x] `PhysicsActor.SetTrigger()` method - ✅ Available in RigidBody
- [x] `RigidBody.SetCollisionFilter()` - ✅ Available
- [ ] Camera FOV setting (may not be exposed, use zoom via offset instead)

---

## 12. Critical Implementation Notes

### 12.1 WasiCore-Specific Rules
- ✅ All physics code wrapped in `#if CLIENT ... #endif`
- ✅ Use `.NET 9.0` in .csproj
- ✅ Use `Game.Logger.LogInformation()` not `Console.WriteLine()`
- ✅ Use `await Game.Delay()` not `Task.Delay()`
- ✅ Trigger handlers MUST return `bool`
- ✅ UIPosition uses `.Left` and `.Top` (not `.X` and `.Y`)
- ✅ Canvas.Children is READ-ONLY (use drawing methods)
- ✅ Always call `canvas.ResetState()` before drawing

### 12.2 Scene Configuration
```csharp
// In ScopeData.cs
public static class GameMode
{
    public static readonly GameLink<GameDataGameMode> BlackHoleGame = new("BlackHoleGame"u8);
}

public static class Scene
{
    public static readonly GameLink<GameDataScene> BlackHoleScene = new("blackhole_scene"u8);
}

// In OnRegisterGameClass()
_ = new GameDataScene(Scene.BlackHoleScene)
{
    Name = "Black Hole Scene",
    HostedSceneTag = new HostedSceneTag("blackhole_scene"u8, "new_scene_1"u8),
    Size = new(2000, 2000),
    PlacedPlayerObjects = [] // Empty - clean scene
};

_ = new GameDataGameMode(GameMode.BlackHoleGame)
{
    Name = "Black Hole Physics Game",
    Gameplay = Gameplay.Default,
    PlayerSettings = PlayerSettings.Default,
    SceneList = [Scene.BlackHoleScene],
    DefaultScene = Scene.BlackHoleScene
};
```

```csharp
// In GlobalConfig.cs
GameDataGlobalConfig.AvailableGameModes = new()
{
    {"BlackHoleGame", ScopeData.GameMode.BlackHoleGame},
};

GameDataGlobalConfig.TestGameMode = ScopeData.GameMode.BlackHoleGame;
```

---

## Document Status

**Status:** ✅ **APPROVED - Ready for Implementation**

All technical questions have been answered and decisions finalized. Implementation can proceed following this specification.

---

**Last Updated:** 2025-10-16
**Approved By:** Project Team
**Next Step:** Begin Phase 1 implementation
