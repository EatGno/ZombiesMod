using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using ZombiesMod.Extensions;
using ZombiesMod.Scripts;
using ZombiesMod.Static;
using ZombiesMod.Wrappers;

namespace ZombiesMod.Zombies
{
    public abstract class ZombiePed : Entity, IEquatable<Ped>
    {
        public static int ZombieDamage = 15;
        public static float SensingRange = 120f;
        public static float SilencerEffectiveRange = 15f;
        public static float BehindZombieNoticeDistance = 5f;
        public static float RunningNoticeDistance = 25f;
        public static float AttackRange = 1.2f;
        public static float VisionDistance = 35f;
        public static float WanderRadius = 100f;

        private Ped _target;
        private readonly Ped _ped;
        private EntityEventWrapper _eventWrapper;
        private bool _goingToTarget;
        private bool _attackingTarget;

        public event OnGoingToTargetEvent GoToTarget;
        public event OnAttackingTargetEvent AttackTarget;

        protected ZombiePed(int handle) : base(handle)
        {
            this._ped = new Ped(handle);
            this._eventWrapper = new EntityEventWrapper((Entity)this._ped);
            this._eventWrapper.Died += new EntityEventWrapper.OnDeathEvent(this.OnDied);
            this._eventWrapper.Updated += new EntityEventWrapper.OnWrapperUpdateEvent(this.Update);
            this._eventWrapper.Aborted += new EntityEventWrapper.OnWrapperAbortedEvent(this.Abort);
        }

        public Ped Target
        {
            get { return this._target; }
            private set 
            {
                if (Entity.op_Equality((Entity)value, (Entity)null) && Entity.op_Inequality((Entity)this._target, (Entity)null))
                {
                    this._ped.get_Task().WanderAround(this.get_Position(), WanderRadius);
                    GoingToTarget = AttackingTarget = false;
                }
                this._target = value; 
            }
        }

        public bool GoingToTarget
        {
            get { return _goingToTarget; }
            set 
            {
                if (value && !_goingToTarget)
                {
                    GoToTarget?.Invoke(this.Target);
                }
                _goingToTarget = value; 
            }
        }

        public bool AttackingTarget
        {
            get { return _attackingTarget; }
            set 
            {
                if (value && !this._ped.get_IsRagdoll() && !this.get_IsDead())
                {
                    AttackTarget?.Invoke(this.Target);
                }
                _attackingTarget = value; 
            }
        }

        public abstract void OnAttackTarget(Ped target);
        
        public abstract void OnGoToTarget(Ped target);

        private void OnDied(EntityEventWrapper sender, Entity entity)
        {
            this.get_CurrentBlip()?.Remove();
            // Logique lors de la mort du zombie
        }

        public void Update(EntityEventWrapper entityEventWrapper, Entity entity)
        {
            if (this.get_Position().VDist(Database.PlayerPosition) > 120.0 && (!this.get_IsOnScreen() || this.get_IsDead()))
                this.Delete();

            GetTarget();

            if (Entity.op_Equality((Entity)this.Target, (Entity)null))
                return;

            if (this.get_Position().VDist(((Entity)this.Target).get_Position()) > AttackRange)
            {
                AttackingTarget = false;
                GoingToTarget = true;
            }
            else
            {
                AttackingTarget = true;
                GoingToTarget = false;
            }
        }

        public void Abort(EntityEventWrapper sender, Entity entity)
        {
            this.Delete();
        }

        private void GetTarget()
        {
            Ped closest = World.GetClosest<Ped>(this.get_Position(), World.GetNearbyPeds(this._ped, SensingRange).Where(IsGoodTarget).ToArray());
            
            if (closest != null)
            {
                Target = closest; // Définit la nouvelle cible si elle est valide
            }
            else 
            {
                Target = null; // Oublie la cible si elle n'est plus valide 
            }
        }

        private bool IsGoodTarget(Ped ped)
        {
            return ped.GetRelationshipWithPed(this._ped) == 5; // Vérifie si la relation entre les peds est hostile 
        }

         // Implémentation des méthodes IEquatable<Ped>
    }
}
