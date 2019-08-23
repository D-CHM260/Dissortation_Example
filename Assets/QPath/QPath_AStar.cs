using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QPath { 

    public class QPath_AStar<T> where T:IQPathTile{
        public QPath_AStar(
            IQPathWorld world, 
            IQPathUnit unit, 
            T starttile, 
            T endtile,
            CostEstimateDelegate CostEstimateFunc)
        {
            this.world = world;
            this.unit = unit;
            this.starttile = starttile;
            this.endtile = endtile;
            this.CostEstimateFunc = CostEstimateFunc;
        }

        IQPathWorld world;
        IQPathUnit unit;
        T starttile;
        T endtile;
        CostEstimateDelegate CostEstimateFunc;

        Queue<T> path;

        public void DoWork()
        {

            path = new Queue<T>();

            HashSet<T> closedset = new HashSet<T>();

            PathfindingPriorityQueue<T> openSet = new PathfindingPriorityQueue<T>();

            openSet.Enqueue(starttile, 0);

            Dictionary<T, T> came_From = new Dictionary<T, T>();

            Dictionary<T, float> g_score = new Dictionary<T, float>();
            g_score[starttile] = 0;

            Dictionary<T, float> f_score = new Dictionary<T, float>();
            f_score[starttile] = CostEstimateFunc(starttile, endtile);

            while (openSet.Count > 0)
            {
                T current = openSet.Dequeue();

                
                if (System.Object.ReferenceEquals(current, endtile))
                {
                    reconstruct_path(came_From, current);
                    return;
                }

                closedset.Add(current);

                foreach (T edge_neighbour in current.GetNeighbours())
                {
                    T neighbour = edge_neighbour;

                    if (closedset.Contains(neighbour))
                    {
                        continue; 
                    }

                    float total_pathfinding_cost_to_neighbor =
                        neighbour.EntryCost(g_score[current], current, unit);

                    if (total_pathfinding_cost_to_neighbor < 0)
                    {
                        
                        continue;
                    }
                    

                    float tentative_g_score = total_pathfinding_cost_to_neighbor;

                   
                    if (openSet.Contains(neighbour) && tentative_g_score >= g_score[neighbour])
                    {
                        continue;
                    }

                    
                    came_From[neighbour] = current;
                    g_score[neighbour] = tentative_g_score;
                    f_score[neighbour] = g_score[neighbour] + CostEstimateFunc(neighbour, endtile);

                    openSet.EnqueueOrUpdate(neighbour, f_score[neighbour]);
                } 
            } 


        }

        private void reconstruct_path(Dictionary<T,T> came_From, T current)
        {
            Queue<T> total_path = new Queue<T>();
            total_path.Enqueue(current);
            

            while (came_From.ContainsKey(current))
            {
                current = came_From[current];
                total_path.Enqueue(current);
            }
            path = new Queue<T>(total_path.Reverse());
        }


    

    

    public T[] GetList()
        {

            return path.ToArray();

        }

	
    }
}