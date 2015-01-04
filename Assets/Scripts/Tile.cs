using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        ExecuteEvents.ExecuteHierarchy<IHasChanged>(gameObject, null, (x, y) => x.HasChanged(gameObject));
    }
}
