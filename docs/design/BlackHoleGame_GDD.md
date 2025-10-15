# Black Hole Physics Game - Game Design Document (GDD)

**Project Name:** Black Hole Swallower
**Engine:** WasiCore/SCE
**Genre:** 3D Physics Action - Casual Competitive
**Target Platform:** PC/Web
**Development Timeline:** 4-6 weeks
**Document Version:** 1.0
**Last Updated:** 2025-10-15

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Core Gameplay Mechanics](#2-core-gameplay-mechanics)
3. [Physics System Design](#3-physics-system-design)
4. [Object System](#4-object-system)
5. [Progression System](#5-progression-system)
6. [Game Modes](#6-game-modes)
7. [Scene Design](#7-scene-design)
8. [UI/UX Design](#8-uiux-design)
9. [Balance and Tuning](#9-balance-and-tuning)
10. [Technical Implementation](#10-technical-implementation)
11. [Audio Design](#11-audio-design)
12. [Polish and Juice](#12-polish-and-juice)

---

## 1. Executive Summary

### 1.1 Game Concept

Black Hole Swallower is a 3D physics-based action game where players control a black hole that grows by consuming objects in a city environment. Inspired by Hole.io, the game features intuitive mouse/touch controls, satisfying physics interactions, and progressive difficulty through size-based gameplay.

### 1.2 Core Pillars

1. **Instant Satisfaction** - Immediate feedback from swallowing objects
2. **Clear Progression** - Visible growth and unlocking larger objects
3. **Smooth Physics** - Responsive controls and natural object interactions
4. **Time Pressure** - Race against the clock to maximize score
5. **Easy to Learn** - Simple one-input control scheme

### 1.3 Target Audience

- **Primary:** Casual gamers (18-35 years)
- **Secondary:** Mobile/web game enthusiasts
- **Skill Level:** All skill levels (easy to learn, hard to master)
- **Session Length:** 2-5 minutes per game

### 1.4 Unique Selling Points

- Physics-driven growth mechanics
- Satisfying object consumption with visual feedback
- Progressive unlock system based on size
- Competitive scoring with increasing challenge
- Built on WasiCore's robust physics engine

---

## 2. Core Gameplay Mechanics

### 2.1 Black Hole Control

#### 2.1.1 Movement System

**Input Method:**
- **PC:** Mouse movement (black hole follows cursor)
- **Touch:** Touch and drag (black hole follows finger)

**Movement Parameters:**
```
Base Speed: 400 units/second
Speed Multiplier by Size:
- Tier 1 (Tiny):   1.2x speed
- Tier 2 (Small):  1.0x speed
- Tier 3 (Medium): 0.85x speed
- Tier 4 (Large):  0.7x speed
- Tier 5 (Huge):   0.6x speed
- Tier 6 (Massive): 0.5x speed

Acceleration: 8 units/second²
Deceleration: 12 units/second² (no input)
Max Speed Cap: 800 units/second
```

**Movement Feel:**
- Smooth interpolation with damping
- Slight momentum for weight feel
- No sudden stops (deceleration curve)

#### 2.1.2 Camera System

**Camera Configuration:**
```
Camera Type: Orthographic (top-down view with slight angle)
Position Offset: (0, -800, 600) relative to black hole
Look At: Black hole center position
FOV Angle: 45 degrees (isometric feel)

Dynamic Zoom:
- Tier 1-2: Zoom = 1.0 (close view)
- Tier 3-4: Zoom = 1.3 (medium view)
- Tier 5-6: Zoom = 1.6 (wide view)

Zoom Transition: Smooth lerp over 0.5 seconds
```

### 2.2 Swallowing Mechanics

#### 2.2.1 Trigger System

**Swallow Zone:**
- Black hole has a spherical trigger collider
- Trigger radius = Black Hole Visual Radius * 1.1
- No physics collision (trigger only)

**Swallow Condition:**
```
CanSwallow(object) =
    object.Size <= (BlackHole.Size * SwallowThreshold)

SwallowThreshold = 0.85
```

**Example:**
- Black Hole Size = 100 units
- Can swallow objects up to: 100 * 0.85 = 85 units

#### 2.2.2 Physics Interaction

**Object States:**
1. **Free State:** Normal physics, gravity enabled
2. **Attracted State:** Within attraction range, pulled toward black hole
3. **Swallowing State:** Inside trigger, animating toward center
4. **Consumed State:** Destroyed, score/growth applied

**Attraction Force:**
```
Attraction Range = BlackHole.Size * 2.0

Force Calculation:
distance = Distance(object, blackHole)
if (distance < AttractionRange && CanSwallow(object))
{
    forceDirection = Normalize(blackHole.position - object.position)
    forceMagnitude = AttractionStrength * (1 - distance / AttractionRange)²
    ApplyForce(object, forceDirection * forceMagnitude)
}

AttractionStrength = 500 (force units)
```

**Swallow Animation:**
```
Duration: 0.15 seconds
Easing: EaseInQuad
Scale: Lerp from 1.0 to 0.0
Position: Lerp to black hole center
Rotation: Random spin (360° * 2)
```

### 2.3 Growth System

#### 2.3.1 Size Progression

**Growth Formula:**
```
NewSize = CurrentSize + (ObjectValue * GrowthRate)

GrowthRate varies by tier:
- Tier 1-2: 1.0 (fast growth)
- Tier 3-4: 0.7 (medium growth)
- Tier 5-6: 0.5 (slow growth)
```

**Size Calculation:**
```
Visual Radius = 50 + (Level * 15)
Trigger Radius = Visual Radius * 1.1
Attraction Radius = Visual Radius * 2.0
```

#### 2.3.2 Level System

**Level Thresholds:**

| Level | Size Range | Visual Radius | Can Swallow | Description |
|-------|------------|---------------|-------------|-------------|
| 1 | 0-100 | 50-65 units | Tiny objects | Starting size |
| 2 | 100-300 | 65-80 units | Small objects | Early game |
| 3 | 300-600 | 80-95 units | Medium objects | Mid game |
| 4 | 600-1000 | 95-110 units | Large objects | Advanced |
| 5 | 1000-1500 | 110-125 units | Huge objects | Expert |
| 6 | 1500+ | 125-140+ units | Everything | Maximum |

**Level Up Event:**
- Visual effect: Pulse ring animation
- Sound effect: Level up chime
- Camera zoom adjustment
- Speed reduction applied
- UI notification: "Level Up! Now: [Level]"

---

## 3. Physics System Design

### 3.1 Collision Layers

**Layer Configuration:**
```
Layer 1: BlackHole (MainUnit)
  - CollisionMask: 0xFFFFFFFF (collides with all)
  - Trigger: true
  - Gravity: false

Layer 2: SwallowableObjects
  - CollisionMask: 0xFFFFFFFF (collides with all)
  - Trigger: false (solid collision)
  - Gravity: true

Layer 4: Environment (ground, walls)
  - CollisionMask: 0xFFFFFFFF
  - Trigger: false
  - Gravity: false (static)
```

### 3.2 RigidBody Configuration

**Black Hole RigidBody:**
```csharp
RigidBody blackHoleRB = node.GetComponent<RigidBody>();
blackHoleRB.SetUseGravity(false);
blackHoleRB.SetCollisionLayer(1u);
blackHoleRB.SetCollisionMask(0xFFFFFFFF);
blackHoleRB.SetLinearVelocity(movementDirection * speed);
```

**Object RigidBody (Default):**
```csharp
RigidBody objectRB = node.GetComponent<RigidBody>();
objectRB.SetUseGravity(true);
objectRB.SetCollisionLayer(2u);
objectRB.SetCollisionMask(0xFFFFFFFF);

// Mass based on size
float mass = objectSize * objectSize * 0.1f;
```

### 3.3 Collision Filter

**Black Hole Collision Filter:**
```csharp
blackHoleRB.SetCollisionFilter((RigidBody otherRB, Vector3 contactPoint) =>
{
    // Ignore physical collision (pass through)
    // Only use trigger detection
    return true; // true = ignore collision
});
```

**Object Collision Filter:**
```csharp
objectRB.SetCollisionFilter((RigidBody otherRB, Vector3 contactPoint) =>
{
    // Objects collide with each other and environment
    // But not with black hole (pass through)
    uint otherLayer = otherRB.GetCollisionLayer();
    if (otherLayer == 1u) // BlackHole layer
        return true; // ignore
    return false; // collide
});
```

### 3.4 Physics Constants

```
Gravity: Vector3(0, 0, -980) // 980 cm/s² downward
Time Step: Fixed 60 FPS (0.0166s)
Max Linear Velocity: 2000 units/s
Angular Damping: 0.1
Linear Damping: 0.05
Bounce Coefficient: 0.3
Friction Coefficient: 0.6
```

---

## 4. Object System

### 4.1 Object Categories

Objects are organized into 6 categories based on size and score value:

| Category | Size Range | Score Range | Min Level Required |
|----------|------------|-------------|-------------------|
| Tiny | 10-30 units | 5-15 | 1 |
| Small | 30-60 units | 15-40 | 2 |
| Medium | 60-100 units | 40-80 | 3 |
| Large | 100-150 units | 80-150 | 4 |
| Huge | 150-220 units | 150-300 | 5 |
| Massive | 220-300 units | 300-500 | 6 |

### 4.2 Object Type Definitions

Using WasiCore's PrimitiveShapes:

#### 4.2.1 Tiny Objects (Level 1+)

| Object Name | Shape | Size | Score | Spawn Weight | Growth Value |
|-------------|-------|------|-------|--------------|--------------|
| Traffic Cone | Cone | 15 | 5 | 1.0 | 2 |
| Small Box | Cube | 18 | 8 | 1.0 | 3 |
| Ball | Sphere | 12 | 6 | 0.8 | 2 |
| Bottle | Cylinder | 20 | 10 | 0.7 | 3 |
| Rock | Sphere | 14 | 7 | 0.9 | 2.5 |

**Color Scheme (Tiny):**
- Traffic Cone: Orange (#FF6600)
- Small Box: Brown (#8B4513)
- Ball: Red (#FF0000)
- Bottle: Blue (#0066FF)
- Rock: Gray (#808080)

#### 4.2.2 Small Objects (Level 2+)

| Object Name | Shape | Size | Score | Spawn Weight | Growth Value |
|-------------|-------|------|-------|--------------|--------------|
| Fire Hydrant | Cylinder | 35 | 20 | 1.0 | 5 |
| Mailbox | Cube | 40 | 25 | 0.9 | 6 |
| Trash Can | Cylinder | 38 | 22 | 1.0 | 5.5 |
| Bench | Cube | 55 | 35 | 0.7 | 8 |
| Sign Post | Cone | 32 | 18 | 0.8 | 5 |
| Barrel | Cylinder | 45 | 28 | 0.8 | 7 |

**Color Scheme (Small):**
- Fire Hydrant: Red (#CC0000)
- Mailbox: Blue (#0044AA)
- Trash Can: Green (#228B22)
- Bench: Brown (#654321)
- Sign Post: Yellow (#FFD700)
- Barrel: Gray (#696969)

#### 4.2.3 Medium Objects (Level 3+)

| Object Name | Shape | Size | Score | Spawn Weight | Growth Value |
|-------------|-------|------|-------|--------------|--------------|
| Small Car | Cube | 75 | 50 | 1.0 | 12 |
| Motorcycle | Capsule | 65 | 45 | 0.8 | 10 |
| Food Cart | Cube | 70 | 48 | 0.7 | 11 |
| Phone Booth | Cube | 80 | 55 | 0.6 | 13 |
| Large Tree | Cone | 85 | 60 | 0.5 | 14 |
| Dumpster | Cube | 90 | 65 | 0.7 | 15 |

**Color Scheme (Medium):**
- Small Car: Various (Red, Blue, Yellow, Green)
- Motorcycle: Black (#000000)
- Food Cart: White (#FFFFFF)
- Phone Booth: Red (#DC143C)
- Large Tree: Green (#228B22)
- Dumpster: Green (#2F4F2F)

#### 4.2.4 Large Objects (Level 4+)

| Object Name | Shape | Size | Score | Spawn Weight | Growth Value |
|-------------|-------|------|-------|--------------|--------------|
| Sedan Car | Cube | 120 | 100 | 1.0 | 20 |
| Van | Cube | 135 | 115 | 0.8 | 23 |
| Small Truck | Cube | 140 | 120 | 0.7 | 24 |
| Bus Stop Shelter | Cube | 110 | 90 | 0.6 | 18 |
| Small Fountain | Cylinder | 125 | 105 | 0.5 | 21 |
| Kiosk | Cube | 115 | 95 | 0.7 | 19 |

**Color Scheme (Large):**
- Sedan: Various colors
- Van: White (#F5F5F5)
- Small Truck: Yellow (#FFD700)
- Bus Stop: Gray (#808080)
- Fountain: Blue (#4682B4)
- Kiosk: Red (#CD5C5C)

#### 4.2.5 Huge Objects (Level 5+)

| Object Name | Shape | Size | Score | Spawn Weight | Growth Value |
|-------------|-------|------|-------|--------------|--------------|
| Large Truck | Cube | 180 | 180 | 1.0 | 35 |
| Bus | Cube | 200 | 220 | 0.8 | 40 |
| Small House | Cube | 190 | 200 | 0.5 | 38 |
| Large Fountain | Cylinder | 170 | 170 | 0.6 | 33 |
| Billboard | Cube | 160 | 150 | 0.7 | 30 |
| Container | Cube | 185 | 190 | 0.8 | 36 |

**Color Scheme (Huge):**
- Large Truck: Blue (#4169E1)
- Bus: Yellow (#FFA500)
- Small House: Various colors
- Large Fountain: Cyan (#00CED1)
- Billboard: Gray with colorful ads
- Container: Orange (#FF6347)

#### 4.2.6 Massive Objects (Level 6+)

| Object Name | Shape | Size | Score | Spawn Weight | Growth Value |
|-------------|-------|------|-------|--------------|--------------|
| Large Building | Cube | 250 | 400 | 0.5 | 60 |
| Tower | Cylinder | 280 | 450 | 0.3 | 70 |
| Warehouse | Cube | 240 | 380 | 0.6 | 58 |
| Multi-Story Building | Cube | 270 | 430 | 0.4 | 65 |
| Monument | Cone | 260 | 410 | 0.3 | 62 |

**Color Scheme (Massive):**
- Large Building: Gray (#A9A9A9)
- Tower: White (#DCDCDC)
- Warehouse: Brown (#8B7355)
- Multi-Story: Various colors
- Monument: Stone gray (#808080)

### 4.3 Object Data Structure

```csharp
public class SwallowableObjectData
{
    public string ObjectID;
    public string DisplayName;
    public ObjectCategory Category;
    public PrimitiveShape Shape;
    public float Size; // Base size in units
    public int Score;
    public float GrowthValue;
    public int MinBlackHoleLevel;
    public float SpawnWeight;
    public Color TintColor;
    public Vector3 ScaleMultiplier; // For non-uniform scaling
}

public enum ObjectCategory
{
    Tiny,
    Small,
    Medium,
    Large,
    Huge,
    Massive
}
```

### 4.4 Spawn Configuration

**Spawn Distribution by Level:**

| Black Hole Level | Tiny % | Small % | Medium % | Large % | Huge % | Massive % |
|------------------|--------|---------|----------|---------|--------|-----------|
| 1 | 80% | 20% | 0% | 0% | 0% | 0% |
| 2 | 50% | 40% | 10% | 0% | 0% | 0% |
| 3 | 30% | 40% | 25% | 5% | 0% | 0% |
| 4 | 15% | 30% | 35% | 15% | 5% | 0% |
| 5 | 10% | 20% | 30% | 25% | 12% | 3% |
| 6 | 5% | 15% | 25% | 30% | 18% | 7% |

**Total Objects in Scene:**
```
Start: 150 objects
Max: 200 objects
Respawn Rate: 2-3 objects per second
Respawn Delay after Swallow: 0.5-1.5 seconds (random)
```

---

## 5. Progression System

### 5.1 Experience and Leveling

**Experience Formula:**
```
XP = Score accumulated
Level progression is automatic based on size thresholds (see Level System table)
```

**Level Benefits:**

| Level | Unlock | Speed Multiplier | Max Swallowable Size |
|-------|--------|------------------|---------------------|
| 1 | Tiny objects | 1.2x | 30 units |
| 2 | Small objects | 1.0x | 60 units |
| 3 | Medium objects | 0.85x | 100 units |
| 4 | Large objects | 0.7x | 150 units |
| 5 | Huge objects | 0.6x | 220 units |
| 6 | Massive objects | 0.5x | 300 units |

### 5.2 Score Multipliers

**Combo System:**
```
Combo Counter: Tracks consecutive swallows within time window
Combo Window: 2 seconds between swallows
Combo Reset: After 2 seconds of no swallowing

Combo Multipliers:
- 1-4 swallows:   1.0x (no bonus)
- 5-9 swallows:   1.2x
- 10-14 swallows: 1.5x
- 15-19 swallows: 1.8x
- 20+ swallows:   2.0x

Score Calculation:
FinalScore = BaseScore * ComboMultiplier
```

**Size Bonus:**
```
Larger objects give bonus score:
- Tiny: +0%
- Small: +10%
- Medium: +20%
- Large: +30%
- Huge: +40%
- Massive: +50%
```

### 5.3 Achievement System

**Achievements:**

| Achievement Name | Requirement | Reward |
|------------------|-------------|--------|
| First Bite | Swallow 1st object | +50 bonus points |
| Growing Up | Reach Level 2 | +100 bonus points |
| Size Matters | Reach Level 4 | +200 bonus points |
| Unstoppable | Reach Level 6 | +500 bonus points |
| Combo Novice | Get 5-combo | +100 bonus points |
| Combo Master | Get 10-combo | +250 bonus points |
| Combo God | Get 20-combo | +500 bonus points |
| Speed Demon | Swallow 10 objects in 5 seconds | +200 bonus points |
| City Eater | Swallow 50 objects in one game | +300 bonus points |
| Black Hole King | Score 5000+ points | +1000 bonus points |

---

## 6. Game Modes

### 6.1 Classic Mode (Primary)

**Core Design:**
- Time-limited gameplay
- Score-focused competition
- Progressive difficulty

**Rules:**
```
Duration: 120 seconds (2 minutes)
Starting Level: 1
Starting Size: 50 units
Goal: Achieve highest score possible

Victory Conditions:
- Score Ranks:
  - Bronze: 1000+ points
  - Silver: 2500+ points
  - Gold: 5000+ points
  - Platinum: 8000+ points

Game Over: When timer reaches 0
```

**Scoring:**
```
Base Score: Sum of all swallowed objects
Time Bonus: RemainingTime * 10
Combo Bonus: Max combo achieved * 50
Level Bonus: Final level * 200
Total Score = Base + Time + Combo + Level
```

**Difficulty Curve:**
```
Phase 1 (0-30s): Easy - Many tiny objects
Phase 2 (30-60s): Medium - More variety
Phase 3 (60-90s): Hard - Fewer small objects
Phase 4 (90-120s): Intense - Large objects dominant
```

### 6.2 Survival Mode (Secondary)

**Core Design:**
- No time limit
- Endurance challenge
- Increasing hazards

**Rules:**
```
Duration: Until death
Starting Level: 1
Starting Size: 50 units
Goal: Survive as long as possible

Hazards:
- Red explosive barrels (damage and shrink)
- Environmental obstacles (growing over time)
- Shrinking rate over time

Death Conditions:
- Size drops below 40 units
- Hit by too many hazards
```

**Hazard System:**
```
Explosive Barrels:
- Appear at: 30s, 60s, 90s, 120s...
- Size: 80 units
- Effect: Reduces black hole size by 30%
- Visual: Red color, pulsing glow

Shrink Rate:
- Starts at 60s
- Rate: -1 size per second
- Forces player to keep eating
```

### 6.3 Challenge Mode (Future)

**Planned Features:**
- Daily challenges with specific goals
- "Eat X objects in Y time"
- "Reach level N with score M"
- Leaderboard for each challenge

---

## 7. Scene Design

### 7.1 City Environment

**Scene Layout:**
```
Dimensions: 2000 x 2000 units (square play area)
Ground Level: Z = 0
Sky Height: Decorative only

Zones:
1. Residential Area (25% of map)
   - Small objects dominant
   - Houses, cars, trees

2. Commercial District (25% of map)
   - Medium objects dominant
   - Shops, signs, benches

3. Industrial Area (25% of map)
   - Large objects dominant
   - Warehouses, trucks, containers

4. City Center (25% of map)
   - Huge/Massive objects
   - Buildings, towers, monuments
```

### 7.2 Object Distribution

**Spawn Zones:**
```
Zone Grid: 10x10 cells (200x200 units each)

Spawn Rules:
- Minimum distance between objects: 50 units
- Maximum objects per cell: 3-5 (based on size)
- Respawn in same zone when consumed
- Avoid spawning under black hole

Spawn Algorithm:
1. Select zone based on black hole level
2. Choose object category from spawn table
3. Find valid position (no overlap)
4. Create object with physics
5. Add to active object pool
```

### 7.3 Visual Design

**Ground Texture:**
```
Type: Asphalt/concrete material
Color: Dark gray (#333333)
Grid Lines: Light gray roads (#555555)
Grid Size: 200 units
```

**Environmental Decorations (non-interactive):**
- Street lamps (small cylinders)
- Trees (green cones) - decorative only
- Road markings (painted lines)
- Shadows (baked into ground texture)

**Lighting:**
```
Main Light: Directional (sun)
  Direction: (-1, -1, -1) normalized
  Color: Warm white (#FFFFEE)
  Intensity: 1.0

Ambient Light:
  Color: Light gray (#AAAAAA)
  Intensity: 0.4

Black Hole Glow:
  Type: Point light attached to black hole
  Color: Purple (#AA00FF)
  Intensity: 2.0
  Range: BlackHole.Size * 2
```

### 7.4 Camera Configuration

**Camera Setup:**
```csharp
// Initial camera position
Vector3 cameraOffset = new Vector3(0, -800, 600);
Vector3 cameraTarget = blackHolePosition;

ScreenViewport.Primary.Camera.SetPosition(
    blackHolePosition + cameraOffset,
    TimeSpan.FromSeconds(0.1)
);

// Camera rotation (fixed isometric view)
CameraRotation rotation = new CameraRotation(
    yaw: -90f,   // Face forward
    pitch: -60f, // Angle down
    roll: 0f
);
ScreenViewport.Primary.Camera.SetRotation(
    rotation,
    TimeSpan.FromSeconds(0.1)
);
```

**Dynamic Zoom:**
```csharp
float GetCameraZoomForLevel(int level)
{
    switch (level)
    {
        case 1:
        case 2: return 1.0f;
        case 3:
        case 4: return 1.3f;
        case 5:
        case 6: return 1.6f;
        default: return 1.0f;
    }
}
```

---

## 8. UI/UX Design

### 8.1 HUD (Heads-Up Display)

**Top Bar:**
```
+--------------------------------------------------+
| Score: 1234       Level: 3      Time: 01:45      |
+--------------------------------------------------+
```

**Elements:**
- Score Counter: Top-left, large font, updates in real-time
- Level Indicator: Top-center, "Level [N]" with icon
- Timer: Top-right, countdown format (MM:SS)

**Progress Bar:**
```
Bottom-center: Size progress to next level
+--------------------------------------------------+
| [=========>                    ] 45% to Level 4  |
+--------------------------------------------------+
```

**Combo Display:**
```
Center-right (appears during combo):
+------------------+
| COMBO x5         |
| +240 POINTS!     |
+------------------+

Animation: Fade in on combo start, pulse on increment, fade out after 2s
```

### 8.2 Start Screen

**Layout:**
```
+--------------------------------------------------+
|                                                  |
|            BLACK HOLE SWALLOWER                  |
|                 [Logo/Title]                     |
|                                                  |
|              [START GAME]                        |
|              [LEADERBOARD]                       |
|              [HOW TO PLAY]                       |
|              [SETTINGS]                          |
|                                                  |
+--------------------------------------------------+
```

### 8.3 End Game Screen

**Layout:**
```
+--------------------------------------------------+
|              GAME OVER!                          |
|                                                  |
|     Final Score: 3450                            |
|     Rank: GOLD                                   |
|                                                  |
|     Objects Eaten: 89                            |
|     Max Combo: x12                               |
|     Max Level: 5                                 |
|                                                  |
|     [PLAY AGAIN]    [MAIN MENU]                  |
+--------------------------------------------------+
```

### 8.4 Tutorial Overlay

**First-Time Player Tutorial:**
```
Phase 1 (0-5s): "Move your mouse to control the black hole"
Phase 2 (5-10s): "Swallow objects smaller than you"
Phase 3 (10-15s): "Grow bigger to eat larger objects"
Phase 4 (15-20s): "Race against time for high score!"

Overlay: Semi-transparent panel with arrow indicators
Skip: Press ESC or click "Skip Tutorial"
```

### 8.5 Visual Feedback

**Swallow Effect:**
```
1. Object starts glowing when in attraction range
2. Trail effect as object moves toward black hole
3. Scale down animation (0.15s)
4. Particle burst at consumption point
5. Score popup (+[points]) floating upward
```

**Level Up Effect:**
```
1. Flash effect on black hole
2. Expanding ring animation
3. Screen shake (subtle)
4. UI banner: "LEVEL UP!" (2s display)
5. Sound effect: Power-up chime
```

**Combo Effect:**
```
1. Combo counter appears/updates
2. Pulse animation on each swallow
3. Color changes based on combo tier:
   - 5-9: Yellow
   - 10-14: Orange
   - 15-19: Red
   - 20+: Purple
```

---

## 9. Balance and Tuning

### 9.1 Core Balance Constants

```csharp
// Growth Rates
public const float TINY_GROWTH_RATE = 1.0f;
public const float SMALL_GROWTH_RATE = 1.0f;
public const float MEDIUM_GROWTH_RATE = 0.7f;
public const float LARGE_GROWTH_RATE = 0.7f;
public const float HUGE_GROWTH_RATE = 0.5f;
public const float MASSIVE_GROWTH_RATE = 0.5f;

// Swallow Mechanics
public const float SWALLOW_THRESHOLD = 0.85f; // 85% size to swallow
public const float ATTRACTION_RANGE_MULTIPLIER = 2.0f;
public const float TRIGGER_RADIUS_MULTIPLIER = 1.1f;
public const float SWALLOW_DURATION = 0.15f; // seconds

// Movement
public const float BASE_SPEED = 400f;
public const float ACCELERATION = 8f;
public const float DECELERATION = 12f;
public const float MAX_SPEED = 800f;

// Spawning
public const int MIN_OBJECTS = 150;
public const int MAX_OBJECTS = 200;
public const float RESPAWN_RATE_MIN = 0.5f;
public const float RESPAWN_RATE_MAX = 1.5f;

// Timing
public const float GAME_DURATION = 120f; // seconds
public const float COMBO_WINDOW = 2f; // seconds

// Scoring
public const float TIME_BONUS_MULTIPLIER = 10f;
public const float COMBO_BONUS_MULTIPLIER = 50f;
public const float LEVEL_BONUS_MULTIPLIER = 200f;
```

### 9.2 Difficulty Pacing

**Early Game (0-30s):**
- **Goal:** Tutorial and confidence building
- **Object Density:** High (80% tiny objects)
- **Growth Rate:** Fast (easy level-ups)
- **Player Feel:** "I'm getting bigger fast!"

**Mid Game (30-90s):**
- **Goal:** Challenge and skill expression
- **Object Density:** Medium (balanced distribution)
- **Growth Rate:** Moderate (steady progression)
- **Player Feel:** "I need to plan my path"

**End Game (90-120s):**
- **Goal:** Intense climax and mastery
- **Object Density:** High (more large objects)
- **Growth Rate:** Slow (rewarding large swallows)
- **Player Feel:** "Every second counts!"

### 9.3 Score Target Analysis

**Score Distribution (Target):**
```
Beginner (1st play): 500-1000 points
Casual (10th play): 1500-2500 points
Intermediate (50th play): 3000-5000 points
Advanced (100th play): 6000-8000 points
Expert (500th play): 9000-12000 points
Master (top 1%): 12000+ points
```

**Score Breakdown (Typical 5000pt game):**
```
Base Object Score: 3500 (70%)
Time Bonus: 600 (12%)
Combo Bonus: 500 (10%)
Level Bonus: 400 (8%)
```

### 9.4 Tuning Knobs

**Developer Adjustable Parameters:**

| Parameter | Default | Range | Impact |
|-----------|---------|-------|--------|
| Swallow Threshold | 0.85 | 0.7-0.95 | Difficulty |
| Base Speed | 400 | 300-600 | Feel/Skill |
| Growth Rate (all tiers) | 1.0/0.7/0.5 | 0.3-1.5 | Pacing |
| Game Duration | 120s | 60-300s | Session length |
| Combo Window | 2.0s | 1.0-5.0s | Skill ceiling |
| Object Spawn Rate | 2-3/s | 1-5/s | Density |
| Max Objects | 200 | 100-500 | Performance |
| Attraction Strength | 500 | 200-1000 | Feel |

---

## 10. Technical Implementation

### 10.1 System Architecture

```
Game Loop (IThinker.Think):
    1. Update Black Hole Position
    2. Process Input (mouse/touch)
    3. Update Camera Follow
    4. Check Object Triggers
    5. Apply Attraction Forces
    6. Update Swallow Animations
    7. Check Level-Up Conditions
    8. Update UI
    9. Spawn New Objects (if needed)
    10. Update Timer
```

### 10.2 Component Structure

**BlackHoleComponent.cs:**
```csharp
public class BlackHoleComponent : ScriptComponent
{
    private float currentSize;
    private int currentLevel;
    private float attractionRadius;
    private float triggerRadius;

    public override void OnStart()
    {
        // Initialize black hole
        currentSize = 50f;
        currentLevel = 1;
        UpdateRadii();
    }

    public override void OnFixedUpdate(float timeStep)
    {
        // Update position based on input
        UpdateMovement(timeStep);

        // Apply attraction to nearby objects
        ApplyAttractionForces();
    }

    public override void OnTriggerEnter(Node node)
    {
        // Check if can swallow
        SwallowableObject obj = GetObjectData(node);
        if (CanSwallow(obj))
        {
            StartSwallowAnimation(obj);
        }
    }

    private void UpdateRadii()
    {
        triggerRadius = currentSize * 1.1f;
        attractionRadius = currentSize * 2.0f;
        UpdateColliderSize();
    }
}
```

**SwallowableObjectComponent.cs:**
```csharp
public class SwallowableObjectComponent : ScriptComponent
{
    public SwallowableObjectData data;
    private bool isBeingSwallowed;
    private float swallowProgress;

    public override void OnFixedUpdate(float timeStep)
    {
        if (isBeingSwallowed)
        {
            UpdateSwallowAnimation(timeStep);
        }
    }

    public void StartSwallow(Vector3 blackHolePos)
    {
        isBeingSwallowed = true;
        swallowProgress = 0f;

        // Disable physics
        RigidBody rb = node.GetComponent<RigidBody>();
        rb.SetUseGravity(false);
    }

    private void UpdateSwallowAnimation(float timeStep)
    {
        swallowProgress += timeStep / 0.15f;

        if (swallowProgress >= 1f)
        {
            OnSwallowComplete();
        }
        else
        {
            // Lerp position and scale
            node.localScale = Vector3.Lerp(
                Vector3.One,
                Vector3.Zero,
                swallowProgress
            );
        }
    }
}
```

### 10.3 Object Pooling

**ObjectPool.cs:**
```csharp
public class ObjectPool
{
    private Dictionary<ObjectCategory, Queue<PhysicsActor>> pools;
    private const int POOL_SIZE_PER_CATEGORY = 50;

    public void Initialize()
    {
        pools = new Dictionary<ObjectCategory, Queue<PhysicsActor>>();
        foreach (ObjectCategory cat in Enum.GetValues<ObjectCategory>())
        {
            pools[cat] = new Queue<PhysicsActor>();
            PrewarmPool(cat, POOL_SIZE_PER_CATEGORY);
        }
    }

    public PhysicsActor GetObject(SwallowableObjectData data)
    {
        Queue<PhysicsActor> pool = pools[data.Category];
        PhysicsActor actor = pool.Count > 0
            ? pool.Dequeue()
            : CreateNewObject(data);

        ConfigureObject(actor, data);
        return actor;
    }

    public void ReturnObject(PhysicsActor actor, ObjectCategory category)
    {
        actor.GetNode().SetEnabled(false);
        pools[category].Enqueue(actor);
    }
}
```

### 10.4 Spawn Manager

**SpawnManager.cs:**
```csharp
public class SpawnManager
{
    private List<SwallowableObjectData> objectDatabase;
    private ObjectPool objectPool;
    private List<PhysicsActor> activeObjects;
    private float spawnTimer;

    public void Update(float deltaTime, int blackHoleLevel)
    {
        // Check if need to spawn
        if (activeObjects.Count < MIN_OBJECTS)
        {
            SpawnObject(blackHoleLevel);
        }

        // Gradual spawning
        spawnTimer -= deltaTime;
        if (spawnTimer <= 0f)
        {
            if (activeObjects.Count < MAX_OBJECTS)
            {
                SpawnObject(blackHoleLevel);
                spawnTimer = Random.Range(RESPAWN_RATE_MIN, RESPAWN_RATE_MAX);
            }
        }
    }

    private void SpawnObject(int level)
    {
        // Select object based on level distribution
        SwallowableObjectData data = SelectObjectByLevel(level);

        // Find valid spawn position
        Vector3 position = FindValidSpawnPosition(data.Size);

        // Get from pool and activate
        PhysicsActor actor = objectPool.GetObject(data);
        actor.GetNode().position = position;
        actor.GetNode().SetEnabled(true);

        activeObjects.Add(actor);
    }
}
```

### 10.5 Data Configuration

**ObjectDatabase.json:**
```json
{
  "objects": [
    {
      "id": "traffic_cone",
      "displayName": "Traffic Cone",
      "category": "Tiny",
      "shape": "Cone",
      "size": 15,
      "score": 5,
      "growthValue": 2,
      "minLevel": 1,
      "spawnWeight": 1.0,
      "color": "#FF6600",
      "scale": [1.0, 1.0, 1.0]
    },
    {
      "id": "small_box",
      "displayName": "Small Box",
      "category": "Tiny",
      "shape": "Cube",
      "size": 18,
      "score": 8,
      "growthValue": 3,
      "minLevel": 1,
      "spawnWeight": 1.0,
      "color": "#8B4513",
      "scale": [1.0, 1.0, 1.0]
    }
    // ... more objects
  ],
  "spawnDistributions": {
    "level1": {
      "Tiny": 0.8,
      "Small": 0.2
    },
    "level2": {
      "Tiny": 0.5,
      "Small": 0.4,
      "Medium": 0.1
    }
    // ... more levels
  }
}
```

### 10.6 Performance Considerations

**Optimization Targets:**
```
Target FPS: 60 (PC/Web)
Min FPS: 30 (acceptable)
Max Objects: 200 (with pooling)
Draw Calls: <100
Memory Usage: <200MB
Physics Steps: 60 per second (fixed)
```

**Optimization Techniques:**
1. **Object Pooling:** Reuse physics actors
2. **LOD (Level of Detail):** Far objects use simpler collision
3. **Culling:** Only update visible objects
4. **Batching:** Group similar objects for rendering
5. **Fixed Update:** Physics at 60Hz, rendering at variable rate

---

## 11. Audio Design

### 11.1 Sound Effects

**Core Gameplay Sounds:**

| Sound Effect | Trigger | Description |
|--------------|---------|-------------|
| Swallow_Small | Tiny/Small object consumed | Soft "whoosh" sound |
| Swallow_Medium | Medium object consumed | Moderate "gulp" sound |
| Swallow_Large | Large/Huge/Massive consumed | Deep "boom" sound |
| LevelUp | Black hole levels up | Power-up chime |
| Combo_Start | 5-combo reached | Ascending notes |
| Combo_Increase | Combo incremented | Short ping |
| Combo_Break | Combo ended | Descending notes |
| Timer_Warning | 30s remaining | Clock ticking |
| GameOver | Time expires | Dramatic sting |
| Achievement | Achievement unlocked | Reward fanfare |

**Sound Parameters:**
```
Format: WAV or OGG
Sample Rate: 44.1kHz
Bit Depth: 16-bit
Channels: Mono (spatial sounds), Stereo (UI)
Duration: 0.1-1.5 seconds
Volume: Normalized, -6dB headroom
```

### 11.2 Music

**Background Music:**
```
Style: Upbeat electronic/synthwave
Tempo: 120-140 BPM
Duration: 2-3 minute loop
Layers:
  - Drums (constant)
  - Bass (builds over time)
  - Melody (intensifies with level)

Dynamic Music System:
  - Level 1-2: Minimal (drums + bass)
  - Level 3-4: Add melody
  - Level 5-6: Full intensity
  - Last 30s: Add urgency layer
```

### 11.3 Audio Mixing

**Volume Levels:**
```
Master: 100%
Music: 60%
SFX: 80%
UI: 70%

Priority System:
1. UI sounds (highest)
2. Level-up/Achievement
3. Combo sounds
4. Swallow sounds
5. Music (lowest)
```

---

## 12. Polish and Juice

### 12.1 Visual Polish

**Screen Effects:**
```
Screen Shake:
  - Level up: Intensity 0.3, Duration 0.2s
  - Large object swallow: Intensity 0.1, Duration 0.1s

Vignette:
  - Darkens edges as black hole grows
  - Intensity: 0.0 (Level 1) to 0.3 (Level 6)

Chromatic Aberration:
  - Slight effect when moving fast
  - Intensity: Speed / MaxSpeed * 0.02
```

**Particle Effects:**
```
Swallow Particles:
  - Count: 10-30 (based on object size)
  - Color: Object color + purple tint
  - Lifetime: 0.3-0.5s
  - Emission: Radial burst from consumption point

Black Hole Aura:
  - Purple glow particles
  - Orbit around black hole
  - Count increases with level

Level-Up Ring:
  - Expanding circle
  - Color: Purple to white gradient
  - Duration: 0.5s
```

### 12.2 Animation Polish

**Object Animations:**
```
Idle Animation:
  - Slight bobbing (0.1 units up/down)
  - Slow rotation (5 degrees/second)
  - Random start offset

Attraction Animation:
  - Faster rotation as closer to black hole
  - Scale pulsing (0.95-1.05x)
  - Glow intensity increases

Swallow Animation:
  - Spiral path into center
  - Accelerating rotation
  - Scale to zero (ease-in)
```

**UI Animations:**
```
Score Popup:
  - Float upward (50 units)
  - Fade out over 1s
  - Slight scale bounce (1.0 -> 1.2 -> 1.0)

Combo Counter:
  - Pulse on increment (1.0 -> 1.3 -> 1.1 scale)
  - Color flash
  - Particle burst

Progress Bar:
  - Smooth fill animation
  - Glow at fill edge
  - Pulse when near next level
```

### 12.3 Feel and Responsiveness

**Input Responsiveness:**
```
Input Lag: <16ms (1 frame at 60fps)
Mouse Sensitivity: Adjustable (0.5x - 2.0x)
Touch Dead Zone: 5 pixels
Smoothing: Light exponential smoothing (factor 0.8)
```

**Game Feel Techniques:**
```
1. Anticipation: Objects glow before being swallowed
2. Impact: Screen shake + particles on consumption
3. Squash and Stretch: Objects compress toward black hole
4. Follow-Through: Objects overshoot slightly when attracted
5. Timing: All animations tuned to feel snappy (0.1-0.3s)
```

---

## Appendix A: Complete Object Database

### Tiny Objects (30 total types)

| ID | Name | Shape | Size | Score | Growth | Weight |
|----|------|-------|------|-------|--------|--------|
| cone_01 | Traffic Cone | Cone | 15 | 5 | 2 | 1.0 |
| cube_01 | Small Box | Cube | 18 | 8 | 3 | 1.0 |
| sphere_01 | Ball | Sphere | 12 | 6 | 2 | 0.8 |
| cylinder_01 | Bottle | Cylinder | 20 | 10 | 3 | 0.7 |
| sphere_02 | Rock | Sphere | 14 | 7 | 2.5 | 0.9 |
| cone_02 | Mini Pylon | Cone | 16 | 6 | 2.2 | 0.8 |
| cube_02 | Crate | Cube | 22 | 11 | 3.5 | 0.9 |
| cylinder_02 | Can | Cylinder | 13 | 6 | 2 | 1.0 |
| sphere_03 | Apple | Sphere | 11 | 5 | 1.8 | 0.7 |
| cube_03 | Book | Cube | 17 | 7 | 2.8 | 0.8 |
| cone_03 | Party Hat | Cone | 14 | 5 | 2 | 0.6 |
| cylinder_03 | Coffee Cup | Cylinder | 15 | 8 | 2.5 | 0.9 |
| sphere_04 | Orange | Sphere | 12 | 6 | 2 | 0.7 |
| cube_04 | Brick | Cube | 19 | 9 | 3 | 1.0 |
| cylinder_04 | Pipe Section | Cylinder | 21 | 10 | 3.2 | 0.8 |

*[15 more tiny objects...]*

### Small Objects (25 total types)

| ID | Name | Shape | Size | Score | Growth | Weight |
|----|------|-------|------|-------|--------|--------|
| cylinder_05 | Fire Hydrant | Cylinder | 35 | 20 | 5 | 1.0 |
| cube_05 | Mailbox | Cube | 40 | 25 | 6 | 0.9 |
| cylinder_06 | Trash Can | Cylinder | 38 | 22 | 5.5 | 1.0 |
| cube_06 | Bench | Cube | 55 | 35 | 8 | 0.7 |
| cone_04 | Sign Post | Cone | 32 | 18 | 5 | 0.8 |
| cylinder_07 | Barrel | Cylinder | 45 | 28 | 7 | 0.8 |
| cube_07 | Vending Machine | Cube | 50 | 32 | 7.5 | 0.7 |
| cylinder_08 | Gas Pump | Cylinder | 42 | 26 | 6.5 | 0.8 |
| cube_08 | ATM | Cube | 48 | 30 | 7 | 0.7 |
| cone_05 | Traffic Light | Cone | 36 | 21 | 5.5 | 0.9 |

*[15 more small objects...]*

### Medium Objects (20 total types)

| ID | Name | Shape | Size | Score | Growth | Weight |
|----|------|-------|------|-------|--------|--------|
| cube_09 | Small Car | Cube | 75 | 50 | 12 | 1.0 |
| capsule_01 | Motorcycle | Capsule | 65 | 45 | 10 | 0.8 |
| cube_10 | Food Cart | Cube | 70 | 48 | 11 | 0.7 |
| cube_11 | Phone Booth | Cube | 80 | 55 | 13 | 0.6 |
| cone_06 | Large Tree | Cone | 85 | 60 | 14 | 0.5 |
| cube_12 | Dumpster | Cube | 90 | 65 | 15 | 0.7 |
| cube_13 | Small Kiosk | Cube | 72 | 49 | 11.5 | 0.8 |
| cylinder_09 | Water Tank | Cylinder | 78 | 53 | 12.5 | 0.7 |

*[12 more medium objects...]*

### Large Objects (15 total types)

| ID | Name | Shape | Size | Score | Growth | Weight |
|----|------|-------|------|-------|--------|--------|
| cube_14 | Sedan Car | Cube | 120 | 100 | 20 | 1.0 |
| cube_15 | Van | Cube | 135 | 115 | 23 | 0.8 |
| cube_16 | Small Truck | Cube | 140 | 120 | 24 | 0.7 |
| cube_17 | Bus Stop Shelter | Cube | 110 | 90 | 18 | 0.6 |
| cylinder_10 | Small Fountain | Cylinder | 125 | 105 | 21 | 0.5 |
| cube_18 | Kiosk | Cube | 115 | 95 | 19 | 0.7 |

*[9 more large objects...]*

### Huge Objects (12 total types)

| ID | Name | Shape | Size | Score | Growth | Weight |
|----|------|-------|------|-------|--------|--------|
| cube_19 | Large Truck | Cube | 180 | 180 | 35 | 1.0 |
| cube_20 | Bus | Cube | 200 | 220 | 40 | 0.8 |
| cube_21 | Small House | Cube | 190 | 200 | 38 | 0.5 |
| cylinder_11 | Large Fountain | Cylinder | 170 | 170 | 33 | 0.6 |
| cube_22 | Billboard | Cube | 160 | 150 | 30 | 0.7 |
| cube_23 | Container | Cube | 185 | 190 | 36 | 0.8 |

*[6 more huge objects...]*

### Massive Objects (8 total types)

| ID | Name | Shape | Size | Score | Growth | Weight |
|----|------|-------|------|-------|--------|--------|
| cube_24 | Large Building | Cube | 250 | 400 | 60 | 0.5 |
| cylinder_12 | Tower | Cylinder | 280 | 450 | 70 | 0.3 |
| cube_25 | Warehouse | Cube | 240 | 380 | 58 | 0.6 |
| cube_26 | Multi-Story Building | Cube | 270 | 430 | 65 | 0.4 |
| cone_07 | Monument | Cone | 260 | 410 | 62 | 0.3 |

*[3 more massive objects...]*

---

## Appendix B: Formula Reference

### Physics Formulas

```csharp
// Can Swallow Check
bool CanSwallow(object, blackHole)
{
    return object.Size <= (blackHole.Size * SWALLOW_THRESHOLD);
}

// Attraction Force
Vector3 CalculateAttractionForce(object, blackHole)
{
    float distance = Distance(object.position, blackHole.position);
    if (distance >= blackHole.AttractionRadius) return Vector3.Zero;

    Vector3 direction = Normalize(blackHole.position - object.position);
    float t = 1.0f - (distance / blackHole.AttractionRadius);
    float forceMagnitude = ATTRACTION_STRENGTH * (t * t);

    return direction * forceMagnitude;
}

// Growth Calculation
float CalculateGrowth(objectValue, currentTier)
{
    float growthRate = GetGrowthRateForTier(currentTier);
    return objectValue * growthRate;
}

// Score with Combo
int CalculateFinalScore(baseScore, comboCount)
{
    float multiplier = 1.0f;
    if (comboCount >= 20) multiplier = 2.0f;
    else if (comboCount >= 15) multiplier = 1.8f;
    else if (comboCount >= 10) multiplier = 1.5f;
    else if (comboCount >= 5) multiplier = 1.2f;

    return (int)(baseScore * multiplier);
}

// Speed by Level
float GetSpeedMultiplier(level)
{
    return level switch
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
```

---

## Appendix C: Testing Checklist

### Core Mechanics Testing

- [ ] Black hole follows mouse/touch accurately
- [ ] Movement speed feels responsive
- [ ] Camera follows smoothly without jitter
- [ ] Objects are attracted at correct range
- [ ] Swallowing only works for smaller objects
- [ ] Growth increases black hole size visibly
- [ ] Level-up triggers at correct thresholds
- [ ] Combo counter works correctly
- [ ] Timer counts down accurately

### Physics Testing

- [ ] Objects collide with each other
- [ ] Objects don't collide with black hole
- [ ] Gravity affects objects correctly
- [ ] Attraction force feels natural
- [ ] Swallow animation is smooth
- [ ] No objects fall through ground
- [ ] Performance stable at 200 objects

### Balance Testing

- [ ] Early game (0-30s) is easy and fun
- [ ] Mid game (30-90s) is challenging
- [ ] End game (90-120s) is intense
- [ ] Score targets are achievable
- [ ] Leveling pace feels good
- [ ] Combo system is noticeable
- [ ] No exploits or cheese strategies

### UI Testing

- [ ] HUD displays correct information
- [ ] Score updates in real-time
- [ ] Timer is visible and clear
- [ ] Level indicator shows correctly
- [ ] Progress bar fills accurately
- [ ] End screen shows all stats
- [ ] Tutorial is clear and helpful

### Audio Testing

- [ ] All sound effects play correctly
- [ ] Music loops seamlessly
- [ ] Volume levels are balanced
- [ ] No audio clipping or distortion
- [ ] Sounds match visual events

### Polish Testing

- [ ] Particle effects look good
- [ ] Screen shake feels impactful
- [ ] Animations are smooth
- [ ] Color scheme is appealing
- [ ] Visual feedback is immediate
- [ ] Game feels "juicy"

---

## Appendix D: Development Roadmap

### Phase 1: Core Prototype (Week 1-2)

**Deliverables:**
- Basic black hole movement
- Object creation and spawning
- Swallowing trigger system
- Simple growth mechanics
- Camera follow system

**Success Criteria:**
- Black hole moves smoothly
- Can swallow objects
- Objects disappear and black hole grows
- Scene has 50+ objects

### Phase 2: Physics Polish (Week 2-3)

**Deliverables:**
- Attraction force system
- Smooth swallow animations
- Collision layers setup
- Object pooling system
- Performance optimization

**Success Criteria:**
- Objects are pulled toward black hole
- Swallowing looks smooth
- 60 FPS with 200 objects
- No memory leaks

### Phase 3: Progression System (Week 3-4)

**Deliverables:**
- Level system with thresholds
- Score and combo system
- Timer and game mode
- UI/HUD implementation
- Object variety (all categories)

**Success Criteria:**
- Full game loop works
- 2-minute games are fun
- Score system is clear
- Level progression feels good

### Phase 4: Polish and Balance (Week 4-5)

**Deliverables:**
- Visual effects (particles, glow)
- Sound effects and music
- Screen shake and juice
- Balance tuning
- Tutorial system

**Success Criteria:**
- Game feels polished
- Audio enhances experience
- Balance is tested
- Tutorial teaches mechanics

### Phase 5: Final Testing (Week 5-6)

**Deliverables:**
- Bug fixes
- Performance optimization
- User testing feedback
- Final balance adjustments
- Documentation

**Success Criteria:**
- No critical bugs
- Smooth performance
- Positive playtester feedback
- Ready for release

---

## Document History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-10-15 | Game Designer | Initial GDD creation |

---

## References

- WasiCore Physics System Documentation: `C:\Dev\AIProject\project-hole\docs\guides\PhysicsSystem.md`
- PrimitiveShape Test: `C:\Dev\AIProject\project-hole\src\PrimitiveShapeTest\README.md`
- Game Type Guide: `C:\Dev\AIProject\project-hole\docs\GAME_TYPE_GUIDE.md`
- Similar Game: Hole.io (Voodoo, 2018)

---

**END OF GAME DESIGN DOCUMENT**
