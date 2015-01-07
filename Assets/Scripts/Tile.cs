using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tile : MonoBehaviour, IDropHandler
{
    private Color m_PreviousColor;

    public void OnDrop(PointerEventData eventData)
    {
        ExecuteEvents.ExecuteHierarchy<IHasChanged>(gameObject, null, (x, y) => x.HasChanged(gameObject));
    }

    public void SetColor(Color color)
    {
        if (m_PreviousColor.a == 0.0f) //empty color
            m_PreviousColor = color;
        else                               
            m_PreviousColor = gameObject.GetComponent<Image>().color;

        gameObject.GetComponent<Image>().color = color;
    }

    public void ResetColor()
    {
        gameObject.GetComponent<Image>().color = m_PreviousColor;
    }
}
