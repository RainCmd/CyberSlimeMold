using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoreHPPanel : MonoBehaviour
{
    public Text prefab;
    private readonly List<Text> infos = new();
    private Camera cam;
    private void Start()
    {
        cam = FindAnyObjectByType<Camera>();
        if (!cam)
        {
            gameObject.SetActive(false);
            return;
        }
        foreach (var player in GameMgr.Instance.battle.players)
            foreach (var core in player.cores)
            {
                var info = Instantiate(prefab, transform);
                info.gameObject.SetActive(true);
                infos.Add(info);
                SetInfo(core, info);
            }
    }
    private void Update()
    {
        var index = 0;
        foreach (var player in GameMgr.Instance.battle.players)
            foreach (var core in player.cores)
            {
                SetInfo(core, infos[index++]);
            }
    }
    private void SetInfo(Player.Core core, Text info)
    {
        info.transform.position = cam.WorldToScreenPoint(core.center);
        info.text = RotatingDisc.FormatNumber(core.hp);
    }
}
