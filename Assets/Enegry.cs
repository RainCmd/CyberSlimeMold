using UnityEngine;

public class Enegry : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public void SetData(Battle.Enegry enegry)
    {
        UpdateColor(enegry);
        transform.position = new Vector3(enegry.sx, enegry.sy);
        gameObject.SetActive(true);
    }
    public void UpdateColor(Battle.Enegry enegry)
    {
        var color = GameMgr.Instance.battle.players[enegry.player].energyColor;
        if (enegry.harvest) color = color * .75f + new Color(.25f, .25f, .25f);
        else if (!enegry.forward) color *= .75f;
        spriteRenderer.color = color;
    }
}
