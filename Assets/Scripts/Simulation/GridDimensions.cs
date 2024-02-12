using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;
using Unity.Mathematics;
using Unity.Burst;

namespace BioCrowdsTechDemo
{

    /// <summary>
    /// A Regular Grid.
    /// With Helper Functions to Hash cells and convert world coordinates into grid coordinates and vice-versa.
    /// </summary>
    [System.Serializable]
    public struct GridDimensions
    {
        // (width/height)

        public float2 origin;
        public float2 dimensions;
        public int2 cells;

        public float2 cell_size;

        public GridDimensions(float2 _dimensions)
        {
            origin = float2(0f, 0f);
            dimensions = _dimensions;
            cells = int2(30, 30);

            cell_size = dimensions / cells;

        }

        /// <summary>
        /// A 30 by 30 cells Grid Constructor.
        /// </summary>
        /// <param name="_dimensions">The dimensions of the Grid in world units.</param>
        /// <param name="_origin">The origin of the Grid in world units.</param>
        public GridDimensions(float2 _dimensions, float2 _origin)
        {
            origin = _origin;
            dimensions = _dimensions;
            cells = int2(30, 30);
            cell_size = dimensions / cells;
        }

        /// <summary>
        /// Grid Constructor. 
        /// The Grid has _divisions.x by _divisions.y cells.
        /// </summary>
        /// <param name="_dimensions">The dimensions of the Grid in world units.</param>
        /// <param name="_origin">The origin of the Grid in world units.</param>
        /// <param name="_divisions">A desired number of Grid divisions per dimension.</param>
        public GridDimensions(float2 _dimensions, float2 _origin, int2 _divisions)
        {
            origin = _origin;
            dimensions = _dimensions;
            cells = _divisions;
            cell_size = dimensions / cells;
        }


        /// <summary>
        /// Returns which Cell a world coordinate position belongs to.
        /// </summary>
        /// <param name="_position">A world coordinate position.</param>
        /// <returns></returns>
        public int2 position_to_cell(float2 _position)
        {
            var p = _position - origin;
            p /= cell_size;

            return int2(p);

        }

        /// <summary>
        /// Converts a Cell to a hash ID.
        /// </summary>
        /// <param name="_cell">A Cell Coordinate pair.</param>
        /// <returns></returns>
        public int cell_to_id(int2 _cell)
        {
            return _cell.x + _cell.y * cells.x;
        }

        /// <summary>
        /// Converts a hash to a Cell coordinate.
        /// </summary>
        /// <param name="id">A Cell hash ID.</param>
        /// <returns></returns>
        public int2 id_to_cell(int id)
        {
            return int2(id % cells.x, id / cells.x);
        }

        /// <summary>
        /// Converts a World coordinate positions to a Cell hash ID.
        /// </summary>
        /// <param name="_position">a world coordinate.</param>
        /// <returns></returns>
        public int position_to_id(float2 _position)
        {
            return cell_to_id(position_to_cell(_position));
        }

        /// <summary>
        /// Sets a Cell size and recomputes number of divisions per dimension to fit the Grid.
        /// </summary>
        /// <param name="_cell_size">The desired cell size.</param>
        public void set_cellsize(float2 _cell_size)
        {
            cell_size = _cell_size;
            cells = int2(dimensions / cell_size);
        }

        public float2 cell_corner(int2 _cell)
        {
            return _cell * cell_size + origin;
        }

        /// <summary>
        /// Creates a UnityEngine.Bounds object representing this Grid.
        /// </summary>
        /// <returns></returns>
        public Bounds AsBounds()
        {

            Vector3 size = new Vector3(dimensions.x, 2.0f, dimensions.y);
            Vector3 center = new Vector3(origin.x, 0.0f, origin.y);
            return new Bounds(center, size);
        }

    }
}