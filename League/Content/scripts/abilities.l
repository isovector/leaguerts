// Change unit type
on AbilityCast("acha"): bool Common_AbilityCast_acha(Ability ability, Unit caster, object target) {
    int count = 0;
    for (int i = 0; i < 4; i++)
        if (ability.Data[i] != null && ability.Data[i] != "")
            count++;

    UnitType type = UnitType.GetUnitType(ability.Data[League.Random.Next(0, count)]);

    float hp = caster.HpPercentage;
    float en = caster.EnergyPercentage;
   
    string uid = caster.Type.Uid;
    caster.Owner.AddAsset(type.Uid);
    caster.Type = type;
    caster.HpPercentage = hp;
    caster.EnergyPercentage = en;
    caster.Owner.RemoveAsset(uid);
    
    return true;
}

// Harvest resources
on AbilityCast("ahar"): bool Common_AbilityCast_ahar(Ability ability, Unit caster, object target) {
    if (!caster.Tags.ContainsKey("ahar:resources")) {
        caster.Tags.Add("ahar:resources", new object[] {ability.Data[0], int.Parse(ability.Data[1]), target});
        ParticleEmitter.CreateTimedParticleEmitter("bluefire", 20, 0.5f, (GameObject)target);
    }
        
    Unit townhall = null;
    foreach (Unit u in League.Engine.Units)
        if (u.Type.Uid == ability.Data[2] && u.Owner == caster.Owner) {
            townhall = u;
            break;
        }

    if (townhall != null) {
        caster.OrderCast("aret", townhall);
        return true;
    }

    return false;
}

on AbilitySmart("ahar"): bool Common_AbilitySmart_ahar(Ability ability, Unit caster, object target) {
    if (!caster.Tags.ContainsKey("ahar:resources")) {
        if (target is Unit && (target as Unit).Type.Uid == "rmin") {
            caster.OrderCast("ahar", (Unit)target);
            return true;
        }
    } else {
        if (target is Unit && (target as Unit).Type.Uid == ability.Data[2]) {
            caster.OrderCast("aret", (Unit)target);
            return true;
        }
    }
        
    return false;
}

on AbilityCast("aret"): bool Common_AbilityCast_aret(Ability ability, Unit caster, object target) {
    if (caster.Tags.ContainsKey("ahar:resources")) {
        object[] data = (object[])caster.Tags["ahar:resources"];
        caster.Owner.AddResource((string)data[0], (int)data[1]);
        caster.OrderCast("ahar", (Unit)data[2]);
        caster.Tags.Remove("ahar:resources");
    }
    return true;
}