using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using System.Collections.Generic;

namespace PurrplingMod
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        public Point myLastLocationTile;
        public Point abbyLastPositionTile;
        public Point targetPositionTile;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Player.Warped += this.Player_Warped;
            helper.Events.GameLoop.DayStarted += this.GameLoop_DayStarted;
            helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;

        }

        private void GameLoop_UpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            NPC abigail = Game1.getCharacterFromName("Abigail");
            Farmer player = Game1.player;

            if (abigail == null || player == null)
                return;

            if (player.currentLocation is MineShaft)
                return;

            bool playerIsFar = this.isFar(player.currentLocation, abigail.currentLocation, player.getTileLocationPoint(), abigail.getTileLocationPoint());
            bool targetIsFar = this.isFar(player.currentLocation, abigail.currentLocation, player.getTileLocationPoint(), this.targetPositionTile);

            if (abigail.controller == null && (playerIsFar || targetIsFar))
                // No planned path and player or target is far => find new path and got it
                this.comeToMe(player.currentLocation, player, abigail);

            if (player.getTileLocationPoint() != this.myLastLocationTile || targetIsFar)
            {
                if (targetIsFar)
                    this.comeToMe(player.currentLocation, player, abigail); // Player position changed, change target
                this.haltWhenPlayerIsNear(abigail, player.currentLocation, abigail.currentLocation, player.getTileLocationPoint(), abigail.getTileLocationPoint());
                //this.Monitor.Log($"Current position tile: {player.getTileLocationPoint()} Last: {this.myLastLocationTile} Face: {player.getFacingDirection()} Location: {player.currentLocation.Name}");
                this.myLastLocationTile = player.getTileLocationPoint();
            }

            if (abigail.currentLocation != player.currentLocation)
                // Player and Abby game location is mismatch => warp Abby to player
                this.spawnAbigailHere(player.currentLocation, player.getTileLocationPoint());

            if (abigail.getTileLocationPoint() != this.abbyLastPositionTile)
            {
                this.Monitor.Log($"Abby position: {abigail.getTileLocationPoint()} Location: {abigail.currentLocation.Name}");
                this.abbyLastPositionTile = abigail.getTileLocationPoint();
            }

            Point abbyPos = abigail.getTileLocationPoint();
            Point playerPos = player.getTileLocationPoint();
            float distance = Utility.distance(abbyPos.X, playerPos.X, abbyPos.Y, playerPos.Y);

            if (distance > 7)
                abigail.Speed = abigail.speed = 4;
            else if (distance > 14)
                abigail.Speed = abigail.speed = 8;
            else if (distance > 20)
                abigail.Speed = abigail.speed = 12;
            else if (distance > 28)
                abigail.setTilePosition(player.getTileLocationPoint());
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            GameLocation location = Game1.player.currentLocation;
            Farmer player = Game1.player;
            this.myLastLocationTile = player.getTileLocationPoint();
            this.spawnAbigailHere(location, player.getTileLocationPoint());
        }

        private void spawnAbigailHere(GameLocation location, Point locationTilePoint)
        {
            NPC abigail = Game1.getCharacterFromName("Abigail");

            if (abigail != null && abigail.currentLocation != location)
            {
                abigail.controller = null;
                abigail.Halt();
                abigail.currentLocation.characters.Remove(abigail);
                abigail.currentLocation = location;
                location.addCharacter(abigail);
                abigail.setTilePosition(locationTilePoint);
                abigail.setNewDialogue("Meow");
                this.abbyLastPositionTile = abigail.getTileLocationPoint();
            }
        }

        private List<Point> getNearPoints(Point p, int distance)
        {
            List<Point> points = new List<Point>();
            for (int x = p.X - distance; x <= p.X + distance; x++)
            {
                for (int y = p.Y - distance; y <= p.Y + distance; y++)
                {
                    if (x == p.X && y == p.Y)
                        continue;
                    points.Add(new Point(x, y));
                }
            }

            return points;
        }

        private bool isFar(GameLocation playerGameLocation, GameLocation npcGameLocation, Point playerLocationTile, Point npcLocationTile, float distance = 3)
        {
            if (playerGameLocation != npcGameLocation)
                return true;

            return Utility.distance(playerLocationTile.X, npcLocationTile.X, playerLocationTile.Y, npcLocationTile.Y) > distance;
        }

        private List<Tuple<Point, float>> getNearPointsWithDistance(List<Point> nearPoints, Point startTilePoint)
        {
            List<Tuple<Point, float>> nearPointsWithDistance = new List<Tuple<Point, float>>();

            foreach (Point nearPoint in nearPoints)
            {
                nearPointsWithDistance.Add(new Tuple<Point, float>(nearPoint, Utility.distance(nearPoint.X, startTilePoint.X, nearPoint.Y, startTilePoint.Y)));
            }

            return nearPointsWithDistance;
        }

        private List<Point> sortPointsByNearest(List<Point> nearPoints, Point startTilePoint)
        {
            List<Tuple<Point, float>> nearPointsWithDistance = this.getNearPointsWithDistance(nearPoints, startTilePoint);

            nearPointsWithDistance.Sort(delegate (Tuple<Point, float> p1, Tuple<Point, float> p2) {
                if (p1.Item2 == p2.Item2)
                {
                    return 0;
                }

                return -1 * p1.Item2.CompareTo(p2.Item2);
            });

            return nearPointsWithDistance.ConvertAll<Point>(
                new Converter<Tuple<Point, float>, Point>(
                    delegate (Tuple<Point, float> tp)
                    {
                        return tp.Item1;
                    }
                )
            );
        }

        private Stack<Point> FindPath(Point startTilePoint, Stack<Point> nearPointsStack, GameLocation location, NPC n, int limit = 100)
        {
            Stack<Point> path;
            PathFindController.isAtEnd endFunction = new PathFindController.isAtEnd(PathFindController.isAtEndPoint);

            while (nearPointsStack.Count > 0)
            {
                this.Monitor.Log($"Trying to find path... Attampts left: {nearPointsStack.Count}");
                Point target = nearPointsStack.Pop();
                path = PathFindController.findPath(startTilePoint, target, endFunction, location, n, limit);
                if (path != null)
                {
                    this.Monitor.Log($"Target changed, path reconstructed! Target: {target}");
                    this.targetPositionTile = target;
                    return path;
                }
            }

            this.Monitor.Log("No path found!");

            return null;
        }

        private void haltWhenPlayerIsNear(NPC n, GameLocation playerGameLocation, GameLocation npcGameLocation, Point playerTilePosition, Point npcTilePosition)
        {
            if (n.controller == null)
                return;
            if (n.controller.pathToEndPoint == null || this.isFar(playerGameLocation, npcGameLocation, playerTilePosition, npcTilePosition))
                return;

            n.controller = null;
            n.Halt();
            this.Monitor.Log("Is near to player, halt");
        }

        private void comeToMe(GameLocation location, Farmer player, NPC n)
        {
            Point playerTilePosition = player.getTileLocationPoint();
            Point npcTilePosition = n.getTileLocationPoint();
            Stack<Point> nearPoints;

            if (!this.isFar(player.currentLocation, n.currentLocation, playerTilePosition, npcTilePosition))
                return;

            nearPoints = new Stack<Point>(
                this.sortPointsByNearest(
                    this.getNearPoints(player.getTileLocationPoint(), 1), 
                    npcTilePosition
                )
            );

            if (n.controller == null)
            {
                Point target = nearPoints.Pop();
                n.temporaryController = null;
                n.Halt();
                n.controller = new PathFindController(n, location, target, 0);
                if (n.controller.pathToEndPoint == null)
                {
                    n.controller.pathToEndPoint = this.FindPath(npcTilePosition, nearPoints, location, n);
                } else
                {
                    this.targetPositionTile = target;
                }
            } else
            {
                n.Halt();
                n.controller.pathToEndPoint = this.FindPath(npcTilePosition, nearPoints, location, n);
                this.Monitor.Log("Target missmatch, path reconstructed!");
            }

            /*if (player.isMoving())
            {
                this.Monitor.Log("Halt and try to refind path");
                n.Halt();
                n.controller.pathToEndPoint = this.FindPath(npcTilePosition, nearPoints, location, n);   
            }*/
            n.Speed = n.speed = 2;
            int nodesCount = n.controller.pathToEndPoint != null ? n.controller.pathToEndPoint.Count : 0;
            this.Monitor.Log($"Target: {this.targetPositionTile} Player: {playerTilePosition} NPCT: {npcTilePosition} Path: {nodesCount}");
        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            if (e.NewLocation is MineShaft)
                return;

            this.spawnAbigailHere(e.NewLocation, e.Player.getTileLocationPoint());
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            // print button presses to the console window
            // this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.");
        }
    }
}