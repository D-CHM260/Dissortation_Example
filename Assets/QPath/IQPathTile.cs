using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QPath
{


    public interface IQPathTile {

        IQPathTile[] GetNeighbours();

        float AggregateCostToEnter(float coastsofar, IQPathTile sourceTile, IQPathUnit theUnit);

    }
}
