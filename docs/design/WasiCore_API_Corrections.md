# WasiCore API Corrections

**Date:** 2025-10-16
**Purpose:** Document correct WasiCore API naming from XML documentation

---

## RigidBody Class

**Namespace:** `EngineInterface.Urho3DInterface`

### ✅ Correct API (All are METHODS, not properties)

```csharp
// Force and velocity
void ApplyForce(Vector3 force)
void SetLinearVelocity(Vector3 velocity)
Vector3 GetLinearVelocity()
void SetAngularVelocity(Vector3 velocity)
Vector3 GetAngularVelocity()

// Mass and damping
void SetMass(float mass)
float GetMass()
void SetLinearDamping(float linearDamping)
float GetLinearDamping()
void SetAngularDamping(float angularDamping)
float GetAngularDamping()

// Gravity
void SetUseGravity(bool flag)
bool GetUseGravity()

// Collision setup
void SetCollisionLayer(uint collisionLayer)
uint GetCollisionLayer()
void SetCollisionMask(uint collisionMask)
uint GetCollisionMask()
void SetCollisionFilter(RigidBody.CollisionFilter filter)
```

### ❌ Common Mistakes to Avoid

```csharp
// WRONG - These don't exist as properties!
rigidBody.IsTrigger = true;  // ❌ No such property
rigidBody.IsKinematic = true; // ❌ No such property
rigidBody.Friction = 0.6f;    // ❌ No such property
rigidBody.Restitution = 0.3f; // ❌ No such property
rigidBody.UseGravity = false; // ❌ Property doesn't exist

// CORRECT - Use methods
rigidBody.SetUseGravity(false); // ✅
// Note: Trigger and Kinematic may need different approach or don't exist
```

---

## Missing APIs (Need Investigation)

The following properties commonly used in Unity/Unreal **may not exist** in WasiCore:

1. **SetTrigger() / IsTrigger** - Not found in XML
   - May need to use collision filter or different approach
   - **Alternative:** Use collision filter returning `true` to ignore collisions

2. **SetKinematic() / IsKinematic** - Not found in XML
   - May need to use SetMass(0) or different approach
   - **Alternative:** Set mass to very high value for static objects

3. **SetFriction()** - Not found in XML
   - May not be exposed in current API

4. **SetRestitution()** - Not found in XML
   - May not be exposed in current API

---

## Node Class

**Namespace:** `EngineInterface.Urho3DInterface`

### Properties

```csharp
// Position
Vector3 position { get; set; }

// Rotation - NEED TO VERIFY EXACT NAME
// Options: rotation, localRotation, worldRotation?
// Quaternion? Euler angles?

// Scale
Vector3 localScale { get; set; }
```

**TODO:** Need to verify exact rotation property name from XML.

---

## Scene System

**Namespace:** `GameCore.SceneSystem`

### Scene.Get() Method

**Need to verify parameter type** - options:
1. `Scene.Get(GameDataScene data)`
2. `Scene.Get(GameLink<GameDataScene> link)`
3. `Scene.Get(string sceneName)`

**TODO:** Search XML for `class Scene` and `Scene.Get`

---

## ScreenViewport

**Namespace:** Need to verify - could be:
- `GameUI.ScreenViewport`
- `GameCore.ScreenViewport`
- `EngineInterface.ScreenViewport`

**TODO:** Search XML for ScreenViewport class

---

## Summary of Required Fixes

### High Priority (Blocking Compilation)

1. **Remove IsTrigger/IsKinematic calls** - Use alternative approaches
2. **Remove Friction/Restitution calls** - Remove or find alternative
3. **Verify Scene.Get() parameter** - Check if it takes GameDataScene or link
4. **Verify Node.rotation property name** - Check exact API
5. **Add correct namespace for ScreenViewport** - Find in XML

### Medium Priority (Nice to have)

1. **Implement trigger behavior without SetTrigger()** - Use collision filter
2. **Implement static objects without SetKinematic()** - Use high mass
3. **Find if friction/restitution can be set** - Check XML thoroughly

---

## Recommended Approach

Since some physics properties may not be exposed, here's the recommended implementation:

### Black Hole (Pass-through trigger)

```csharp
// No SetTrigger available - use collision filter instead
rigidBody.SetUseGravity(false);
rigidBody.SetCollisionLayer(LAYER_BLACKHOLE);
rigidBody.SetCollisionMask(0xFFFFFFFF); // Detect all for OnTriggerEnter

// Key: Filter returns TRUE to IGNORE collision (pass through)
rigidBody.SetCollisionFilter(new RigidBody.CollisionFilter((RigidBody other, Vector3 contact) =>
{
    return true; // Ignore all physical collisions, but still trigger OnTriggerEnter
}));
```

### Static Ground (No SetKinematic available)

```csharp
// No SetKinematic - use very high mass instead
rigidBody.SetMass(1000000f); // Effectively immovable
rigidBody.SetUseGravity(false);
rigidBody.SetLinearDamping(999f); // Prevent any movement
rigidBody.SetAngularDamping(999f);
```

### Swallowable Objects (No friction/restitution)

```csharp
// Just set basic physics without friction/restitution
rigidBody.SetUseGravity(true);
rigidBody.SetMass(mass);
rigidBody.SetLinearDamping(0.05f);
rigidBody.SetAngularDamping(0.1f);
// Accept default friction and restitution
```

---

**Next Steps:**
1. Search XML for Scene, Node, ScreenViewport exact APIs
2. Update implementation code with correct method calls
3. Test compilation
4. Adjust physics behavior if needed

**Last Updated:** 2025-10-16
