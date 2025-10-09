using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoreHPPanel : MonoBehaviour
{
    public Text prefab;
    private readonly List<Text> infos = new();
    private void Update()
    {
        var count = 0;
        foreach (var player in GameMgr.Instance.battle.players)
            foreach (var core in player.cores)
                count++;
        if (count < infos.Count)
        {
            for (var i = count; i < infos.Count; i++)
                Destroy(infos[i].gameObject);
            infos.RemoveRange(count, infos.Count - count);
        }
        else if (count > infos.Count)
        {
            for (var i = infos.Count; i < count; i++)
            {
                var info = Instantiate(prefab, transform);
                info.gameObject.SetActive(true);
                infos.Add(info);
            }
        }

        var index = 0;
        foreach (var player in GameMgr.Instance.battle.players)
            foreach (var core in player.cores)
                SetInfo(core, infos[index++]);
    }
    private void SetInfo(Player.Core core, Text info)
    {
        info.transform.position = GameMgr.Instance.Camera.WorldToScreenPoint(core.center);
        info.text = RotatingDisc.FormatNumber(core.hp);
    }
}
