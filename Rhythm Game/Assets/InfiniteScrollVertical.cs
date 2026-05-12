using UnityEngine;
using UnityEngine.UI;

public class InfiniteScrollVertical : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform viewPortTransform;
    public RectTransform contentPanelTransform;
    public VerticalLayoutGroup VLG;

    //For items in the scroll view
    public RectTransform[] ItemList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Duplicate items
        //Calculate the number of required items: Divide length of view port by the total space for a single item
        //Total space required for one item is the sum of its height & the layer Group Spacing
        //Mathf.CeilToInt() to round up
        int ItemsToAdd = Mathf.CeilToInt(viewPortTransform.rect.height / (ItemList[0].rect.height + VLG.spacing));

        //For duplicating items in the bottom
        for (int i = 0; i < ItemsToAdd; i++)
        {
            //Write on the top , instantiate from 1st to last & set as last sibling
            RectTransform RT = Instantiate(ItemList[i % ItemList.Length], contentPanelTransform);
            RT.SetAsLastSibling();
        }

        //For duplicating items on the top (first sibling)
        for (int i = 0; i < ItemsToAdd; i++)
        {
            //Add items from last to first & set as first sibling
            int num = ItemList.Length - i - 1;

            while (num < 0)
            {
                num += ItemList.Length;
            }

            RectTransform RT = Instantiate(ItemList[num], contentPanelTransform);
            RT.SetAsFirstSibling();
        }



        //Set position of content panel so original items are shown in the front at the start of the scene
        contentPanelTransform.anchoredPosition = new Vector2(0, ItemList.Length * (ItemList[0].rect.height + VLG.spacing));
    }

    // Update is called once per frame
    void Update()
    {
        //Loop scroll view
        float itemHeight = ItemList[0].rect.height + VLG.spacing;
        float totalOriginalHeight = ItemList.Length * itemHeight;

        Vector2 pos = contentPanelTransform.anchoredPosition;

        //Scrolled too far down, passed bottom duplicates
        if (pos.y > totalOriginalHeight)
        {
            //Move up by totalOriginalHeight
            pos.y -= totalOriginalHeight;
            contentPanelTransform.anchoredPosition = pos;
        }

        //Scrolled too far up, pass top duplicates
        else if (pos.y < 0)
        {
            //Move down by totalOriginalHeight
            pos.y += totalOriginalHeight;
            contentPanelTransform.anchoredPosition = pos;
        }


        
    }
}
