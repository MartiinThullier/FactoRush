﻿using System.Collections.Generic;
using UnityEngine;

namespace GameContent.Entities.UnmanagedEntities.Scriptables.Weapons
{
    [CreateAssetMenu(fileName = "WeaponMinigun", menuName = "Components/WeaponComponents/WeaponMinigun")]
    public sealed class WeaponMinigun : WeaponComponent
    {
        
        [Header("Effect Parameters")]
        public float MinigunSpeedMultiplier = 0.9f; // % speed multiplier per attack
        
        #region Unique Effects Handlers
        protected override void HandleUniqueEffect(Unit attacker, Unit target, List<Unit> allUnitsInRange)
        {
            // Gain 10% attack speed per attack, up to 50%. Resets on change position
            
            Unit priorityTarget = null;
            
            foreach (var unit in allUnitsInRange)
            {
                if (unit.weaponComponent != null && unit.weaponComponent.isAPriorityTarget)
                {
                    priorityTarget = unit;
                    break;
                }
            }
            
            if (priorityTarget != null)
            {
                priorityTarget.ApplyDamage(attacker.damage);
            }
            else
            {
                target.ApplyDamage(attacker.damage);
            }
            
            attacker.attackSpeed = Mathf.Max(0.5f, attacker.attackSpeed * MinigunSpeedMultiplier); // Increase attack speed
            
        }
        
        #endregion
        
    }
    
}