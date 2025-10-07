using System.Collections.Generic;
using UnityEngine;

public class InfoPanel : MonoBehaviour
{
    public PlayerInfo prefab;
    public Transform infoContent;
    public RotatingDisc rotatingDiscPrefab;
    public Transform rotatingDiscContent;
    private readonly List<PlayerInfo> infos = new();
    private readonly List<RotatingDisc> rotatingDiscs = new();
    private void Start()
    {
        foreach (var player in GameMgr.Instance.battle.players)
        {
            var info = Instantiate(prefab, infoContent);
            info.playerID = player.id;
            info.gameObject.SetActive(true);
            infos.Add(info);
            var rotatingDisc = Instantiate(rotatingDiscPrefab, rotatingDiscContent);
            rotatingDisc.playerId = player.id;
            rotatingDisc.gameObject.SetActive(true);
            rotatingDiscs.Add(rotatingDisc);
        }
        GameMgr.Instance.OnRestart += Instance_OnRestart;
    }

    private void Instance_OnRestart()
    {
        foreach (var info in infos)
        {
            info.transform.SetAsLastSibling();
            info.defeat = false;
        }
        foreach (var rotating in rotatingDiscs)
        {
            rotating.transform.SetAsLastSibling();
        }
    }
    private void OnDestroy()
    {
        GameMgr.Instance.OnRestart -= Instance_OnRestart;
    }
}
