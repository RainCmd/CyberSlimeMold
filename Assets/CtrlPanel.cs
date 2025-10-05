using UnityEngine;
using UnityEngine.UI;

public class CtrlPanel : MonoBehaviour
{
    public Dropdown selectPlayer;
    public Image color;
    public Text mapSizeText;
    public Slider mapSizeSlider;
    public void OnSelectPlayer(int player)
    {
        color.color = GameMgr.Instance.battle.players[player].color;
    }
    private void Start()
    {
        var players = GameMgr.Instance.battle.players;
        foreach (var player in players)
            selectPlayer.options.Add(new Dropdown.OptionData(player.name));
        OnMapSizeChanged(mapSizeSlider.value);
    }
    public void AddSoldier(int count)
    {
        while (count-- > 0)
            GameMgr.Instance.battle.AddEnegry(selectPlayer.value);
    }
    public void AddHP(int hp)
    {
        var player = GameMgr.Instance.battle.players[selectPlayer.value];
        foreach (var core in player.cores)
            core.hp += hp;
    }
    public void OnMapSizeChanged(float value)
    {
        var size = (int)value;
        mapSizeText.text = $"地图大小:{size}x{size}";
    }
    public void OnRestart()
    {
        GameMgr.Instance.Restart((int)mapSizeSlider.value);
    }
    public void OnQuitClick()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
