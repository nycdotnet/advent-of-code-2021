using static common.Utils;

namespace AStar
{
    /// <summary>
    /// Implements AStar where the cost is measured as a <see cref="short"/>.
    /// </summary>
    public class AStarShort<TLocationIdentifier> where TLocationIdentifier : notnull
    {
        // for the given key location identifier, returns the identifier of the previous location
        // along the least expensive path.
        public readonly Dictionary<TLocationIdentifier, TLocationIdentifier> cameFrom = new();

        // the least expensive known cost to get to the given key location
        public readonly Dictionary<TLocationIdentifier, short> bestKnownCostTo = new();

        public AStarShort(IWeightedGraph<TLocationIdentifier> graph,
            TLocationIdentifier start,
            TLocationIdentifier goal,
            Func<TLocationIdentifier, TLocationIdentifier, short> heuristic)
        {
            Start = start;
            Goal = goal;

            var frontier = new PriorityQueue<TLocationIdentifier, short>();
            frontier.Enqueue(start, 0);

            // we came from the start, so this is effectively the "identity element"
            cameFrom[start] = start;
            // there is zero cost to get here because this is the start.
            bestKnownCostTo[start] = 0;

            while (frontier.TryDequeue(out var current, out var _))
            {
                if (current.Equals(goal))
                {
                    PathFound = true;
                    break;
                }
                var costToGetHere = bestKnownCostTo[current];
                foreach (var next in graph.GetNeighbors(current))
                {
                    var newCost = (short)(costToGetHere + graph.Cost(current, next));
                    if (!bestKnownCostTo.TryGetValue(next, out var previousBestCost) || previousBestCost > newCost)
                    {
                        // we have never been here, or the previous best cost was more expensive.
                        bestKnownCostTo[next] = newCost;
                        var priority = (short)(newCost + heuristic(next, goal));
                        frontier.Enqueue(next, priority);
                        cameFrom[next] = current;
                    }
                }
            }
        }

        public TLocationIdentifier Start { get; }
        public TLocationIdentifier Goal { get; }
        public bool PathFound { get; private set; }

        /// <summary>
        /// returns the optimal path, navigating backward from the goal to the start.
        /// </summary>
        public IEnumerable<TLocationIdentifier> GetOptimalPath()
        {
            ThrowIfNotSuccess();

            var path = new List<TLocationIdentifier>();

            var current = Goal;
            do
            {
                path.Add(current);
                current = cameFrom[current];
            } while (!current.Equals(Start));
            return path;
        }

        public short GetOptimalPathCost()
        {
            ThrowIfNotSuccess();
            return bestKnownCostTo[Goal];
        }

        private void ThrowIfNotSuccess()
        {
            if (!PathFound)
            {
                throw new Exception("No path found.");
            }
        }
    }

    public class Dec15Grid : IWeightedGraph<(int x, int y)>
    {
        private short[][] costs;
        public Dec15Grid(short[][] costs, (int x, int y) start, (int x, int y) goal)
        {
            this.costs = costs;
            Start = start;
            Goal = goal;
            var astar = new AStarShort<(int x, int y)>(this, start, goal, Cost);
            OptimalPath = astar.GetOptimalPath().ToList();
            OptimalPath.Reverse();
            OptimalPathCost = astar.GetOptimalPathCost();

        }

        public List<(int x, int y)> OptimalPath { get; private set; }
        public short OptimalPathCost { get; }
        public (int x, int y) Start { get; private set; }
        public (int x, int y) Goal { get; private set; }

        public short Cost((int x, int y) from, (int x, int y) to) => costs[to.y][to.x];

        private bool InBounds((int x, int y) location) => location.x >= 0 && location.y >= 0 && location.y < costs.Length && location.x < costs[location.y].Length;

        public IEnumerable<(int x, int y)> GetNeighbors((int x, int y) location)
        {
            // down
            var test = (x: location.x, y: location.y + 1);
            if (InBounds(test))
            {
                yield return test;
            }

            // up
            test = (x: location.x, y: location.y - 1);
            if (InBounds(test))
            {
                yield return test;
            }

            // left
            test = (x: location.x - 1, y: location.y);
            if (InBounds(test))
            {
                yield return test;
            }

            // right
            test = (x: location.x + 1, y: location.y);
            if (InBounds(test))
            {
                yield return test;
            }
        }
    }

    public interface IWeightedGraph<TLocationIdentifier>
    {
        /// <summary>
        /// Returns the cost to travel from location <paramref name="from"/> to <paramref name="to"/>.
        /// </summary>
        short Cost(TLocationIdentifier from, TLocationIdentifier to);
        IEnumerable<TLocationIdentifier> GetNeighbors(TLocationIdentifier location);
    }
}
