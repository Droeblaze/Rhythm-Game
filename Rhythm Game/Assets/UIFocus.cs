using UnityEngine;
using UnityEngine.EventSystems;

public class UIFocus : MonoBehaviour
{
    public GameObject scrollMenu;
    public GameObject scrollButton;
    public GameObject paneButton;

    public ScrollViewControl scrollControl;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FocusPane()
    {
        scrollControl.inputLocked = true;

        EventSystem.current.SetSelectedGameObject(paneButton);
    }

    public void FocusScroll()
    {
        scrollControl.inputLocked = false;

        EventSystem.current.SetSelectedGameObject(scrollButton);
    }
}
