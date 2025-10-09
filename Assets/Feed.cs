using UnityEngine;
using UnityEngine.EventSystems;

public class Feed : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        var ray = GameMgr.Instance.Camera.ScreenPointToRay(eventData.position);
        var hit = Physics2D.Raycast(ray.origin, ray.direction);
        if (!hit.collider) return;
        var node = hit.collider.GetComponent<Node>();
        if (!node || node.node.state == Map.State.Source || node.node.state == Map.State.Obstacle) return;
        node.AddEnegry(100);
    }
}
