using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Unit : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static Unit m_DraggedUnit = null;
    private Vector3 m_StartPosition;
    private Transform m_StartParent;
    private Transform m_DragParent;

    public Transform OldParent
    {
        get { return m_StartParent; }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        m_DraggedUnit = this;
        m_StartPosition = transform.position;
        m_StartParent = transform.parent;
        m_DragParent = m_StartParent.transform.parent;

        //Loosen from field so we always render on top!
        transform.SetParent(m_DragParent);

        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData evnetData)
    {
        m_DraggedUnit = null;
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        if (transform.parent == m_DragParent)
        {
            transform.SetParent(m_StartParent);
            transform.position = m_StartPosition;
        }

        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f); //For some reason this resets
    }

    public void SetColor(Color color)
    {
        gameObject.GetComponent<Image>().color = color;
    }
}
