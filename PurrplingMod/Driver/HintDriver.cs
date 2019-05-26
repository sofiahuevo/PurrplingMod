using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;

namespace PurrplingMod.Driver
{
    public class HintDriver
    {
        public enum Hint
        {
            NONE,
            DIALOGUE,
            GIFT,
        }

        public event EventHandler<CheckHintArgs> CheckHint;

        public Hint ShowHint { get; set; }
        public ICursorPosition CursorPosition { get; private set; }
        public HintDriver(IModHelper helper)
        {
            helper.Events.Input.CursorMoved += this.Input_CursorMoved;
            helper.Events.GameLoop.UpdateTicking += this.Update;
        }

        public void ResetHint()
        {
            this.ShowHint = Hint.NONE;
        }

        private void Input_CursorMoved(object sender, CursorMovedEventArgs e)
        {
            this.CursorPosition = e.NewPosition;

            if (!Context.IsWorldReady)
                return;

            Vector2 cursorTile = e.OldPosition.Tile;
            GameLocation location = Game1.currentLocation;
            NPC n = location.isCharacterAtTile(cursorTile);

            if (n == null)
            {
                // Try next Y position if no NPC fetched
                n = location.isCharacterAtTile(cursorTile + new Vector2(0f, 1f));
                if (n == null)
                    this.ResetHint();
            }

            this.OnCheckHint(n);
        }

        private void Update(object sender, UpdateTickingEventArgs e)
        {

            if (!Context.IsWorldReady)
                return;

            switch (this.ShowHint)
            {
                case Hint.DIALOGUE:
                    Game1.mouseCursor = 4;
                    break;
                case Hint.GIFT:
                    Game1.mouseCursor = 3;
                    break;
                default:
                    Game1.mouseCursor = 0;
                    break;
            }

            if (this.ShowHint != Hint.NONE)
            {
                Vector2 tileLocation = this.CursorPosition.Tile;
                Game1.mouseCursorTransparency = !Utility.tileWithinRadiusOfPlayer((int)tileLocation.X, (int)tileLocation.Y, 1, Game1.player) ? 0.5f : 1f;
            }

            Game1.updateCursorTileHint();
        }

        private void OnCheckHint(NPC forNPC)
        {
            if (this.CheckHint == null)
                return;

            CheckHintArgs args = new CheckHintArgs()
            {
                Npc = forNPC,
            };

            this.CheckHint(this, args);
        }
    }

    public class CheckHintArgs
    {
        public NPC Npc { get; set; }
    }
}
