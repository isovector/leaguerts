on UnitDied: void Common_Effects_blood(Unit u) {
    ParticleEmitter.CreateTimedParticleEmitter("blood", 40.0f, 0.75f, u.Position);
}

declare ParticleSystem("blood"): void Common_Particle_fire(ParticleSettings settings) {
    settings.TextureName = "textures/particles/fire";

    settings.MaxParticles = 2400;

    settings.Duration = TimeSpan.FromSeconds(1);

    settings.DurationRandomness = 1;

    settings.MinHorizontalVelocity = 0;
    settings.MaxHorizontalVelocity = 5;

    settings.MinVerticalVelocity = 15;
    settings.MaxVerticalVelocity = 15;

    // Set gravity upside down, so the flames will 'fall' upward.
    settings.Gravity = new Vector3(0, -15, 0);

    settings.MinColor = new Color(255, 255, 255, 10);
    settings.MaxColor = new Color(255, 255, 255, 40);

    settings.MinStartSize = 5;
    settings.MaxStartSize = 10;

    settings.MinEndSize = 10;
    settings.MaxEndSize = 40;

    // Use additive blending.
    settings.SourceBlend = Blend.SourceAlpha;
    settings.DestinationBlend = Blend.One;
}

declare ParticleSystem("bluefire"): void Common_Particle_bluefire(ParticleSettings settings) {
    settings.TextureName = "textures/jamie";

    settings.MaxParticles = 2400;

    settings.Duration = TimeSpan.FromSeconds(2);

    settings.DurationRandomness = 1;

    settings.MinHorizontalVelocity = 0;
    settings.MaxHorizontalVelocity = 15;

    settings.MinVerticalVelocity = -10;
    settings.MaxVerticalVelocity = 10;

    // Set gravity upside down, so the flames will 'fall' upward.
    settings.Gravity = new Vector3(0, 15, 0);

    settings.MinColor = new Color(0, 0xCC, 0xFF, 10);
    settings.MaxColor = new Color(50, 0xCC, 0xFF, 40);

    settings.MinStartSize = 5;
    settings.MaxStartSize = 10;

    settings.MinEndSize = 10;
    settings.MaxEndSize = 40;

    // Use additive blending.
    settings.SourceBlend = Blend.SourceAlpha;
    settings.DestinationBlend = Blend.One;
}

on ProjectileDestroyed("ppar"): void Common_ProjectileDestroy_ppar(Projectile proj, string dat) {
    GameObject go = GameObject.CreateGameObject(proj.Position);
    ParticleEmitter.CreateParticleEmitter(dat, 100.0f, go);
    Thread.Sleep(500);
    League.Engine.Components.Remove(go);
}