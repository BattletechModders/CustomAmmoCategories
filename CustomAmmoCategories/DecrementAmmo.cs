using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using BattleTech;
using BattleTech.AttackDirectorHelpers;
using System.Reflection;
using CustAmmoCategories;
using UnityEngine;

namespace CustAmmoCategories
{
    public static partial class CustomAmmoCategories
    {
        public static void ReturnNoFireHeat(Weapon weapon, int stackItemUID, int numSuccesHits)
        {
            CustomAmmoCategoriesLog.Log.LogWrite("Returining heat\n");
            int numNeedShots = weapon.ShotsWhenFired * weapon.ProjectilesPerShot;
            CustomAmmoCategoriesLog.Log.LogWrite(" Needed shots " + numNeedShots + "\n");
            CustomAmmoCategoriesLog.Log.LogWrite(" Success shots " + numSuccesHits + "\n");
            float returnHeat = 0.0f - ((float)(numNeedShots - numSuccesHits) * weapon.HeatGenerated) / (float)numNeedShots;
            CustomAmmoCategoriesLog.Log.LogWrite(" Heat to return " + (int)returnHeat + "\n");
            weapon.parent.AddWeaponHeat(weapon, (int)returnHeat);
        }
        public static int DecrementAmmo(Weapon instance, int stackItemUID, int StreakHitCount)
        {
            int shotsWhenFired = instance.ShotsWhenFired;
            if(StreakHitCount != 0)
            {
                shotsWhenFired = StreakHitCount / instance.ProjectilesPerShot;
            }
            bool streakEffect = CustomAmmoCategories.getExtWeaponDef(instance.weaponDef.Description.Id).StreakEffect;
            if((streakEffect == false)&&(StreakHitCount == 0))
            {
                StreakHitCount = shotsWhenFired;
            }
            CustomAmmoCategoriesLog.Log.LogWrite("Weapon.DecrementAmmo " + instance.UIName + " real fire count:" + StreakHitCount + "\n");
            int result = 0;
            if (instance.AmmoCategory == AmmoCategory.NotSet || (instance.parent != null && instance.parent is Turret))
            {
                if (instance.weaponDef.ComponentTags.Contains("wr-clustered_shots"))
                {
                    result = shotsWhenFired;
                }
                else
                {
                    result = shotsWhenFired * instance.ProjectilesPerShot;
                }
                CustomAmmoCategoriesLog.Log.LogWrite("  weapon has no ammo (energy or turret) " + instance.UIName + "\n");
                return result;
            }
            int modValue;
            if (instance.InternalAmmo >= shotsWhenFired)
            {
                if (StreakHitCount != 0) instance.StatCollection.ModifyStat<int>(instance.uid, stackItemUID, "InternalAmmo", StatCollection.StatOperation.Int_Subtract, shotsWhenFired, -1, true);
                modValue = 0;
            }
            else
            {
                modValue = shotsWhenFired - instance.InternalAmmo;
                if (StreakHitCount != 0) instance.StatCollection.ModifyStat<int>(instance.uid, stackItemUID, "InternalAmmo", StatCollection.StatOperation.Set, 0, -1, true);
            }
            string CurrentAmmoId = "";
            if (CustomAmmoCategories.checkExistance(instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true)
            {
                CurrentAmmoId = instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
            }
            //ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
            //ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(__instance.weaponDef.Description.Id);
            if (modValue == 0)
            {
                if (instance.weaponDef.ComponentTags.Contains("wr-clustered_shots"))
                {
                    result = shotsWhenFired;
                }
                else
                {
                    result = shotsWhenFired * instance.ProjectilesPerShot;
                }
                CustomAmmoCategoriesLog.Log.LogWrite("  fire internal ammo. projectiles:" + result + "\n");
                return result;
            }
            for (int index = 0; index < instance.ammoBoxes.Count; ++index)
            {
                AmmunitionBox ammoBox = instance.ammoBoxes[index];
                if (ammoBox.IsFunctional == false) { continue; }
                if (ammoBox.CurrentAmmo <= 0) { continue; }
                if ((string.IsNullOrEmpty(CurrentAmmoId) == false) && (ammoBox.ammoDef.Description.Id != CurrentAmmoId)) { continue; }
                if (ammoBox.CurrentAmmo >= modValue)
                {
                    if (StreakHitCount != 0) ammoBox.StatCollection.ModifyStat<int>(instance.uid, stackItemUID, "CurrentAmmo", StatCollection.StatOperation.Int_Subtract, modValue, -1, true);
                    modValue = 0;
                }
                else
                {
                    modValue -= ammoBox.CurrentAmmo;
                    if (StreakHitCount != 0) ammoBox.StatCollection.ModifyStat<int>(instance.uid, stackItemUID, "CurrentAmmo", StatCollection.StatOperation.Set, 0, -1, true);
                }
            }
            if (instance.weaponDef.ComponentTags.Contains("wr-clustered_shots"))
            {
                result = (instance.ShotsWhenFired - modValue);
            }
            else
            {
                result = (instance.ShotsWhenFired - modValue) * instance.ProjectilesPerShot;
            }
            CustomAmmoCategoriesLog.Log.LogWrite("  fire exteranl ammo. projectiles:" + result + "\n");
            return result;
        }
    }
}
