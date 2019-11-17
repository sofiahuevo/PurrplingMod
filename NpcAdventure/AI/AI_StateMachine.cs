using NpcAdventure.AI.Controller;
using NpcAdventure.Loader;
using NpcAdventure.Model;
using NpcAdventure.StateMachine;
using NpcAdventure.Utils;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NpcAdventure.AI
{
    /// <summary>
    /// State machine for companion AI
    /// </summary>
    internal class AI_StateMachine : Internal.IUpdateable
    {
        const int HEAL_COUNTDOWN = 2000;
        public enum State
        {
            FOLLOW,
            FIGHT,
            IDLE,
        }

        private const float MONSTER_DISTANCE = 9f;
        public readonly NPC npc;
        public readonly Farmer player;
        private readonly IModEvents events;
        internal IMonitor Monitor { get; private set; }

        private readonly IContentLoader loader;
        private Dictionary<State, IController> controllers;
        private int changeStateCooldown = 0;
        private int medkits = 3;
        private int healCooldown = 0;
        private bool lifeSaved;

        internal AI_StateMachine(CompanionStateMachine csm, IModEvents events, IMonitor monitor)
        {
            this.npc = csm.Companion;
            this.player = csm.CompanionManager.Farmer;
            this.events = events ?? throw new ArgumentException(nameof(events));
            this.Monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            this.Csm = csm;
            this.loader = csm.ContentLoader;
        }

        public State CurrentState { get; private set; }
        internal IController CurrentController { get => this.controllers[this.CurrentState]; }

        internal CompanionStateMachine Csm { get; }

        public event EventHandler<EventArgsLocationChanged> LocationChanged;

        /// <summary>
        /// Setup AI state machine
        /// </summary>
        public void Setup()
        {
            this.controllers = new Dictionary<State, IController>()
            {
                [State.FOLLOW] = new FollowController(this),
                [State.FIGHT] = new FightController(this, this.loader, this.events, this.Csm.Metadata.Sword),
                [State.IDLE] = new IdleController(this, this.loader),
            };

            // By default AI following the player
            this.ChangeState(State.FOLLOW);

            this.events.GameLoop.TimeChanged += this.GameLoop_TimeChanged;
        }

        private void GameLoop_TimeChanged(object sender, TimeChangedEventArgs e)
        {
            this.lifeSaved = false;
        }

        public bool PerformAction()
        {
            if (this.Csm.HasSkill("doctor") && (this.player.health < this.player.maxHealth / 3) && this.healCooldown == 0 && this.medkits != -1)
            {
                this.TryHealFarmer();
                return true;
            }

            return false;
        }

        private bool TryHealFarmer()
        {
            if (this.medkits > 0 && this.player.health < this.player.maxHealth && this.player.health > 0)
            {
                int healthBonus = (this.player.maxHealth / 100) * (this.player.getFriendshipHeartLevelForNPC(this.npc.Name) / 2); // % health bonus based on friendship hearts
                int health = Math.Max(10, (1 / this.player.health * 10) + Game1.random.Next(0, (int)(this.player.maxHealth * .1f))) + healthBonus;
                this.player.health += health;
                this.healCooldown = HEAL_COUNTDOWN;
                this.medkits--;

                if (this.player.health > this.player.maxHealth)
                    this.player.health = this.player.maxHealth;

                Game1.drawDialogue(this.npc, DialogueHelper.GetDialogueString(this.npc, "heal"));
                Game1.addHUDMessage(new HUDMessage(this.Csm.ContentLoader.LoadString("Strings/Strings:healed", this.npc.displayName, health), HUDMessage.health_type));
                this.Monitor.Log($"{this.npc.Name} healed you! Remaining medkits: {this.medkits}", LogLevel.Info);
                return true;
            }

            if (this.medkits == 0)
            {
                this.Monitor.Log($"No medkits. {this.npc.Name} can't heal you!", LogLevel.Info);
                Game1.drawDialogue(this.npc, DialogueHelper.GetDialogueString(this.npc, "nomedkits"));
                this.medkits = -1;
            }

            return false;
        }

        private void ChangeState(State state)
        {
            this.Monitor.Log($"AI changes state {this.CurrentState} -> {state}");

            if (this.CurrentController != null)
            {
                this.CurrentController.Deactivate();
            }

            this.CurrentState = state;
            this.CurrentController.Activate();
        }

        private bool IsThereAnyMonster()
        {
            return Helper.GetNearestMonsterToCharacter(this.npc, MONSTER_DISTANCE) != null;
        }

        private bool PlayerIsNear()
        {
            return Helper.Distance(this.player.getTileLocationPoint(), this.npc.getTileLocationPoint()) < 11f;
        }

        private void CheckPotentialStateChange()
        {
            if (this.changeStateCooldown == 0 && this.CurrentState != State.FIGHT && this.PlayerIsNear() && this.IsThereAnyMonster())
            {
                this.ChangeState(State.FIGHT);
                this.Monitor.Log("A 50ft monster is here!");
            }

            if (this.CurrentState == State.FIGHT && this.CurrentController.IsIdle)
            {
                this.changeStateCooldown = 100;
                this.ChangeState(State.FOLLOW);
            }

            if (this.CurrentState == State.FOLLOW && this.CurrentController.IsIdle)
            {
                this.ChangeState(State.IDLE);
            }

            if (this.CurrentState == State.IDLE && this.CurrentController.IsIdle)
            {
                this.ChangeState(State.FOLLOW);
            }
        }

        public void Update(UpdateTickedEventArgs e)
        {
            if (e.IsMultipleOf(15))
            {
                this.CheckPotentialStateChange();
            }

            if (this.changeStateCooldown > 0)
                this.changeStateCooldown--;

            // Countdown to companion can heal you if heal cooldown greather than zero
            if (this.healCooldown > 0 && Context.IsPlayerFree)
            {
                // Every 3 seconds while countdown add 1% of maxhealth to player's health (Companion heal side-effect) 
                // Take effect when cooldown half way though and player's health is lower than 60% of maxhealth
                // Adds count of friendship hearts as health bonus
                if (e.IsMultipleOf(180) && (this.healCooldown > HEAL_COUNTDOWN / 2) && this.player.health < (this.player.maxHealth * .6f))
                    this.player.health += Math.Max(1, (int)Math.Round(this.player.maxHealth * .01f)) + (int)Math.Round(this.player.getFriendshipHeartLevelForNPC(this.npc.Name) / 4f);

                this.healCooldown--;
            }

            // Doctor companion try to save your life if you have less than 5% of health and your life not saved in last time
            if (e.IsOneSecond && this.Csm.HasSkill("doctor") && this.medkits > 0 && this.player.health < this.player.maxHealth * 0.05 && !this.lifeSaved)
                this.TrySaveLife();

            if (this.CurrentController != null)
                this.CurrentController.Update(e);
        }

        /// <summary>
        /// Try to save player's life when player is in dangerous with first aid medikit.
        /// There are chance based on player's luck level and daily luck to life will be saved or not
        /// Can try save life when NPC and any monster are near to player
        /// </summary>
        private void TrySaveLife()
        {
            float npcPlayerDistance = Helper.Distance(this.player.GetBoundingBox().Center, this.npc.GetBoundingBox().Center);
            bool noMonstersNearPlayer = Helper.GetNearestMonsterToCharacter(this.player, 4f) == null;

            if (this.player.health <= 0 || npcPlayerDistance > 2.25 * Game1.tileSize || noMonstersNearPlayer)
                return;

            double chance = Math.Max(0.01, (Game1.dailyLuck / 2.0 + this.player.LuckLevel / 100.0 + this.player.getFriendshipHeartLevelForNPC(this.npc.Name) * 0.05));
            double random = Game1.random.NextDouble();
            this.Monitor.Log($"{this.npc.Name} try to save your poor life. Chance is: {chance}/{1.0 - chance}, Random pass: {random}");

            if (random <= chance || random >= (1.0 - chance))
            {
                this.lifeSaved = this.TryHealFarmer();

                if (this.lifeSaved)
                {
                    Game1.showGlobalMessage(this.loader.LoadString("Strings/Strings:lifeSaved"));
                    this.Monitor.Log($"{this.npc.Name} saved your life!", LogLevel.Info);
                }
            }
        }

        public void ChangeLocation(GameLocation l)
        {
            GameLocation previousLocation = this.npc.currentLocation;
            
            // Warp NPC to player's location at theirs position
            Helper.WarpTo(this.npc, l, this.player.getTileLocationPoint());

            // Fire location changed event
            this.OnLocationChanged(previousLocation, this.npc.currentLocation);
        }

        private void OnLocationChanged(GameLocation previous, GameLocation next)
        {
            EventArgsLocationChanged args = new EventArgsLocationChanged()
            {
                PreviousLocation = previous,
                CurrentLocation = next,
            };

            this.LocationChanged?.Invoke(this, args);
        }

        public void Dispose()
        {
            this.events.GameLoop.TimeChanged -= this.GameLoop_TimeChanged;
            this.CurrentController.Deactivate();
            this.controllers.Clear();
            this.controllers = null;
        }
    }
}
