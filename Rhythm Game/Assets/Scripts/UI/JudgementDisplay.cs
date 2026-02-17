using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class JudgementDisplay : MonoBehaviour
{
    [Header("UI References")]
    public Text judgementText;
    public Text comboText;
    
    [Header("Display Settings")]
    public float displayDuration = 0.5f;
    public float fadeOutDuration = 0.2f;
    
    [Header("Judgement Colors")]
    public Color perfectColor = new Color(1f, 0.84f, 0f);  // Gold
    public Color greatColor = new Color(0f, 1f, 0f);       // Green
    public Color goodColor = new Color(0f, 0.5f, 1f);      // Blue
    public Color okColor = new Color(1f, 0.5f, 0f);        // Orange
    public Color missColor = new Color(1f, 0f, 0f);        // Red
    
    private int currentCombo = 0;
    private Coroutine displayCoroutine;
    
    void Start()
    {
        if (judgementText != null)
            judgementText.enabled = false;
            
        UpdateComboDisplay();
    }
    
    public void ShowJudgement(string judgement)
    {
        if (judgementText == null) return;
        
        // Update combo
        if (judgement != "Miss")
        {
            currentCombo++;
        }
        else
        {
            currentCombo = 0;
        }
        
        UpdateComboDisplay();
        
        // Set judgement text and color
        judgementText.text = judgement;
        judgementText.color = GetJudgementColor(judgement);
        judgementText.enabled = true;
        
        // Restart display coroutine
        if (displayCoroutine != null)
            StopCoroutine(displayCoroutine);
        displayCoroutine = StartCoroutine(DisplayJudgementCoroutine());
    }
    
    Color GetJudgementColor(string judgement)
    {
        switch (judgement)
        {
            case "Perfect!": return perfectColor;
            case "Great!": return greatColor;
            case "Good": return goodColor;
            case "OK": return okColor;
            case "Miss": return missColor;
            default: return Color.white;
        }
    }
    
    IEnumerator DisplayJudgementCoroutine()
    {
        // Full opacity display
        Color originalColor = judgementText.color;
        originalColor.a = 1f;
        judgementText.color = originalColor;
        
        yield return new WaitForSeconds(displayDuration);
        
        // Fade out
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / fadeOutDuration);
            Color newColor = judgementText.color;
            newColor.a = alpha;
            judgementText.color = newColor;
            yield return null;
        }
        
        judgementText.enabled = false;
    }
    
    void UpdateComboDisplay()
    {
        if (comboText == null) return;
        
        if (currentCombo > 0)
        {
            comboText.text = $"{currentCombo} Combo";
            comboText.enabled = true;
        }
        else
        {
            comboText.enabled = false;
        }
    }
    
    public void ResetCombo()
    {
        currentCombo = 0;
        UpdateComboDisplay();
    }
}