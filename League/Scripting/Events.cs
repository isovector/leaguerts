using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace LeagueEngine.Scripting {
    /// <summary>
    /// A Specific event in the scripting engine
    /// </summary>
    public abstract class Specific : Attribute {
        public object Data;

        public Specific(object arg) { Data = arg; }
    }

    /// <summary>
    /// Async. Called on every update. Signature = void(GameTime)
    /// </summary>
    public class UpdateAttribute : Attribute { }
    /// <summary>
    /// Async. Called on map initialization. Signature = void()
    /// </summary>
    public class MapInitAttribute : Attribute {}
    /// <summary>
    /// Async. Called on a unit being created. Signature = void(Unit)
    /// </summary>
    public class UnitCreatedAttribute : Attribute {}
    /// <summary>
    /// Async. Called when a unit dies. Signature = void(Unit)
    /// </summary>
    public class UnitDiedAttribute : Attribute {}
    /// <summary>
    /// Async. Called when a unit is removed by a process other than dying. Signature = void(Unit)
    /// </summary>
    public class UnitRemovedAttribute : Attribute { }
    /// <summary>
    /// Async. Called when a unit is attacked. Signature = void(Unit, Unit) // attacker, attacked
    /// </summary>
    public class UnitAttackedAttribute : Attribute { }
    /// <summary>
    /// Async. Called once after a specific amount of time (seconds in float). Signature = void()
    /// </summary>
    public class TimeElapsedAttribute : Specific { public TimeElapsedAttribute(float time) : base(time) { } }
    /// <summary>
    /// Async. Called every specific seconds (as float). Signature = void()
    /// </summary>
    public class PeriodicAttribute : Specific { public PeriodicAttribute(float time) : base(time) { } }
    /// <summary>
    /// Async. Called when a unit enters a specific region (as string). Signature = void(Unit)
    /// </summary>
    public class RegionEnteredAttribute : Specific { public RegionEnteredAttribute(string region) : base(region) { } }
    /// <summary>
    /// Async. Called when a unit leaves a specific region (as string). Signature = void(Unit)
    /// </summary>
    public class RegionLeftAttribute : Specific { public RegionLeftAttribute(string region) : base(region) { } }
    /// <summary>
    /// Sync. Called when a specific ability code is invoked (as string). Signature = bool(Ability, Unit, object)
    /// </summary>
    public class AbilityCastAttribute : Specific { public AbilityCastAttribute(string aid) : base(aid) { } }
    /// <summary>
    /// Sync. Called when a specific ability code is smarted (as string). Signature = bool(Ability, Unit, object)
    /// </summary>
    public class AbilitySmartAttribute : Specific { public AbilitySmartAttribute(string aid) : base(aid) { } }
    /// <summary>
    /// Sync. Called when a specific particle system is being created (as string). Signature = void(ParticleSettings)
    /// </summary>
    public class ParticleSystemAttribute : Specific { public ParticleSystemAttribute(string vid) : base(vid) { } }
    /// <summary>
    /// Async. Called when a projectile with a specific magic (as string) is created. Signature = void(Projectile)
    /// </summary>
    public class ProjectileCreatedAttribute : Specific { public ProjectileCreatedAttribute(string mid) : base(mid) { } }
    /// <summary>
    /// Async. Called when a projectile with a specific magic (as string) is destroyed. Signature = void(Projectile)
    /// </summary>
    public class ProjectileDestroyedAttribute : Specific { public ProjectileDestroyedAttribute(string mid) : base(mid) { } }
    /// <summary>
    /// Sync. Called at initialization to create Resources
    /// </summary>
    public class ResourceAttribute : Attribute { }
}
