using UnityEngine;

public class Map : System.IDisposable
{
    public enum State
    {
        Alive,
        Death,
        Source,
        Obstacle
    }
    [System.Serializable]
    public struct Node
    {
        public global::Node node;
        public int x, y;
        public int px, py;
        public int player;
        public int value;
        public State state, next;
        public int enegry;
        public float pheromone;
        public bool dirty;
        public Node(int x, int y, global::Node prefab)
        {
            node = Object.Instantiate(prefab);
            node.gameObject.SetActive(true);
            node.transform.position = new Vector3(x, y);
            node.transform.SetParent(GameMgr.Instance.mapRoot);
            this.x = x; this.y = y;
            px = py = -1;
            player = -1;
            value = Battle.MAX_NODE_VALUE;
            state = next = State.Death;
            enegry = 0;
            pheromone = 0;
            dirty = false;
        }
    }
    public readonly int width, height;
    public readonly Node[,] nodes;
    public Map(int width, int height, global::Node prefab)
    {
        this.width = width;
        this.height = height;
        nodes = new Node[width, height];
        for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                ref var node = ref nodes[x, y];
                node = new Node(x, y, prefab);
            }
    }
    public void Update()
    {
        for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                ref var node = ref nodes[x, y];
                if (node.state != State.Source && node.px >= 0 && node.py >= 0)
                {
                    var parent = nodes[node.px, node.py];
                    if (parent.player != node.player)
                    {
                        node.px = node.py = -1;
                        node.next = State.Death;
                    }
                    else if (parent.state == State.Death)
                        node.next = State.Death;
                    else
                        node.next = State.Alive;
                }
            }
        for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                ref var node = ref nodes[x, y];
                node.pheromone *= .975f;
                node.state = node.next;
                node.node.UpdateNode(node);
                node.dirty = false;
            }
    }
    public void PathAddPheromone(int x, int y, float pheromone)
    {
        while (true)
        {
            ref var node = ref nodes[x, y];
            if (node.player < 0 || node.state != State.Alive) return;
            if (node.px < 0 || node.py < 0) return;
            if (node.dirty) return;
            node.pheromone += pheromone;
            node.dirty = true;
            x = node.px;
            y = node.py;
        }
    }
    public void Dispose()
    {
        foreach (var node in nodes)
            Object.Destroy(node.node.gameObject);
    }
}
