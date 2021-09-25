﻿using EntityStates.Commando.CommandoWeapon;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using static AncientScepter.SkillUtil;

namespace AncientScepter
{
    public class CommandoBarrage2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Fires twice as many bullets, twice as fast, with twice the accuracy.</color>";

        public override string targetBody => "CommandoBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 0;

        internal override void SetupAttributes()
        {
            var oldDef = Resources.Load<SkillDef>("skilldefs/commandobody/CommandoBodyBarrage");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_COMMANDO_BARRAGENAME";
            newDescToken = "ANCIENTSCEPTER_COMMANDO_BARRAGEDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Death Blossom";
            LanguageAPI.Add(nametoken, namestr);
            //TODO: fire auto-aim bullets at every enemy in range

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("texCommandoR1");

            LoadoutAPI.AddSkillDef(myDef);
        }

        internal override void LoadBehavior()
        {
            On.EntityStates.Commando.CommandoWeapon.FireBarrage.OnEnter += On_FireBarrage_Enter;
            On.EntityStates.Commando.CommandoWeapon.FireBarrage.FireBullet += On_FireBarrage_FireBullet;
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.Commando.CommandoWeapon.FireBarrage.OnEnter -= On_FireBarrage_Enter;
            On.EntityStates.Commando.CommandoWeapon.FireBarrage.FireBullet -= On_FireBarrage_FireBullet;
        }

        private void On_FireBarrage_Enter(On.EntityStates.Commando.CommandoWeapon.FireBarrage.orig_OnEnter orig, FireBarrage self)
        {
            orig(self);
            if (AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0)
            {
                self.durationBetweenShots /= 2f;
                self.bulletCount *= 2;
            }
        }

        private void On_FireBarrage_FireBullet(On.EntityStates.Commando.CommandoWeapon.FireBarrage.orig_FireBullet orig, FireBarrage self)
        {
            bool hasScep = AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0;
            var origAmp = FireBarrage.recoilAmplitude;
            if (hasScep)
            {
                FireBarrage.recoilAmplitude /= 2;

            }
            orig(self);
            FireBarrage.recoilAmplitude = origAmp;
        }
    }
}