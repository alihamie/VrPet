using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

#if !UNITY_WEBGL
public class SeekBarCtrl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler
{
    public MediaPlayerCtrl m_srcVideo;
    public Slider m_srcSlider;
    public float m_fDragTime = 0.2f;

    bool bActiveDrag = true;
    bool bUpdate = true;

    float fDeltaTime = 0.0f;
    float fLastValue = 0.0f;
    float fLastSetValue = 0.0f;

    void Update()
    {
        if (bActiveDrag == false)
        {
            fDeltaTime += Time.deltaTime;
            if (fDeltaTime > m_fDragTime)
            {
                bActiveDrag = true;
                fDeltaTime = 0.0f;
            }
        }

        if (bUpdate == false)
        {
            return;
        }

        if (m_srcVideo != null && m_srcSlider != null)
        {
            m_srcSlider.value = m_srcVideo.GetSeekBarValue();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        bUpdate = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        bUpdate = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        m_srcVideo.SetSeekBarValue(m_srcSlider.value);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (bActiveDrag == false)
        {
            fLastValue = m_srcSlider.value;
            return;
        }
        fLastSetValue = m_srcSlider.value;
        bActiveDrag = false;
    }
}
#endif