using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QPath {

    public static class QPath
    {
        public static T[] FindPath<T>(
            IQPathWorld world,
            IQPathUnit unit,
            T starttile,
            T endtile,
            CostEstimateDelegate CostEstimateFunc
            
            ) where T : IQPathTile
        {
            //Debug.Log("Running Status");

            if (world == null || unit == null || starttile == null || endtile == null)
            {

                Debug.LogError("null values passes");

            }

            QPath_AStar<T> resolver = new QPath_AStar<T>(world, unit, starttile, endtile, CostEstimateFunc);

            resolver.DoWork();

            return resolver.GetList();


        }
    }

    public delegate float CostEstimateDelegate(IQPathTile a, IQPathTile b);



}