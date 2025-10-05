using System;
using UnityEngine;
using UnityEngine.UI;

public class BarDiagram : MonoBehaviour
{
    public Image prefab;
    private Image[] images;
    public string property;
    private Func<int>[] gets;
    private int[] values;
    private void Start()
    {
        var players = GameMgr.Instance.battle.players;
        images = new Image[players.Length];
        values = new int[players.Length];
        gets = new Func<int>[players.Length];
        foreach (var player in GameMgr.Instance.battle.players)
        {
            var image = Instantiate(prefab, transform);
            image.color = player.color;
            image.gameObject.SetActive(true);
            image.transform.SetAsFirstSibling();
            images[player.id] = image;
        }
        OnRestart();
        GameMgr.Instance.OnRestart += OnRestart;
    }
    public void OnRestart()
    {
        var method = typeof(Player).GetMethod("get_" + property);
        foreach (var player in GameMgr.Instance.battle.players)
            gets[player.id] = (Func<int>)Delegate.CreateDelegate(typeof(Func<int>), player, method);
    }
    public void Update()
    {
        for (int i = 0; i < values.Length; i++)
            values[i] = gets[i].Invoke();
        var total = 0;
        foreach (var v in values)
            total += v;
        var fillAmount = 0f;
        for (int i = 0; i < values.Length; i++)
        {
            fillAmount += (float)values[i] / total;
            images[i].fillAmount = fillAmount;
        }
    }
}
