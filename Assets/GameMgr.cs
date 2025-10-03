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
        for (int i = 0; i < 4; i++)
        {
            if (battle.players[i].hp > 0)
                battle.AddEnegry(i, 50);
        }
        paracargoCD -= Time.deltaTime;
        if (paracargoCD < 0)
        {
            paracargoCD += Random.Range(2, 5);
            (var x, var y) = GetParacargoPosition();
            battle.map.nodes[x, y].node.AddEnegry(Random.Range(100, 1000));
        }
    }
    private (int, int) GetParacargoPosition()
    {
        var wr = battle.map.width / 2;
        var hr = battle.map.height / 2;
        return (Random.Range(0, wr) + battle.map.width / 4, Random.Range(0, hr) + battle.map.height / 4);
    }
    public void Restart(int size)
    {
        battle?.Dispose();
        battle = new Battle(size, size, enegryPrefab, nodePrefab);
        Camera.transform.position = new Vector3(battle.map.width - 1, battle.map.height - 1, -20) * .5f;
        Camera.orthographicSize = battle.map.height / 2;
    }
    public static GameMgr Instance { get; private set; }
}
