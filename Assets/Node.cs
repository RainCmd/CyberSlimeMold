using UnityEngine;

public class Node : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer pathRenderer;
    public Transform path;
    public Map.Node node;

    public void AddEnegry(int value)
    {
        ref var node = ref GameMgr.Instance.battle.map.nodes[this.node.x, this.node.y];
        node.enegry += value;
        UpdateNode(node);
    }

    public void UpdateNode(Map.Node node)
    {
        if (node.player != this.node.player || node.state != this.node.state || (node.enegry == 0 && this.node.enegry > 0))
        {
            if (node.state == Map.State.Obstacle) spriteRenderer.color = Color.clear;
            else if (node.player < 0) spriteRenderer.color = Color.black;
            else
            {
                var player = GameMgr.Instance.battle.players[node.player];
                switch (node.state)
                {
                    case Map.State.Alive:
                        spriteRenderer.color = player.territoryColor;
                        break;
                    case Map.State.Death:
                        spriteRenderer.color = player.territoryColor * .5f;
                        break;
                    case Map.State.Source:
                        spriteRenderer.sortingOrder = 1;
                        spriteRenderer.color = player.color;
                        break;
                    default:
                        break;
                }
            }
            pathRenderer.color = spriteRenderer.color * .5f + new Color(.5f, .5f, .5f, .5f);
        }
        if (node.state != this.node.state || node.px != this.node.px || node.py != this.node.py)
        {
            if (node.state == Map.State.Source || node.state == Map.State.Obstacle || node.px < 0 || node.py < 0) path.gameObject.SetActive(false);
            else
            {
                path.gameObject.SetActive(true);
                var dir = new Vector2(node.px, node.py) - (Vector2)transform.position;
                path.transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg, new Vector3(0, 0, 1));
                path.transform.localScale = new Vector3(dir.magnitude, .1f, 1);
            }
        }
        if (node.enegry > 0)
        {
            var color = node.player < 0 ? Color.black : GameMgr.Instance.battle.players[node.player].color;
            color = color * .5f + new Color(.5f, .5f, .5f);
            color.a = 1;
            spriteRenderer.color = color;
        }
        this.node = node;
    }
}
