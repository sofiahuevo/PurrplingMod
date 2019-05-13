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
        public const int FOLLOWING_LOST_TIMEOUT = 50;
        public const float SPEEDUP_DISTANCE_THRESHOLD = 7;
        public const float MOVE_THRESHOLD_DISTANCE = 3;
        public const float PROXIMITY_THRESHOLD = 80;
        public const float LOST_DISTANCE = 16;
        public const float OUT_OF_RANGE_DISTANCE = 64;
        public const int PATH_MAX_NODE_COUNT = 28;
        public Character leader;
        public NPC follower;
        public int followingLostTime = 0;
        public Queue<Point> pathToFollow;
        public Point currentFollowedPoint;
        public Point leaderLastTileCheckPoint;

        public FollowController()
        {
            this.pathToFollow = new Queue<Point>();
        }

        public void Update(GameTime time)
        {
            if (this.follower == null || this.leader == null)
                return;

            if (this.leader.currentLocation != this.follower.currentLocation || this.FollowerLeaderIsTooFar())
            {
                FollowController.WarpTo(this.follower, this.leader.currentLocation, this.leader.getTileLocationPoint());
                this.pathToFollow.Clear();
                this.currentFollowedPoint = Point.Zero;
                this.leaderLastTileCheckPoint = Point.Zero;
            }

            this.UpdateFollowing(time, this.follower, this.leader);
        }

        protected void UpdateFollowing(GameTime time, NPC follower, Character leader)
        {
            Point leaderTilePoint = leader.getTileLocationPoint();
            Point followerTilePoint = follower.getTileLocationPoint();
            Point leaderBoxCenter = leader.GetBoundingBox().Center;
            Point followerBoxCenter = follower.GetBoundingBox().Center;

            if (this.follower.speed != this.leader.speed)
                this.follower.speed = this.leader.speed;

            if (Helper.Distance(leaderTilePoint, followerTilePoint) > SPEEDUP_DISTANCE_THRESHOLD)
                this.follower.addedSpeed = 2;

            if (follower.isMoving())
                this.followingLostTime = 0; // Follower move? Reset standing time
            else
            {
                if (Helper.Distance(leaderTilePoint, followerTilePoint) < MOVE_THRESHOLD_DISTANCE)
                    return;

                if (this.followingLostTime++ >= FOLLOWING_LOST_TIMEOUT)
                {
                    this.ResolveLostFollow();
                    return;
                }
            }

            if (leaderTilePoint != this.leaderLastTileCheckPoint)
            {
                this.AddPathPoint(leaderTilePoint);
            }

            if (Helper.Distance(leaderBoxCenter, followerBoxCenter) < PROXIMITY_THRESHOLD)
            {
                follower.Halt();
                this.pathToFollow.Clear();
                this.currentFollowedPoint = leaderTilePoint;
                return;
            }

            if (this.pathToFollow.Count > PATH_MAX_NODE_COUNT || Helper.Distance(leaderTilePoint, followerTilePoint) > LOST_DISTANCE)
            {
                this.ResolveLostFollow(true);
                return;
            }

            if (follower.controller == null)
                this.FollowPath(time);
        }

        private void ResolveLostFollow(bool emoteWhenPathIsFound = false)
        {
            Point endTilePoint = this.leader.getTileLocationPoint();

            this.follower.addedSpeed = 4;
            this.pathToFollow.Clear();
            this.currentFollowedPoint = endTilePoint;
            if (!FollowController.ComeTo(this.follower, endTilePoint, emoteWhenPathIsFound))
                FollowController.WarpTo(this.follower, endTilePoint);
            this.followingLostTime = 0;
        }

        private void FollowPath(GameTime time)
        {
            if (this.pathToFollow.Count == 0)
            {
                this.AddPathPoint(this.leader.getTileLocationPoint());
                return;
            }
            else if (this.currentFollowedPoint == this.follower.getTileLocationPoint())
                this.currentFollowedPoint = this.pathToFollow.Dequeue();

            FollowController.FollowTile(this.follower, this.currentFollowedPoint, time);
        }

        private void AddPathPoint(Point p)
        {
            if (this.pathToFollow.Count == 0)
                this.currentFollowedPoint = p;
            this.pathToFollow.Enqueue(p);
            this.leaderLastTileCheckPoint = p;
        }

        private bool FollowerLeaderIsTooFar()
        {
            return Helper.Distance(this.leader.getTileLocationPoint(), this.follower.getTileLocationPoint()) > OUT_OF_RANGE_DISTANCE;
        }

        public static void FollowTile(NPC follower, Point endPointTile, GameTime time)
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
            follower.MovePosition(time, Game1.viewport, follower.currentLocation);
        }

        public static bool ComeTo(NPC follower, Point endPointTile, bool emoteWhenPathIsFound = false)
        {
            Point npcTilePosition = follower.getTileLocationPoint();
            bool pathFound = false;

            if (follower.controller == null)
            {
                follower.Halt();
                follower.temporaryController = null;
                follower.willDestroyObjectsUnderfoot = false;

                Stack<Point> path = FollowController.FindPath(npcTilePosition, endPointTile, follower.currentLocation, follower);
                follower.controller = new PathFindController(path, follower, follower.currentLocation);

                if (follower.controller.pathToEndPoint == null)
                    follower.doEmote(8);
                else
                {
                    if (emoteWhenPathIsFound)
                        follower.doEmote(40);
                    pathFound = true;
                }
            }

            follower.updateEmote(Game1.currentGameTime);
            return pathFound;
        }

        public static void WarpTo(NPC follower, Point tilePosition)
        {
            follower.Halt();
            follower.controller = follower.temporaryController = null;
            follower.setTilePosition(tilePosition);
        }

        public static void WarpTo(NPC follower, GameLocation location, Point tilePosition)
        {
            if (follower.currentLocation == location)
                FollowController.WarpTo(follower, tilePosition);

            follower.Halt();
            follower.controller = follower.temporaryController = null;
            follower.currentLocation.characters.Remove(follower);
            follower.currentLocation = location;
            follower.setTilePosition(tilePosition);

            location.addCharacter(follower);
        }

        public static Stack<Point> FindPath(Point startTilePoint, Point endPointTile, GameLocation location, NPC n, int limit = 100)
        {
            Stack<Point> path;
            PathFindController.isAtEnd endFunction = new PathFindController.isAtEnd(PathFindController.isAtEndPoint);
            Stack<Point> nearPointsStack = new Stack<Point>(
                Helper.SortPointsByNearest(
                    Helper.NearPoints(endPointTile, 1), startTilePoint
                )
            );

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
