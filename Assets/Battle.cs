using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class Player
{
    public readonly struct Candidate
    {
        public readonly int sx, sy;
        public readonly int tx, ty;

        public Candidate(int sx, int sy, int tx, int ty)
        {
            this.sx = sx;
            this.sy = sy;
            this.tx = tx;
            this.ty = ty;
        }
    }
    public class Core
    {
        public bool stringentState = false;
        public int hp = 1000;
        public Vector2 center;
        public readonly HashSet<Vector2Int> candidates = new();
    }
    public int id;
    public string name;
    public int x, y;
    public int width, height;
    public int territory;
    public int soldier;
    public float spawn;
    public Color color;
    public Color territoryColor;
    public Color energyColor;
    public readonly List<Candidate> candidates = new();
    public readonly List<Core> cores = new();
    public int HP
    {
        get
        {
            var hp = 0;
            foreach (var core in cores)
                hp += core.hp;
            return hp;
        }
    }
    public int Territory => territory;
    public int Soldier => soldier;
    public Player(int id, string name, int x, int y, int width, int height, Color color)
    {
        this.id = id;
        this.name = name;
        this.x = x - width / 2;
        this.y = y - height / 2;
        this.width = width;
        this.height = height;
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
        public const float SPEED = .15f;
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

        public void UpdatePV()
        {
            position = new Vector2(sx, sy);
            velocity = (new Vector2(tx, ty) - position).normalized * 0.15f;
        }
        public void SetForward(Battle battle, bool forward)
        {
            this.forward = forward;
            if (forward || harvest)
            {
                if (!enegry)
                {
                    enegry = battle.GetEnegry();
                    enegry.transform.position = position;
                }
            }
            else if (enegry)
            {
                battle.Recycle(enegry);
                enegry = null;
            }
        }
    }
    public const int MAX_NODE_VALUE = 2;
    public readonly Map map;
    public readonly Player[] players;
    public readonly List<Enegry> enegries = new();
    private readonly global::Enegry prefab;
    private readonly Stack<global::Enegry> pool = new();
    private readonly Dictionary<Vector2Int, Player.Core> cores = new();
    private int obstacle = 4;//当死了多少个玩家后消除中心障碍物
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
        {
            var core = new Player.Core();
            for (var x = 0; x < player.width; x++)
                for (var y = 0; y < player.height; y++)
                {
                    ref var node = ref map.nodes[player.x + x, player.y + y];
                    node.player = player.id;
                    node.state = node.next = Map.State.Source;
                    core.candidates.Add(new Vector2Int(node.x, node.y));
                    core.center += new Vector2(node.x, node.y);
                }
            core.center /= core.candidates.Count;
            player.cores.Add(core);

            UpdatePlayerCandidates(player);
        }
        SetObstacleArea(Map.State.Obstacle);
    }
    private void SetObstacleArea(Map.State state)
    {
        var offsetX = map.width / 4;
        var offsetY = map.height / 4;
        var obstacleAreaWidth = map.width / 2;
        var obstacleAreaHeight = map.height / 2;
        for (var x = 0; x < obstacleAreaWidth; x++)
            for (var y = 0; y < obstacleAreaHeight; y++)
            {
                ref var node = ref map.nodes[x + offsetX, y + offsetY];
                node.state = node.next = state;
            }
    }
    private void CreateCore(int x, int y, int width, int height)
    {
        x -= width / 2;
        y -= height / 2;
        var core = new Player.Core();
        for (var ix = 0; ix < width; ix++)
            for (var iy = 0; iy < height; iy++)
            {
                var pos = new Vector2Int(ix + x, iy + y);
                ref var node = ref map.nodes[pos.x, pos.y];
                node.state = node.next = Map.State.Source;
                core.candidates.Add(pos);
                core.center += new Vector2(pos.x, pos.y);
                cores[pos] = core;
            }
        core.center /= core.candidates.Count;
    }
    private void CreateNeutralityCore()
    {
        var width = map.width / 16;
        var height = map.height / 16;
        //CreateCore(map.width / 3, map.height / 3, width, height);
        //CreateCore(map.width / 3 * 2, map.height / 3 * 2, width, height);
        //CreateCore(map.width / 3 * 2, map.height / 3, width, height);
        //CreateCore(map.width / 3, map.height / 3 * 2, width, height);
        CreateCore(map.width / 2, map.height / 2, width, height);
    }
    private void AddCandidates(List<Player.Candidate> candidates, Vector2Int pos, int tx, int ty)
    {
        var state = map.nodes[tx, ty].state;
        if (state != Map.State.Source && state != Map.State.Obstacle)
            candidates.Add(new Player.Candidate(pos.x, pos.y, tx, ty));
    }
    private void UpdatePlayerCandidates(Player player)
    {
        player.candidates.Clear();
        foreach (var core in player.cores)
            foreach (var pos in core.candidates)
            {
                AddCandidates(player.candidates, pos, pos.x + 1, pos.y);
                AddCandidates(player.candidates, pos, pos.x - 1, pos.y);
                AddCandidates(player.candidates, pos, pos.x, pos.y + 1);
                AddCandidates(player.candidates, pos, pos.x, pos.y - 1);
            }
    }
    private void GetStartPosition(ref Enegry enegry, int playerId)
    {
        var player = players[playerId];
        var total = 0f;
        foreach (var candidate in player.candidates)
            total += Mathf.Log10(10 + map.nodes[candidate.tx, candidate.ty].pheromone);
        var result = player.candidates[0];
        var value = Random.Range(0, total);
        foreach (var candidate in player.candidates)
        {
            value -= Mathf.Log10(10 + map.nodes[candidate.tx, candidate.ty].pheromone);
            if (value <= 0)
            {
                result = candidate;
                break;
            }
        }
        enegry.sx = result.sx;
        enegry.sy = result.sy;
        enegry.tx = result.tx;
        enegry.ty = result.ty;
    }
    public void AddEnegry(int player, int value = MAX_NODE_VALUE / 2)
    {
        if (value <= 0) return;
        if (players[player].candidates.Count == 0) return;
        var enegry = new Enegry(player, value);
        GetStartPosition(ref enegry, player);
        enegry.UpdatePV();
        enegry.enegry = GetEnegry();
        enegry.enegry.SetData(enegry);
        players[player].soldier++;
        enegries.Add(enegry);
    }
    private void SetCorePlayer(Player.Core core, int player)
    {
        core.stringentState = false;
        core.hp = 100;
        foreach (var p in core.candidates)
            ChangeNodePlayer(p.x, p.y, player);
        players[player].cores.Add(core);
        UpdatePlayerCandidates(players[player]);
    }
    private void HitPlayer(int source, int target, int damage, Vector2Int pos)
    {
        if (target < 0)
        {
            if (cores.TryGetValue(pos, out var core))
                SetCorePlayer(core, source);
        }
        else
        {
            var targetPlayer = players[target];
            var idx = targetPlayer.cores.FindIndex(core => core.candidates.Contains(pos));
            if (idx >= 0)
            {
                var core = targetPlayer.cores[idx];
                if (core.hp > damage)
                {
                    core.hp -= damage;
                    core.stringentState = true;
                }
                else
                {
                    SetCorePlayer(core, source);
                    targetPlayer.cores.RemoveAt(idx);
                    UpdatePlayerCandidates(targetPlayer);
                    if (--obstacle == 0)
                    {
                        SetObstacleArea(Map.State.Death);
                        CreateNeutralityCore();
                    }
                }
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
        if (node.state == Map.State.Obstacle) return 0;
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
        var dot = Vector2.Dot(new Vector2(x - enegry.tx, y - enegry.ty), enegry.velocity / Enegry.SPEED);
        candidate.width += (int)(dot * 100);

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
    private bool IsParent(int x, int y, int px, int py)
    {
        if (x < 0 || y < 0 || x >= map.width || y >= map.height) return false;
        var node = map.nodes[x, y];
        return node.px == px && node.py == py;
    }
    private bool IsLink(int ax, int ay, int bx, int by)
    {
        if (ax < 0 || ay < 0 || ax >= map.width || ay >= map.height) return false;
        var node = map.nodes[ax, ay];
        if (node.px == bx && node.py == by) return false;
        if (bx < 0 || by < 0 || bx >= map.width || by >= map.height) return false;
        node = map.nodes[bx, by];
        if (node.px == ax && node.py == ay) return false;
        return true;
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
                    {
                        HitPlayer(enegry.player, node.player, enegry.value, new Vector2Int(node.x, node.y));
                        map.PathAddPheromone(enegry.sx, enegry.sy);
                    }
                    else
                    {
                        map.PathAddPheromone(enegry.sx, enegry.sy);
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
                    if (node.enegry > 0) map.PathAddPheromone(node.x, node.y);
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
                        enegry.SetForward(this, false);
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

                    if (IsLink(enegry.tx - 1, enegry.ty, enegry.tx, enegry.ty - 1) || IsParent(enegry.tx - 1, enegry.ty - 1, enegry.tx, enegry.ty))
                        width += AddCandidate(enegry.tx - 1, enegry.ty - 1, enegry);
                    if (IsLink(enegry.tx + 1, enegry.ty, enegry.tx, enegry.ty - 1) || IsParent(enegry.tx + 1, enegry.ty - 1, enegry.tx, enegry.ty))
                        width += AddCandidate(enegry.tx + 1, enegry.ty - 1, enegry);
                    if (IsLink(enegry.tx - 1, enegry.ty, enegry.tx, enegry.ty + 1) || IsParent(enegry.tx - 1, enegry.ty + 1, enegry.tx, enegry.ty))
                        width += AddCandidate(enegry.tx - 1, enegry.ty + 1, enegry);
                    if (IsLink(enegry.tx + 1, enegry.ty, enegry.tx, enegry.ty + 1) || IsParent(enegry.tx + 1, enegry.ty + 1, enegry.tx, enegry.ty))
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
                        candidates.Clear();
                    }
                    else
                    {
                        (enegry.sx, enegry.tx) = (enegry.tx, enegry.sx);
                        (enegry.sy, enegry.ty) = (enegry.ty, enegry.sy);
                        enegry.SetForward(this, false);
                    }
                }
                else
                {
                    if (node.state == Map.State.Source)
                    {
                        GetStartPosition(ref enegry, enegry.player);
                        enegry.SetForward(this, true);
                        if (enegry.harvest)
                        {
                            enegry.harvest = false;
                            foreach (var core in players[enegry.player].cores)
                                if (core.candidates.Contains(new Vector2Int(node.x, node.y)))
                                {
                                    core.hp++;
                                    break;
                                }
                            AddEnegry(enegry.player, enegry.value);
                        }
                    }
                    else if (node.px < 0 || node.py < 0)
                    {
                        (enegry.sx, enegry.tx) = (enegry.tx, enegry.sx);
                        (enegry.sy, enegry.ty) = (enegry.ty, enegry.sy);
                        enegry.SetForward(this, true);
                    }
                    else
                    {
                        (enegry.sx, enegry.sy) = (enegry.tx, enegry.ty);
                        (enegry.tx, enegry.ty) = (node.px, node.py);
                    }
                }
                enegry.UpdatePV();
                if (enegry.enegry)
                    enegry.enegry.UpdateColor(enegry);
                if (enegry.forward) node.pheromone++;
                else if (enegry.harvest) node.pheromone += 100;
                else node.pheromone = Mathf.Max(0, node.pheromone - 1);
            }
            else enegry.position += enegry.velocity;
            if (enegry.enegry)
                enegry.enegry.transform.position = enegry.position;
            enegries[i] = enegry;
            continue;
        label_remove_enegry:
            if (enegry.enegry)
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
            if (enegry.enegry)
                Object.Destroy(enegry.enegry.gameObject);
        map.Dispose();
    }
}
