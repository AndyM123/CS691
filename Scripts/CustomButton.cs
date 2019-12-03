using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    private Color _initialColor;
    private void Start () {
        _initialColor = GetComponent<Image> ().color;
    }
    public void OnPointerEnter (PointerEventData eventData) {
        GetComponent<Image> ().color = Color.cyan;
    }
    public void OnPointerExit (PointerEventData eventData) {
        GetComponent<Image> ().color = _initialColor;
    }

}