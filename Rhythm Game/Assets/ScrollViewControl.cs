using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ScrollViewControl : MonoBehaviour
{
    public ScrollRect scrollView;
    public float scrollSpeed = 50f;

    public RectTransform contentPanelTransform;
    public VerticalLayoutGroup VLG;
    public RectTransform[] ItemList;

    private int currentIndex = 0;

    private int topDuplicateCount;

    private float targetY;

    //For animation speed
    public float smoothSpeed = 10f;

    private float scrollVelocity;

    public float itemHeight;

    public float scrollStep;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        itemHeight = ItemList[0].rect.height + VLG.spacing;

        float viewportHeight = scrollView.viewport.rect.height;
        topDuplicateCount = Mathf.CeilToInt(viewportHeight / itemHeight);

        //Center first item
        CenterOnIndex(currentIndex, true);

    }

    // Update is called once per frame
    void Update()
    {
        //Check for input
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveDown();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveUp();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SelectItem();
        }

        Vector2 currentPos = contentPanelTransform.anchoredPosition;

        //currentPos.y = Mathf.Lerp(currentPos.y, targetY, Time.deltaTime * smoothSpeed);
        currentPos.y = Mathf.SmoothDamp(currentPos.y, targetY, ref scrollVelocity, 0.15f);

        contentPanelTransform.anchoredPosition = currentPos;

        //Keep targetY in sync if infinite scroll adjusted position
        float totalOriginalHeight = ItemList.Length * itemHeight;

        if (contentPanelTransform.anchoredPosition.y > totalOriginalHeight)
        {
            targetY -= totalOriginalHeight;
        }
        else if (contentPanelTransform.anchoredPosition.y < 0)
        {
            targetY += totalOriginalHeight;
        }
    }

    void MoveDown()
    {
        currentIndex = (currentIndex + 1) % ItemList.Length;
        CenterOnIndex(currentIndex, false);
    }

    void MoveUp()
    {
        currentIndex--;

        if (currentIndex < 0)
            currentIndex = ItemList.Length - 1;

        CenterOnIndex(currentIndex, false);
    }

    

    void CenterOnIndex(int index, bool instant)
    {
        float viewportHeight = scrollView.viewport.rect.height;
        float viewportCenterY = viewportHeight / 2f;

        float totalOriginalHeight = ItemList.Length * itemHeight;

        //Base visual index in middle block
        int baseVisualIndex = index + topDuplicateCount;

        float baseItemCenterY = baseVisualIndex * itemHeight + (itemHeight / 2f);
        float baseTargetY = baseItemCenterY - viewportCenterY;

        //Choose closest looped version
        float currentY = contentPanelTransform.anchoredPosition.y;

        float option1 = baseTargetY;
        float option2 = baseTargetY + totalOriginalHeight;
        float option3 = baseTargetY - totalOriginalHeight;

        //Pick closest to current position
        targetY = option1;

        float dist = Mathf.Abs(currentY - option1);

        if (Mathf.Abs(currentY - option2) < dist)
        {
            targetY = option2;
            dist = Mathf.Abs(currentY - option2);
        }

        if (Mathf.Abs(currentY - option3) < dist)
        {
            targetY = option3;
        }

        if (instant)
        {
            Vector2 pos = contentPanelTransform.anchoredPosition;
            pos.y = targetY;
            contentPanelTransform.anchoredPosition = pos;
        }

    }



    void SelectItem()
    {
        Debug.Log("Selecte index " + currentIndex);

        /*
        Button btn = ItemList[currentIndex].GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.Invoke();
        }
        */
    }
}
