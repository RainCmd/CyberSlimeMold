using UnityEngine;

public class GameMgr : MonoBehaviour
{
    public Battle battle;
    public Node nodePrefab;
    public Enegry enegryPrefab;
    public Transform mapRoot, enegryRoot;
    public float paracargoCD;
    public Camera Camera;
    private void Start()
    {
        Instance = this;
        Restart(32);
    }
    private void FixedUpdate()
    {
        battle.Update();
        foreach (var player in battle.players)
            if (player.hp > 0)
                battle.AddEnegry(player.id, 50);
        paracargoCD -= Time.deltaTime;
        if (paracargoCD < 0)
        {
            paracargoCD += Random.Range(.5f, 2);
            while (true)
            {
                var x = Random.Range(0, battle.map.width);
                var y = Random.Range(0, battle.map.height);
                var node = battle.map.nodes[x, y];
                if (node.state != Map.State.Source)
                {
                    node.node.AddEnegry(Random.Range(300, 500));
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
        Camera.orthographicSize = battle.map.height * .5f;
    }
    public static GameMgr Instance { get; private set; }
}
