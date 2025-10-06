using UnityEngine;

public class InfoPanel : MonoBehaviour
{
    public PlayerInfo prefab;
    public Transform infoContent;
    public RotatingDisc rotatingDiscPrefab;
    public Transform rotatingDiscContent;
    private void Start()
    {
        foreach (var player in GameMgr.Instance.battle.players)
        {
            var info = Instantiate(prefab, infoContent);
            info.playerID = player.id;
            info.gameObject.SetActive(true);
            var rotatingDisc = Instantiate(rotatingDiscPrefab, rotatingDiscContent);
            rotatingDisc.playerId = player.id;
            rotatingDisc.gameObject.SetActive(true);
        }
    }
}
