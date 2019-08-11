using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class SampleUniRx : MonoBehaviour
{
    private System.IDisposable m_Observer = null;

    private void Start()
    {
        OnSetObserver(true);
    }

    private void OnSetObserver(bool bEnable)
    {
        if (null != m_Observer)
        {
            m_Observer.Dispose();
            m_Observer = null;
        }

        if (false == bEnable)
            return;

        List<Vector2> listPosition = new List<Vector2>();

        m_Observer = Observable.EveryUpdate()

#if UNITY_EDITOR
            .Where(_ => (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0)))
            .Select(pos => new Vector2(Input.mousePosition.x, Input.mousePosition.y))
#else
            .Where(_ => Input.touchCount > 0)
            .Where(_ => (Input.GetTouch(0).phase == TouchPhase.Began) || (Input.GetTouch(0).phase == TouchPhase.Ended))
            .Select(pos => Input.GetTouch(0).position)            
#endif
            .Subscribe(positions =>
            {
                if (listPosition.Count == 0)
                {
#if UNITY_EDITOR
                    if (Input.GetMouseButtonUp(0))
#else
                    if((Input.GetTouch(0).phase == TouchPhase.Ended))
#endif
                        return;
                }

                listPosition.Add(positions);

                if (listPosition.Count < 2)
                    return;

                float fDelta = (listPosition[0] - listPosition[1]).magnitude;
                if (Mathf.Abs(fDelta) < 100.0f)
                    OnSetRay(listPosition);
                else
                    OnSwipe(listPosition);

                listPosition.Clear();
            });
    }

    private void OnSwipe(IList<Vector2> positions)
    {
        float fDelta = positions[0].x - positions[1].x;

        if(0f < fDelta)
            transform.Rotate(0f, 45f, 0f);
        else
            transform.Rotate(0f, -45f, 0f);       
    }

    private void OnSetRay(IList<Vector2> positions)
    {
        Ray ray = Camera.main.ScreenPointToRay(positions[0]);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            GameObject clickedObject = hit.collider.gameObject;
            Debug.Log(clickedObject.name);
        }
    }
}
