using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameMgr : MonoBehaviour
{
    public Battle battle;
    public Node nodePrefab;
    public Enegry enegryPrefab;
    public Transform mapRoot, enegryRoot;
    public float paracargoCD;
    public Camera Camera;
    public Camera EnegryCamera;
    public event Action OnRestart;
    private void Start()
    {
        Instance = this;
        Restart(48);
    }
    private void FixedUpdate()
    {
        battle.Update();
        foreach (var player in battle.players)
        {
            foreach (var core in player.cores)
                if (core.hp > 0)
                {
                    if (core.stringentState)
                        for (var i = 0; i < 50 && core.stringentState; i++)
                            if (core.hp-- > 100) battle.AddEnegry(player.id);
                            else core.stringentState = false;
                }
            if (player.cores.Count > 0)
            {
                player.spawn += Mathf.Sqrt(Mathf.Max(Mathf.Log10(player.HP + 10) + Mathf.Log10(player.territory + 10), 1));
                while (player.spawn-- > 0)
                    battle.AddEnegry(player.id);
            }
        }

        paracargoCD -= Time.deltaTime;
        if (paracargoCD < 0)
        {
            paracargoCD += Random.Range(.5f, 2);
            while (true)
            {
                var x = Random.Range(0, battle.map.width);
                var y = Random.Range(0, battle.map.height);
                var node = battle.map.nodes[x, y];
                if (node.state != Map.State.Source && node.state != Map.State.Obstacle)
                {
                    node.node.AddEnegry(Random.Range(400, 600));
                    break;
                }
            }
        }
    }
    public void Restart(int size)
    {
        battle?.Dispose();
        battle = new Battle(size, size, enegryPrefab, nodePrefab);
        Camera.transform.position = new Vector3(battle.map.width - 1, battle.map.height - 1, -20) * .5f;
        EnegryCamera.orthographicSize = Camera.orthographicSize = battle.map.height * .5f;
        OnRestart?.Invoke();
    }
    public static GameMgr Instance { get; private set; }
}
