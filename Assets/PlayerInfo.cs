using UnityEngine;
using UnityEngine.UI;

public class PlayerInfo : MonoBehaviour
{
    public int playerID;
    public Image color;
    public Text HP;
    public Text territory;
    public Text soldier;
    public bool defeat = false;
    private void Update()
    {
        var battle = GameMgr.Instance.battle;
        var player = battle.players[playerID];
        color.color = player.color;
        HP.text = "HP:" + player.HP.ToString();
        territory.text = "领地:" + player.Territory.ToString();
        soldier.text = "粒子:" + player.Soldier.ToString();
        if (!defeat && player.HP == 0 && player.soldier == 0)
        {
            defeat = true;
            transform.SetAsLastSibling();
        }
    }
}
