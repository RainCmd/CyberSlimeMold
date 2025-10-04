using UnityEngine;

public class InfoPanel : MonoBehaviour
{
    public PlayerInfo prefab;
    public Transform infoContent;
    private void Start()
    {
        foreach (var player in GameMgr.Instance.battle.players)
        {
            var info = Instantiate(prefab, infoContent);
            info.playerID = player.id;
            info.gameObject.SetActive(true);
        }
    }
}
