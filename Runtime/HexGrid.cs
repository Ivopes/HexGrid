using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NavigationSystem.GridSystem
{
    public class HexGrid : MonoBehaviour
    {
        [SerializeField] private GameObject tilePrefab;
        public Dictionary<HexCoord, HexNode> Nodes { get; private set; } = new();
        private List<HexCoord> debugNodes = new();

        [SerializeField] private Vector2 worldSize;
        [SerializeField] private LayerMask obstacleMask;
        
        [SerializeField] private bool drawGrid;

        private int gridSizeX, gridSizeY;
        private Vector3 worldBottomLeft;

        private static readonly HexOrientation pointUp = new HexOrientation(Mathf.Sqrt(3f), Mathf.Sqrt(3f) / 2f, 0f,
            3f / 2f, Mathf.Sqrt(3f) / 3f, -1f / 3f, 0f, 2f / 3f, .5f);

        private static readonly HexOrientation flatUp = new HexOrientation(3f / 2f, 0f, Mathf.Sqrt(3f) / 2f,
            Mathf.Sqrt(3f), 2f / 3f, 0f, -1f / 3f, Mathf.Sqrt(3f) / 3f, 0f);

        private const float twoPI = 2f * Mathf.PI;
        private static readonly float[] anglesSin = new []{
            Mathf.Sin(twoPI * (.5f - 0) / 6f),
            Mathf.Sin(twoPI * (.5f - 1) / 6f),
            Mathf.Sin(twoPI * (.5f - 2) / 6f),
            Mathf.Sin(twoPI * (.5f - 3) / 6f),
            Mathf.Sin(twoPI * (.5f - 4) / 6f),
            Mathf.Sin(twoPI * (.5f - 5) / 6f)
        };
        private static readonly float[] anglesCos = new []{
            Mathf.Cos(twoPI * (.5f - 0) / 6f),
            Mathf.Cos(twoPI * (.5f - 1) / 6f),
            Mathf.Cos(twoPI * (.5f - 2) / 6f),
            Mathf.Cos(twoPI * (.5f - 3) / 6f),
            Mathf.Cos(twoPI * (.5f - 4) / 6f),
            Mathf.Cos(twoPI * (.5f - 5) / 6f)
        };
        
        [SerializeField]
        public Vector2 size;
        private readonly HexOrientation orientation = pointUp;

        public Vector2 FlatSize => Mathf.Sqrt(3) * size/2;
        public Vector2 PointSize => size;
        
        // Start is called before the first frame update
        private void Awake()
        {
            worldBottomLeft = transform.position - Vector3.right * worldSize.x / 2 - Vector3.forward * worldSize.y / 2;
        
            CreateGrid();
        }
        private void CreateGrid()
        {
            // How many hex can fit in area?
            gridSizeX = Mathf.FloorToInt(worldSize.x / (Mathf.Sqrt(3)*size.x));
            gridSizeY = Mathf.FloorToInt(worldSize.y / (size.y * 3 / 2));

            GenerateHexes();
        }
        private void CreateDebugGrid()
        {
            // How many hex can fit in area?
            gridSizeX = Mathf.FloorToInt(worldSize.x / (Mathf.Sqrt(3)*size.x));
            gridSizeY = Mathf.FloorToInt(worldSize.y / (size.y * 3 / 2));

            GenerateDebugHexes();
        }
        private void GenerateHexes()
        {
            Nodes.Clear();
            for (int i = 0; i < gridSizeY; i++)
            {
                for (int j = 0; j < gridSizeX; j++)
                {
                    worldBottomLeft = transform.position - Vector3.right * worldSize.x / 2 - Vector3.forward * worldSize.y / 2;

                    float allegedXPos = j * FlatSize.x*2 + (i % 2 * (FlatSize.x)) + FlatSize.x; 
                    float allegedYPos = PointSize.y + i * PointSize.y*2 * 3 / 4f;
                    
                    Vector3 allegedCenter = worldBottomLeft + Vector3.right * allegedXPos + Vector3.forward * allegedYPos;

                    HexCoord position = PixelToHex(allegedCenter);
                    
                    Vector3 centerByCoord = HexToPixel(position);
                    
                    bool walkable = !Physics.CheckBox(centerByCoord,new Vector3(FlatSize.x/2, 1,FlatSize.y/2), Quaternion.identity, obstacleMask);

                    if (walkable)
                    {
                        GameObject nodeGameObject = Instantiate(tilePrefab, centerByCoord, tilePrefab.transform.rotation, transform);
                        nodeGameObject.name = position.ToString();
                        HexNode hexNode = nodeGameObject.GetComponent<HexNode>();
                        hexNode.coord = position;
                        hexNode.isWalkable = walkable;
                        Nodes[position] = hexNode;
                    }
                }
            }
        }
        private void GenerateDebugHexes()
        {
            debugNodes.Clear();
            for (int i = 0; i < gridSizeY; i++)
            {
                for (int j = 0; j < gridSizeX; j++)
                {
                    worldBottomLeft = transform.position - Vector3.right * worldSize.x / 2 - Vector3.forward * worldSize.y / 2;

                    float allegedXPos = j * FlatSize.x*2 + (i % 2 * (FlatSize.x)) + FlatSize.x; 
                    float allegedYPos = PointSize.y + i * PointSize.y*2 * 3 / 4f;
                    
                    Vector3 allegedCenter = worldBottomLeft + Vector3.right * allegedXPos + Vector3.forward * allegedYPos;

                    HexCoord position = PixelToHex(allegedCenter);
                    
                    Vector3 centerByCoord = HexToPixel(position);
                    
                    bool walkable = !Physics.CheckBox(centerByCoord,new Vector3(FlatSize.x/2, 1,FlatSize.y/2), Quaternion.identity, obstacleMask);

                    if (walkable)
                        debugNodes.Add(position);
                }
            }
        }
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(worldSize.x, 1, worldSize.y));

            if (drawGrid)
            {
                CreateDebugGrid();;
                DrawDebugGrid();
            }
        }
        private void DrawDebugGrid()
        {
            foreach (var kvNode in debugNodes)
            {
                HexCoord hexNode = kvNode;

                Vector3[] points = new Vector3[6];
                
                points = PolygonCorners(hexNode).ToArray();

                //Gizmos.color = hexNode.isWalkable ? Color.white : Color.red;
                for (int i = 0; i < 5; ++i)
                {
                    Gizmos.DrawLine(points[i], points[i + 1]);
                }

                Gizmos.DrawLine(points[5], points[0]);

                //Handles.Label(hexNode.worldPosition+new Vector3(-.2f, 0, 0), $" q:{hexNode.coord.q} r:{hexNode.coord.r}");
                
                //Gizmos.color = Color.white;
            }
        }
        private Vector3 HexCornerOffset(int corner)
        {
            const float twoPI = 2f * Mathf.PI;
            float angle = twoPI * (orientation.start_angle - corner) / 6f;
            return new Vector3(size.x * Mathf.Cos(angle), 0,size.y * Mathf.Sin(angle));
        }
        private List<Vector3> PolygonCorners(HexCoord h)
        {
            List<Vector3> corners = new List<Vector3>{};
            Vector3 center = HexToPixel(h);
            for (int i = 0; i < 6; i++)
            {
                Vector3 offset = HexCornerOffset(i);
                corners.Add(new Vector3(center.x + offset.x, center.y,center.z + offset.z));
            }
            return corners;
        }
        public Vector3 HexToPixel(HexCoord h)
        {
            float x = (orientation.f0 * h.q + orientation.f1 * h.r) * size.x;
            float y = (orientation.f2 * h.q + orientation.f3 * h.r) * size.y;
            return new Vector3(x + transform.position.x, .1f, y + transform.position.z);
        }
        public HexCoord PixelToHex(Vector3 p)
        {
            Vector2 pt = new Vector3((p.x - transform.position.x) / size.x, (p.z - transform.position.z) / size.y);
            float q = orientation.b0 * pt.x + orientation.b1 * pt.y;
            float r = orientation.b2 * pt.x + orientation.b3 * pt.y;
            return HexCoord.CubeRound(q, r, -q - r);
        }
        public int Distance(HexCoord a, HexCoord b)
        {
            HexCoord c = a - b;
            return Mathf.Max(Mathf.Abs(c.q), Mathf.Abs(c.r), Mathf.Abs(c.s));
        }
        public List<HexCoord> GetNeighbours(HexCoord center)
        {
            
            var n = new List<HexCoord>
            {
                center + HexCoord.Directions[Direction.East],
                center + HexCoord.Directions[Direction.SouthEast],
                center + HexCoord.Directions[Direction.SouthWest],
                center + HexCoord.Directions[Direction.West],
                center + HexCoord.Directions[Direction.NorthWest],
                center + HexCoord.Directions[Direction.NorthEast]   
            };

            for (int i = n.Count - 1; i >= 0; i--)
            {
                if (!Nodes.ContainsKey(n[i]))
                {
                    n.Remove(n[i]);
                }
            }
            
            return n;
        }
    }
}
