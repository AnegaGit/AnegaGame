/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
using System.Text;
using UnityEngine;
using Mirror;
[CreateAssetMenu(menuName = "uMMORPG/Spell/Target Projectile", order = 999)]
public class TargetProjectileSpell : DamageSpell
{
    [Header("Projectile")]
    public ProjectileSpellEffect projectile; // Arrows, Bullets, Fireballs, ...
    // helper function to find the equipped weapon index
    int GetEquipmentWeaponIndex(Entity caster)
    {
        Item item;
        int amount;
        if (caster.inventory.GetEquipment(GlobalVar.equipmentRightHand, out item, out amount))
            if (item.data is WeaponItem)
                return GlobalVar.equipmentRightHand;
        if (caster.inventory.GetEquipment(GlobalVar.equipmentLeftHand, out item, out amount))
            if (item.data is WeaponItem)
                return GlobalVar.equipmentLeftHand;
        return -1;
    }
    bool HasRequiredWeaponAndAmmo(Entity caster)
    {
        int weaponIndex = GetEquipmentWeaponIndex(caster);
        if (weaponIndex != -1)
        {
            // no ammo required, or has that ammo equipped?
            caster.inventory.GetEquipment(weaponIndex, out Item weaponItem, out int weaponAmount);
            WeaponItem itemData = (WeaponItem)weaponItem.data;
            if (itemData.requiredAmmo == null)
                return true;
            if (caster.inventory.GetEquipment(GlobalFunc.OtherHand(weaponIndex), out Item ammoItem, out int ammoAmount))
            {
                //>>>Das ist nicht so toll. requiredAmmo sollte ein ammoType sein und nicht ein spezielles Ammo (mehrere Pfeile je Bogen!)
                if (ammoItem.name == itemData.requiredAmmo.name)
                    return true;
            }
        }
        return false;
    }
    void ConsumeRequiredWeaponsAmmo(Entity caster)
    {
        int weaponIndex = GetEquipmentWeaponIndex(caster);
        if (weaponIndex != -1)
        {
            // no ammo required, or has that ammo equipped?
            caster.inventory.GetEquipment(weaponIndex, out Item weaponItem, out int weaponAmount);
            WeaponItem itemData = (WeaponItem)weaponItem.data;
            if (itemData.requiredAmmo != null)
            {
                if (caster.inventory.GetEquipment(GlobalFunc.OtherHand( weaponIndex), out Item ammoItem, out int ammoAmount))
                {
                    //>>>Das ist nicht so toll. requiredAmmo sollte ein ammoType sein und nicht ein spezielles Ammo (mehrere Pfeile je Bogen!)
                    if (ammoItem.name == itemData.requiredAmmo.name)
                        caster.inventory.DecreaseAmount(GlobalVar.containerEquipment, GlobalFunc.OtherHand(weaponIndex), 1);
                }
            }
        }
    }
    public override bool CheckSelf(Entity caster)
    {
        // check base and ammo
        return base.CheckSelf(caster) &&
               (!requiresWeapon || HasRequiredWeaponAndAmmo(caster));
    }
    public override bool CheckTarget(Entity caster)
    {
        // target exists, alive, not self, oktype?
        return caster.target != null && caster.CanAttack(caster.target);
    }
    public override bool CheckDistance(Entity caster, out Vector3 destination)
    {
        // target still around?
        if (caster.target != null)
        {
            destination = caster.target.collider.ClosestPoint(caster.transform.position);
            return Utils.ClosestDistance(caster.collider, caster.target.collider) <= CastRange(caster);
        }
        destination = caster.transform.position;
        return false;
    }
    public override void Apply(Entity caster)
    {
        // consume ammo if needed
        ConsumeRequiredWeaponsAmmo(caster);
        // spawn the spell effect. this can be used for anything ranging from
        // blood splatter to arrows to chain lightning.
        // -> we need to call an RPC anyway, it doesn't make much of a diff-
        //    erence if we use NetworkServer.Spawn for everything.
        // -> we try to spawn it at the weapon's projectile mount
        if (projectile != null)
        {
            GameObject go = Instantiate(projectile.gameObject, caster.effectMount.position, caster.effectMount.rotation);
            ProjectileSpellEffect effect = go.GetComponent<ProjectileSpellEffect>();
            effect.target = caster.target;
            effect.caster = caster;
            effect.damage = 0;//>>> damage;
            effect.stunChance = stunChance;
            effect.stunTime = 0; //>>> stunTime;
            NetworkServer.Spawn(go);
        }
        else Debug.LogWarning(name + ": missing projectile");
    }
}
