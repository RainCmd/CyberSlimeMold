using UnityEngine;
using UnityEngine.UI;

public class PlayerInfo : MonoBehaviour
{
    public int playerID;
    public Image color;
    public Text HP;
    public Text territory;
    public Text soldier;
    private void Update()
    {
        var battle = GameMgr.Instance.battle;
        var player = battle.players[playerID];
        color.color = player.color;
        HP.text = "HP:" + player.hp.ToString();
        territory.text = "领地:" + player.territory.ToString();
        soldier.text = "粒子:" + player.soldier.ToString();
    }
}
