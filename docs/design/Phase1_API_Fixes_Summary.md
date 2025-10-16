# Phase 1 API Fixes Summary

**Date:** 2025-10-16
**Status:** ✅ All fixes applied - Compilation successful

---

## Fixed Files

1. `src/BlackHoleGame/BlackHoleController.cs`
2. `src/BlackHoleGame/ObjectSpawner.cs`
3. `src/BlackHoleGame/SwallowableObject.cs`
4. `src/BlackHoleGame/BlackHoleGame.cs`
5. `src/AVGSystem/Engine/AVGScriptEngine.cs` (bonus fix)

---

## API Corrections Applied

### 1. Scene.Get() - Fixed Parameter Type

**Issue:** Incorrect parameter type
**Files:** BlackHoleController.cs (line 40), ObjectSpawner.cs (line 49)

```csharp
// ❌ WRONG
var scene = Scene.Get(GameEntry.ScopeData.Scene.BlackHoleScene.Data);

// ✅ CORRECT
var scene = Scene.Get("blackhole_scene"u8);
```

**Reason:** `Scene.Get()` takes `ReadOnlySpan<byte>` (u8 string) or hash, not GameDataScene.

---

### 2. RigidBody.SetTrigger() - Method Does Not Exist

**Issue:** API method not available
**File:** BlackHoleController.cs (line 63)

```csharp
// ❌ WRONG
rigidBody.SetTrigger(true);

// ✅ CORRECT - Use collision filter instead
rigidBody.SetCollisionFilter(new RigidBody.CollisionFilter((RigidBody other, Vector3 contact) =>
{
    return true; // Return true = ignore collision (pass through)
}));
```

**Reason:** WasiCore RigidBody doesn't have `SetTrigger()`. Use collision filter where returning `true` ignores the collision (allows pass-through while still triggering `OnTriggerEnter`).

---

### 3. RigidBody.SetKinematic() - Method Does Not Exist

**Issue:** API method not available
**File:** ObjectSpawner.cs (line 89)

```csharp
// ❌ WRONG
groundRB.SetKinematic(true);

// ✅ CORRECT - Use very high mass + damping
groundRB.SetMass(1000000f);  // Effectively immovable
groundRB.SetUseGravity(false);
groundRB.SetLinearDamping(999f);
groundRB.SetAngularDamping(999f);
```

**Reason:** WasiCore doesn't have kinematic mode. Simulate with very high mass and damping.

---

### 4. RigidBody.SetFriction() / SetRestitution() - Methods Do Not Exist

**Issue:** API methods not available
**File:** ObjectSpawner.cs (lines 134-135)

```csharp
// ❌ WRONG
rb.SetFriction(0.6f);
rb.SetRestitution(0.3f);

// ✅ CORRECT - Use damping instead
rb.SetLinearDamping(0.1f);
rb.SetAngularDamping(0.2f);
// Accept default friction and restitution
```

**Reason:** Friction and restitution are not exposed in WasiCore RigidBody API. Use damping for physics stability.

---

### 5. Collision Filter - Correct Lambda Syntax

**Issue:** Incorrect instantiation syntax
**Files:** BlackHoleController.cs (line 66), ObjectSpawner.cs (lines 128-131)

```csharp
// ❌ WRONG - Lambda alone
rigidBody.SetCollisionFilter((RigidBody other, Vector3 contact) => true);

// ✅ CORRECT - Must instantiate CollisionFilter object
rigidBody.SetCollisionFilter(new RigidBody.CollisionFilter((RigidBody other, Vector3 contact) =>
{
    return true;
}));
```

**Reason:** Must explicitly instantiate `RigidBody.CollisionFilter` object with lambda.

---

### 6. Node.rotation - Property Does Not Exist

**Issue:** Property not available in API
**File:** SwallowableObject.cs (lines 93-97)

```csharp
// ❌ WRONG
node.rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, angle);

// ✅ CORRECT - Property removed
// Note: Node.rotation property not available in WasiCore API
// Visual rotation effect removed for Phase 1
```

**Reason:** WasiCore Node class doesn't expose rotation property. Available properties: `position`, `localPosition`, `localScale`.

---

### 7. ScreenViewport.Primary - Class Does Not Exist

**Issue:** Wrong namespace/class name
**File:** BlackHoleGame.cs (line 87)

```csharp
// ❌ WRONG
var camera = ScreenViewport.Primary.Camera;

// ✅ CORRECT
var camera = GameUI.Device.DeviceInfo.PrimaryViewport.Camera;
```

**Reason:** Correct API is `DeviceInfo.PrimaryViewport` in namespace `GameUI.Device`.

---

### 8. Task.Delay() - Not Allowed in WebAssembly

**Issue:** WebAssembly incompatibility
**File:** AVGScriptEngine.cs (line 190)

```csharp
// ❌ WRONG
await Task.Delay(100);

// ✅ CORRECT
await Game.Delay(TimeSpan.FromMilliseconds(100));
```

**Reason:** WebAssembly environment requires `Game.Delay()` instead of `Task.Delay()`.

---

## Complete List of WasiCore RigidBody Methods (Verified)

### Available Methods (All are methods, NOT properties):

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

### NOT Available (Do not use):
- ❌ `SetTrigger()` / `IsTrigger`
- ❌ `SetKinematic()` / `IsKinematic`
- ❌ `SetFriction()` / `Friction`
- ❌ `SetRestitution()` / `Restitution`

---

## Compilation Result

✅ **Success!**

```
🎉 WasiCore (SCE/星火编辑器)项目编译成功完成！

📊 编译结果：
  • Server-Debug: 成功
  • Client-Debug: 成功

📁 输出文件：
  • C:\Dev\AIProject\project-hole/AppBundle/managed/GameEntry.dll
  • C:\Dev\AIProject\project-hole/ui/AppBundle/managed/GameEntry.dll

⏱️ 编译耗时：3899ms
```

---

## Key Takeaways

1. **ALWAYS use methods, not properties** for RigidBody configuration
2. **Scene.Get() requires u8 string** like `"scene_name"u8`
3. **Trigger behavior** = Use `SetCollisionFilter()` returning `true` to ignore collisions
4. **Static objects** = Use very high mass (1000000f) + high damping (999f)
5. **Collision filter** = Must instantiate `new RigidBody.CollisionFilter(lambda)`
6. **Camera access** = Use `DeviceInfo.PrimaryViewport.Camera`
7. **Async delays** = Use `Game.Delay(TimeSpan)` not `Task.Delay()`
8. **Node properties** = Only `position`, `localPosition`, `localScale` available

---

## Files Modified

### BlackHoleController.cs
- Line 40: Fixed Scene.Get() parameter
- Lines 58-68: Fixed RigidBody setup (removed SetTrigger, added collision filter)

### ObjectSpawner.cs
- Line 49: Fixed Scene.Get() parameter
- Lines 87-96: Fixed ground RigidBody (removed SetKinematic, use high mass)
- Lines 119-142: Fixed object RigidBody (removed SetFriction/SetRestitution, added damping, fixed collision filter)

### SwallowableObject.cs
- Lines 81-94: Removed Node.rotation usage (not available in API)

### BlackHoleGame.cs
- Line 87: Fixed camera access (DeviceInfo.PrimaryViewport)

### AVGScriptEngine.cs
- Line 190: Fixed Task.Delay to Game.Delay

---

**All Phase 1 implementation files are now API-compliant and compile successfully!**
