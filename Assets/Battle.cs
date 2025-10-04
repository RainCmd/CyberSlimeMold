using System.Collections.Generic;
using UnityEngine;
public class Player
{
    public int id;
    public string name;
    public int x, y;
    public int width, height;
    public int hp;
    public int territory;
    public int soldier;
    public Color color;
    public Color territoryColor;
    public Color energyColor;
    public Player(int id, string name, int x, int y, int width, int height, Color color)
    {
        this.id = id;
        this.name = name;
        this.x = x - width / 2;
        this.y = y - height / 2;
        this.width = width;
        this.height = height;
        hp = 100;
        territory = width * height;
        soldier = 0;
        this.color = color;
        territoryColor = color * .5f;
        territoryColor.a = 1;
        energyColor = territoryColor + new Color(.5f, .5f, .5f);
        energyColor.a = 1;
    }
}
public class Battle : System.IDisposable
{
    public struct Enegry
    {
        public global::Enegry enegry;
        public int player;
        public int value;
        public bool harvest;
        public Vector2 position;
        public Vector2 velocity;
        public bool forward;
        public int sx, sy;
        public int tx, ty;

        public Enegry(int player, int value) : this()
        {
            this.player = player;
            this.value = value;
            forward = true;
        }

        public void Back()
        {
            (sx, tx) = (tx, sx);
            (sy, ty) = (ty, sy);
            forward = !forward;
        }
        public void UpdatePV()
        {
            position = new Vector2(sx, sy);
            velocity = (new Vector2(tx, ty) - position).normalized * 0.15f;
        }
    }
    public const int MAX_NODE_VALUE = 100;
    public readonly Map map;
    public readonly Player[] players;
    public readonly List<Enegry> enegries = new();
    private readonly global::Enegry prefab;
    private readonly Stack<global::Enegry> pool = new();
    public Battle(int width, int height, global::Enegry enegryPrefab, Node nodePrefab)
    {
        prefab = enegryPrefab;
        map = new Map(width, height, nodePrefab);
        players = new Player[]
        {
            new(0, "红", width / 8, height / 8, width / 16, height / 16, Color.red),
            new(1, "黄", width * 7 / 8, height / 8, width / 16, height / 16, Color.yellow),
            new(2, "蓝", width * 7 / 8, height * 7 / 8, width / 16, height / 16, Color.blue),
            new(3, "绿", width / 8, height * 7 / 8, width / 16, height / 16, Color.green),
            new(4, "青", width / 8, height / 2, width / 16, height / 16, Color.cyan),
            new(5, "品红", width * 7 / 8, height / 2, width / 16, height / 16, Color.magenta),
            new(6, "紫", width / 2, height / 8, width / 16, height / 16, new Color(.5f, 0, 1)),
            new(7, "橙", width / 2, height * 7 / 8, width / 16, height / 16, new Color(1, .5f, 0)),
        };
        foreach (var player in players)
            for (var x = 0; x < player.width; x++)
                for (var y = 0; y < player.height; y++)
                {
                    ref var node = ref map.nodes[player.x + x, player.y + y];
                    node.player = player.id;
                    node.state = node.next = Map.State.Source;
                }
    }
    private void GetStartPosition(ref Enegry enegry, int playerId)
    {
        var player = players[playerId];
        int x, y, tx, ty;
        var dir = Random.value > .5f;
        if (Random.Range(0, player.width + player.height) < player.width)
        {
            x = player.x + Random.Range(0, player.width);
            y = player.y + (dir ? 0 : player.height - 1);
            tx = x;
            ty = dir ? y - 1 : y + 1;
        }
        else
        {
            x = player.x + (dir ? 0 : player.width - 1);
            y = player.y + Random.Range(0, player.height);
            tx = dir ? x - 1 : x + 1;
            ty = y;
        }
        enegry.sx = x;
        enegry.sy = y;
        enegry.tx = tx;
        enegry.ty = ty;
    }
    public void AddEnegry(int player, int value)
    {
        if (players[player].hp <= 0) return;
        var enegry = new Enegry(player, value);
        GetStartPosition(ref enegry, player);
        enegry.UpdatePV();
        enegry.enegry = GetEnegry();
        enegry.enegry.SetData(enegry);
        players[player].soldier++;
        enegries.Add(enegry);
    }
    private void HitPlayer(int source, int target, int damage)
    {
        var targetPlayer = players[target];
        if (targetPlayer.hp > damage) targetPlayer.hp -= damage;
        else
        {
            targetPlayer.hp = 0;
            damage -= targetPlayer.hp;
            var count = targetPlayer.width * targetPlayer.height;
            var value = damage / count;
            count = (int)(damage % count);
            for (var x = 0; x < targetPlayer.width; x++)
                for (var y = 0; y < targetPlayer.height; y++)
                {
                    ref var node = ref map.nodes[targetPlayer.x + x, targetPlayer.y + y];
                    node.px = node.py = -1;
                    ChangeNodePlayer(node.x, node.y, source);
                    node.state = node.next = Map.State.Death;
                    node.value = value;
                    if (count-- > 0) node.value++;
                }
        }
    }
    private struct NodeCandidate
    {
        public int x, y;
        public int width;
        public NodeCandidate(int x, int y, int width)
        {
            this.x = x;
            this.y = y;
            this.width = width;
        }
    }
    private readonly List<NodeCandidate> candidates = new();
    private int AddCandidate(int x, int y, Enegry enegry)
    {
        if (x < 0 || y < 0 || x >= map.width || y >= map.height) return 0;
        var node = map.nodes[x, y];
        if (node.state == Map.State.Source && node.player == enegry.player) return 0;
        NodeCandidate candidate;
        if (node.px == enegry.tx && node.py == enegry.ty)
            candidate = new NodeCandidate(x, y, 300);
        else if (node.player != enegry.player)
            candidate = new NodeCandidate(x, y, 200);
        else if (node.state == Map.State.Death)
        {
            var tNode = map.nodes[enegry.tx, enegry.ty];
            if (tNode.px == x && tNode.py == y) return 0;
            candidate = new NodeCandidate(x, y, 600);
        }
        else return 0;
        candidate.width += (int)(node.pheromone * 100);
        candidates.Add(candidate);
        return candidate.width;
    }
    private void ChangeNodePlayer(int x, int y, int player)
    {
        ref var node = ref map.nodes[x, y];
        if (player >= 0) players[player].territory++;
        if (node.player >= 0) players[node.player].territory--;
        node.player = player;
    }
    private void EnegryUpdate()
    {
        for (var i = 0; i < enegries.Count; i++)
        {
            var enegry = enegries[i];
            var dis = new Vector2(enegry.tx, enegry.ty) - enegry.position;
            if (dis.sqrMagnitude < enegry.velocity.sqrMagnitude)
            {
                ref var node = ref map.nodes[enegry.tx, enegry.ty];
                if (node.player != enegry.player)
                {
                    if (node.state == Map.State.Source)
                        HitPlayer(enegry.player, node.player, enegry.value);
                    else
                    {
                        if (node.value < enegry.value)
                        {
                            node.value = enegry.value - node.value;
                            ChangeNodePlayer(node.x, node.y, enegry.player);
                            node.px = enegry.sx;
                            node.py = enegry.sy;
                            if (map.nodes[node.px, node.py].state == Map.State.Death)
                                node.state = node.next = Map.State.Death;
                            else
                                node.state = node.next = Map.State.Alive;
                            if (node.value > MAX_NODE_VALUE)
                            {
                                enegry.value = node.value - MAX_NODE_VALUE;
                                node.value = MAX_NODE_VALUE;
                                goto label_next_node;
                            }
                        }
                        else node.value -= enegry.value;
                    }
                    goto label_remove_enegry;
                }
                else if (node.state != Map.State.Source)
                {
                    if (node.value < MAX_NODE_VALUE)
                    {
                        node.value += enegry.value;
                        enegry.value = node.value - MAX_NODE_VALUE;
                        if (node.value > MAX_NODE_VALUE) node.value = MAX_NODE_VALUE;
                    }
                    if (node.state == Map.State.Death && enegry.forward)
                    {
                        node.px = enegry.sx;
                        node.py = enegry.sy;
                    }
                    if (node.enegry > 0 && !enegry.harvest)
                    {
                        enegry.harvest = true;
                        node.enegry--;
                        node.node.UpdateNode(node);
                        enegry.forward = false;
                    }
                }
            label_next_node:
                if (enegry.value <= 0) goto label_remove_enegry;
                if (enegry.forward)
                {
                    var width = 0;
                    width += AddCandidate(enegry.tx + 1, enegry.ty + 0, enegry);
                    width += AddCandidate(enegry.tx - 1, enegry.ty + 0, enegry);
                    width += AddCandidate(enegry.tx + 0, enegry.ty + 1, enegry);
                    width += AddCandidate(enegry.tx + 0, enegry.ty - 1, enegry);

                    width += AddCandidate(enegry.tx - 1, enegry.ty - 1, enegry);
                    width += AddCandidate(enegry.tx + 1, enegry.ty - 1, enegry);
                    width += AddCandidate(enegry.tx - 1, enegry.ty + 1, enegry);
                    width += AddCandidate(enegry.tx + 1, enegry.ty + 1, enegry);
                    if (candidates.Count > 0)
                    {
                        enegry.sx = enegry.tx;
                        enegry.sy = enegry.ty;
                        width = Random.Range(0, width);
                        foreach (var candidate in candidates)
                        {
                            width -= candidate.width;
                            if (width < 0)
                            {
                                enegry.tx = candidate.x;
                                enegry.ty = candidate.y;
                                break;
                            }
                        }
                    }
                    else
                    {
                        enegry.Back();
                    }
                    candidates.Clear();
                }
                else
                {
                    if (node.state == Map.State.Source)
                    {
                        GetStartPosition(ref enegry, enegry.player);
                        enegry.forward = true;
                        if (enegry.harvest)
                        {
                            enegry.harvest = false;
                            players[enegry.player].hp++;
                            AddEnegry(enegry.player, enegry.value);
                        }
                    }
                    else if (node.px < 0 || node.py < 0)
                    {
                        enegry.forward = true;
                        goto label_next_node;
                    }
                    else
                    {
                        (enegry.sx, enegry.sy) = (enegry.tx, enegry.ty);
                        (enegry.tx, enegry.ty) = (node.px, node.py);
                    }
                }
                enegry.UpdatePV();
                enegry.enegry.UpdateColor(enegry);
                if (enegry.forward) node.pheromone++;
                else if (enegry.harvest) node.pheromone += 100;
                else node.pheromone = Mathf.Max(0, node.pheromone - 1);
            }
            else enegry.position += enegry.velocity;
            enegry.enegry.transform.position = enegry.position;
            enegries[i] = enegry;
            continue;
        label_remove_enegry:
            Recycle(enegry.enegry);
            players[enegry.player].soldier--;
            enegries[i--] = enegries[^1];
            enegries.RemoveAt(enegries.Count - 1);
        }
    }
    public void Update()
    {
        EnegryUpdate();
        map.Update();
    }
    private global::Enegry GetEnegry()
    {
        if (pool.Count > 0) return pool.Pop();
        var result = Object.Instantiate(prefab);
        result.transform.SetParent(GameMgr.Instance.enegryRoot);
        return result;
    }
    private void Recycle(global::Enegry enegry)
    {
        enegry.gameObject.SetActive(false);
        pool.Push(enegry);
    }

    public void Dispose()
    {
        while (pool.Count > 0)
            Object.Destroy(pool.Pop().gameObject);
        foreach (var enegry in enegries)
            Object.Destroy(enegry.enegry.gameObject);
        map.Dispose();
    }
}
