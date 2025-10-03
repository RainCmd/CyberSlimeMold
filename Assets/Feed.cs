using UnityEngine;
using UnityEngine.EventSystems;

public class Feed : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        var camera = FindObjectOfType<Camera>();
        if (camera)
        {
            var ray = camera.ScreenPointToRay(eventData.position);
            var hit = Physics2D.Raycast(ray.origin, ray.direction);
            if (!hit.collider) return;
            var node = hit.collider.GetComponent<Node>();
            if (!node || node.node.state == Map.State.Source) return;
            node.AddEnegry(100);
        }
    }
}
