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
        public int standingTimeout = 100;
        public FollowController followController;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.followController = new FollowController();

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

            this.followController.Update(Game1.currentGameTime);
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            GameLocation location = Game1.player.currentLocation;
            Farmer player = Game1.player;
            this.myLastLocationTile = player.getTileLocationPoint();
            this.followController.leader = player;
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
                this.followController.follower = abigail;
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

        private bool isFar(Point p1, Point p2, float distanceThreshold)
        {
            return Utility.distance(p1.X, p2.X, p1.Y, p2.Y) > distanceThreshold;
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

        private void follow(NPC npc, Point endPointTile)
        {
            Point startPointTile = npc.getTileLocationPoint();

            bool left = endPointTile.X < startPointTile.X;
            bool right = endPointTile.X > startPointTile.X;
            bool up = endPointTile.Y < startPointTile.Y;
            bool down = endPointTile.Y > startPointTile.Y;

            if (left)
                npc.SetMovingOnlyLeft();
            if (right)
                npc.SetMovingOnlyRight();
            if (up)
                npc.SetMovingOnlyUp();
            if (down)
                npc.SetMovingOnlyDown();

            npc.willDestroyObjectsUnderfoot = false;
            npc.MovePosition(Game1.currentGameTime, Game1.viewport, npc.currentLocation);
            this.Monitor.Log($"left {left}; right: {right}; up: {up}; down: {down}; moving: {npc.isMoving()} destroy: {npc.willDestroyObjectsUnderfoot} timeout: {this.standingTimeout}");
        }

        private void comeTo(NPC n, Point endPointTile)
        {
            Point npcTilePosition = n.getTileLocationPoint();
            Stack<Point> nearPoints;

            nearPoints = new Stack<Point>(
                this.sortPointsByNearest(
                    this.getNearPoints(endPointTile, 1), 
                    npcTilePosition
                )
            );

            if (n.controller == null)
            {
                Point target = nearPoints.Pop();
                n.temporaryController = null;
                n.Halt();
                n.controller = new PathFindController(n, n.currentLocation, target, 0);
                if (n.controller.pathToEndPoint == null)
                {
                    n.controller.pathToEndPoint = this.FindPath(npcTilePosition, nearPoints, n.currentLocation, n);
                } else
                {
                    this.targetPositionTile = target;
                }
            }

            int nodesCount = n.controller.pathToEndPoint != null ? n.controller.pathToEndPoint.Count : 0;
            this.Monitor.Log($"Target: {this.targetPositionTile} NPCT: {npcTilePosition} Path: {nodesCount}");
        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            if (e.NewLocation is MineShaft)
                return;

            //this.followController.leader = e.Player;
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