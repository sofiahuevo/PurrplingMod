using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Locations;
using Microsoft.Xna.Framework;

namespace PurrplingMod
{
    public class FollowController
    {
        public const int STANDING_TIMEOUT = 100;
        public Character leader;
        public NPC follower;
        public int standingTime = 0;

        public void Update(GameTime time)
        {
            if (this.follower == null || this.leader == null)
                return;

            if (this.leader.currentLocation is MineShaft)
                return;

            this.UpdateFollowing(time, this.follower, this.leader);
        }

        protected void UpdateFollowing(GameTime time, NPC follower, Character leader)
        {
            Point leaderTilePoint = leader.getTileLocationPoint();
            Point followerTilePoint = follower.getTileLocationPoint();
            Point leaderBoxCenter = leader.GetBoundingBox().Center;
            Point followerBoxCenter = follower.GetBoundingBox().Center;

            if (Helper.Distance(leaderTilePoint, followerTilePoint) > 7)
                this.follower.speed = this.leader.speed;

            if (!follower.isMoving())
                if (Helper.Distance(leaderTilePoint, followerTilePoint) < 3)
                    return;
                else
                {
                    if (this.standingTime >= FollowController.STANDING_TIMEOUT)
                    {
                        follower.Halt();
                        follower.addedSpeed = 2;
                        this.comeTo(follower, leaderTilePoint);
                        this.standingTime = 0;
                    }
                    else
                        this.standingTime++;
                }
            else
                this.standingTime = 0;

            if (Helper.Distance(leaderBoxCenter, followerBoxCenter) < 64)
                follower.Halt();
            else if (follower.controller == null)
                this.followTile(follower, leaderTilePoint);
        }

        private void followTile(NPC follower, Point endPointTile)
        {
            Point startPointTile = follower.getTileLocationPoint();

            if (endPointTile.X < startPointTile.X)
                follower.SetMovingOnlyLeft();
            if (endPointTile.X > startPointTile.X)
                follower.SetMovingOnlyRight();
            if (endPointTile.Y < startPointTile.Y)
                follower.SetMovingOnlyUp();
            if (endPointTile.Y > startPointTile.Y)
                follower.SetMovingOnlyDown();

            follower.willDestroyObjectsUnderfoot = false;
            follower.MovePosition(Game1.currentGameTime, Game1.viewport, follower.currentLocation);
        }

        private void comeTo(NPC follower, Point endPointTile)
        {
            Point npcTilePosition = follower.getTileLocationPoint();
            Stack<Point> nearPoints;

            nearPoints = new Stack<Point>(
                Helper.SortPointsByNearest(
                    Helper.NearPoints(endPointTile, 1), npcTilePosition
                )
            );

            if (follower.controller == null)
            {
                Point target = nearPoints.Pop();
                follower.temporaryController = null;
                follower.Halt();
                follower.controller = new PathFindController(follower, follower.currentLocation, target, 0);
                if (follower.controller.pathToEndPoint == null)
                {
                    follower.controller.pathToEndPoint = FollowController.FindFirstPath(npcTilePosition, nearPoints, follower.currentLocation, follower);
                }
            }
        }

        public static Stack<Point> FindFirstPath(Point startTilePoint, Stack<Point> nearPointsStack, GameLocation location, NPC n, int limit = 100)
        {
            Stack<Point> path;
            PathFindController.isAtEnd endFunction = new PathFindController.isAtEnd(PathFindController.isAtEndPoint);

            while (nearPointsStack.Count > 0)
            {
                Point target = nearPointsStack.Pop();
                path = PathFindController.findPath(startTilePoint, target, endFunction, location, n, limit);
                if (path != null)
                    return path;
            }

            return null;
        }

    }
}
