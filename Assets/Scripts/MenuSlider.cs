using UnityEngine;
using System.Collections;

public class MenuSlider : MonoBehaviour
{
    public float m_SlideSpeed = 1.0f;
    public float m_MinDistance = 50.0f;

    private bool m_IsDragging = false;

    private float m_StartMousePosX = 0.0f;  //Used to check the minimum distnace
    private float m_LastMousePosX = 0.0f;   //Used for moving the menu

    private RectTransform m_RectTransform;

    private void Awake()
    {
        m_RectTransform = gameObject.GetComponent<RectTransform>();
    }

	// Update is called once per frame
	private void Update ()
    {
        Vector3 virtualKeyPosition = Vector3.zero;

        //Get the input
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (Input.touchCount > 0)
            {
                virtualKeyPosition = new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y);
                Swipe(virtualKeyPosition);
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0) || Input.GetMouseButton(0))
            {
                virtualKeyPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
                Swipe(virtualKeyPosition);
            }
        }
	}

    public void Close()
    {
        StartCoroutine(Slide(-1.0f));
    }

    private void Swipe(Vector3 virtualKeyPosition)
    {
        float dist = virtualKeyPosition.x - m_LastMousePosX;
        m_LastMousePosX = virtualKeyPosition.x;

        //Left mouse down: Start swiping
        if (Input.GetMouseButtonDown(0))
        {
            m_StartMousePosX = virtualKeyPosition.x;
            m_LastMousePosX = virtualKeyPosition.x;
        }

        //Left mouse up: stop swiping
        else if (Input.GetMouseButtonUp(0))
        {
            if (m_IsDragging) StartCoroutine(Slide(Mathf.Sign(dist)));
        }

        //Hold left mouse button: Swipe
        else if (Input.GetMouseButton(0))
        {
            if (!m_IsDragging && (Mathf.Abs(virtualKeyPosition.x - m_StartMousePosX) > m_MinDistance))
            {
                bool temp = true;

                //Only start Draggin if we were close to the edge when we started swiping LAME FIX
                if (m_RectTransform.transform.position.x == -m_RectTransform.sizeDelta.x)
                {
                    temp = false;

                    if (m_StartMousePosX < m_MinDistance)
                    {
                        temp = true;
                    }
                }
                
                //If we're dragging a unit, this is always false
                if (Unit.m_DraggedUnit != null) temp = false;

                if (temp)
                {
                    m_IsDragging = true;
                    StopAllCoroutines();
                }
            }

            if (m_IsDragging)
            {
                //Calculate new position
                float width = m_RectTransform.sizeDelta.x;

                float xPos = m_RectTransform.transform.position.x;
                xPos += dist;
            
                //Don't let the values go out of bounds
                if (xPos > 0.0f)    xPos = 0.0f;
                if (xPos < -width) xPos = -width;

                //Actually set the position
                m_RectTransform.transform.position = new Vector3(xPos, m_RectTransform.transform.position.y, 0.0f);
            }
        }
    }

    private IEnumerator Slide(float dir)
    {
        float width = m_RectTransform.sizeDelta.x;
        m_IsDragging = false;

        bool isRunning = true;
        while (isRunning) 
        {
            float xPos = m_RectTransform.transform.position.x + (dir * m_SlideSpeed * Time.deltaTime);

            //Don't let the values go out of bounds
            if (dir > 0.0f && xPos > 0.0f)
            {
                xPos = 0.0f;
                isRunning = false;
            }

            if (dir < 0.0f && xPos < -width)
            {
                xPos = -width;
                isRunning = false;
            }

            m_RectTransform.transform.position = new Vector3(xPos, m_RectTransform.transform.position.y, 0.0f);
            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }
}
