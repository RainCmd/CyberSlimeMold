using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class BarDiagram : MonoBehaviour
{
    public Image prefab;
    private Image[] images;
    public string field;
    private FieldInfo fieldInfo;
    private RectTransform rt;
    private int[] values;
    private void Start()
    {
        fieldInfo = typeof(Player).GetField(field);
        rt = transform as RectTransform;
        var players = GameMgr.Instance.battle.players;
        images = new Image[players.Length];
        values = new int[players.Length];
        foreach (var player in players)
        {
            var image = Instantiate(prefab, transform);
            image.color = player.color;
            image.gameObject.SetActive(true);
            image.transform.SetAsFirstSibling();
            images[player.id] = image;
        }
    }
    private void UpdateValues()
    {
        var players = GameMgr.Instance.battle.players;
        for (int i = 0; i < players.Length; i++)
            values[i] = (int)fieldInfo.GetValue(players[i]);
    }
    public void Update()
    {
        UpdateValues();
        var width = rt.rect.width;
        var total = 0;
        foreach (var v in values)
            total += v;
        for (int i = 0; i < values.Length; i++)
        {
            var rt = images[i].rectTransform;
            var sd = rt.sizeDelta;
            sd.x = width * values[i] / total;
            rt.sizeDelta = sd;
        }
    }
}
