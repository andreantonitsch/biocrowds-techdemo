using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;
using Unity.Mathematics;
using Unity.Burst;

namespace BioCrowdsTechDemo
{
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

        public GridDimensions(float2 _dimensions, float2 _origin)
        {
            origin = _origin;
            dimensions = _dimensions;
            cells = int2(30, 30);
            cell_size = dimensions / cells;
        }

        public GridDimensions(float2 _dimensions, float2 _origin, int2 _cell_size)
        {
            origin = _origin;
            dimensions = _dimensions;
            cells = _cell_size;
            cell_size = dimensions / cells;
        }


        public int2 position_to_cell(float2 _position)
        {
            var p = _position - origin;
            p /= cell_size;

            return int2(p);

        }

        public int cell_to_id(int2 _cell)
        {
            return _cell.x + _cell.y * cells.x;
        }

        public int2 id_to_cell(int id)
        {
            return int2(id % cells.x, id / cells.x);
        }


        public int position_to_id(float2 _position)
        {
            return cell_to_id(position_to_cell(_position));
        }

        public void set_cellsize(float2 _cell_size)
        {
            cell_size = _cell_size;
            cells = int2(dimensions / cell_size);
        }

        public float2 cell_corner(int2 _cell)
        {
            return _cell * cell_size + origin;
        }

    }
}