using UnityEngine;
using UnityEngine.UI;

public class BuffPanel : MonoBehaviour
{
    public SystemAudioCapture capture;
    public Image scale;
    public Image prefab;
    private Image[] playerScales;
    private void Start()
    {
        var players = GameMgr.Instance.battle.players;
        playerScales = new Image[players.Length];
        foreach (var player in players)
        {
            var img = Instantiate(prefab, transform);
            img.gameObject.SetActive(true);
            img.color = player.color;
            playerScales[player.id] = img;
        }
    }
    private void Update()
    {
        var players = GameMgr.Instance.battle.players;
        var total = 0;
        foreach (var player in players)
            total += player.territory;
        var totalAmount = 0f;
        foreach (var player in players)
        {
            var amount = (float)player.territory / total;
            totalAmount += amount * amount;
        }
        totalAmount = Mathf.Sqrt(totalAmount);
        scale.fillAmount = totalAmount;

        var nodes = GameMgr.Instance.battle.map.nodes;
        for (int i = 0; i < players.Length; i++)
        {
            var amount = capture.SpectrumData[i];
            amount = Mathf.Lerp(playerScales[i].fillAmount, Mathf.Sqrt(amount), .15f);
            playerScales[i].fillAmount = amount;
            players[i].spawn += totalAmount * amount * 10;
            foreach (var core in players[i].cores)
                foreach (var pos in core.candidates)
                    nodes[pos.x, pos.y].node.transform.localScale = Vector3.one * (1 + amount * totalAmount * 2);
        }
    }
}
