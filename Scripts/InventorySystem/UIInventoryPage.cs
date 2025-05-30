using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInventoryPage : MonoBehaviour
{
    [SerializeField] 
    private UIInventoryItem itemPrefab;
    [SerializeField] 
    private RectTransform contentPanel;
    List<UIInventoryItem> listOfUIItems = new List<UIInventoryItem>();
    public void InitializeInventorySize(int size)
    {
        for (int i = 0; i < size; i++)
        {
            UIInventoryItem uiItem = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
            uiItem.transform.SetParent(contentPanel);
            uiItem.transform.localScale = Vector3.one;
            listOfUIItems.Add(uiItem);
        }
    }
    public void Show(){
        gameObject.SetActive(true);
    }
    public void Hide(){
        gameObject.SetActive(false);
    }
}
