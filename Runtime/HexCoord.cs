using System;
using System.Collections.Generic;
using UnityEngine;

namespace NavigationSystem.GridSystem
{
    public struct HexCoord
    {
        public readonly int q;
        public readonly int r;
        public readonly int s;

        public static readonly Dictionary<Direction, HexCoord> Directions = new()
        {
            { Direction.East, new HexCoord(1, 0, -1) },
            { Direction.SouthEast, new HexCoord(1, -1, 0)},
            { Direction.SouthWest, new HexCoord(0, -1, 1)},
            { Direction.West, new HexCoord(-1, 0, 1)},
            { Direction.NorthWest, new HexCoord(-1, 1, 0)},
            { Direction.NorthEast, new HexCoord(0, 1, -1)}
        };
        
        public HexCoord(int q, int r, int s)
        {
            this.q = q;
            this.r = r;
            this.s = s;
            if (q + r + s != 0) throw new ArgumentException($"q + r + s must be 0, but was q:{q}, r:{r}, s:{s}");
        }
        public override string ToString()
        {
            return $"q: {q}, r: {r}";
        }
        public static HexCoord CubeRound(float q, float r , float s)
        {
            return CubeRound(new Vector3(q, r, s));
        }
        public static HexCoord CubeRound(Vector3 point)
        {
            int q = Mathf.RoundToInt(point.x);
            int r = Mathf.RoundToInt(point.y);
            int s = Mathf.RoundToInt(point.z);

            float qDiff = Mathf.Abs(q - point.x);
            float rDiff = Mathf.Abs(r - point.y);
            float sDiff = Mathf.Abs(s - point.z);

            if (qDiff > rDiff && qDiff > sDiff)
            {
                q = -r - s;
            }
            else if (rDiff > sDiff)
            {
                r = -q - s;
            }
            else
            {
                s = -q - r;
            }

            return new HexCoord(q, r, s);
        }
        public static HexCoord operator +(HexCoord a, HexCoord b)
        {
            return new HexCoord(a.q + b.q, a.r + b.r, a.s + b.s);
        }
        public static HexCoord operator -(HexCoord a, HexCoord b)
        {
            return new HexCoord(a.q - b.q, a.r - b.r, a.s - b.s);
        }
        public static bool operator ==(HexCoord a, HexCoord b)
        {
            return a.q == b.q && a.r == b.r && a.s == b.s;
        }
        public static bool operator !=(HexCoord a, HexCoord b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            if (obj is HexCoord c)
            {
                return c == this;
            }

            return false;
        }
        public override int GetHashCode()
        {
            return (q, r, s).GetHashCode();
        }

        public List<HexCoord> GetPossibleNeighbours()
        {
            return new List<HexCoord>
            {
                this + HexCoord.Directions[Direction.East],
                this + HexCoord.Directions[Direction.SouthEast],
                this + HexCoord.Directions[Direction.SouthWest],
                this + HexCoord.Directions[Direction.West],
                this + HexCoord.Directions[Direction.NorthWest],
                this + HexCoord.Directions[Direction.NorthEast]   
            };
        }
    }

    public enum Direction
    {
        East = 0,
        SouthEast = 1,
        SouthWest = 2,
        West = 3,
        NorthWest = 4,
        NorthEast = 5
    }
}
