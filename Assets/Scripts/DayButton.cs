using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DayButton : MonoBehaviour
{
    private Image theButton;
    public int TimeOfDay;
    public int dialPosition;
    public DayButton[] buttonArray;
    enum Time { Morning, Noon, Night };
    public Image timeImage;
    public Sprite[] shownSprite;


    // Use this for initialization
    void Start()
    {
        theButton = GetComponent<Image>();
        theButton.alphaHitTestMinimumThreshold = 1f;

        SetColor();
    }

    public void SetColor()
    {
        Color morningColor = new Color(.9f, .7f, .5f);
        Color noonColor = Color.yellow;//new Color(.8f, .1f, .3f);
        Color nightColor = new Color(.6f, .9f, .9f);

        timeImage.GetComponent<Image>().sprite = shownSprite[TimeOfDay];

        switch (TimeOfDay)
        {
            case (int)Time.Morning:
                theButton.color = morningColor;
                break;
            case (int)Time.Noon:
                theButton.color = noonColor;
                break;
            default: //Time.Night
                theButton.color = nightColor;
                break;
        }
    }

    public void ChangeTime()
    {
        DayButton dayButtonScript;
        if (dialPosition == -1)
        {
            foreach (DayButton dial in buttonArray)
            {
                dayButtonScript = dial.GetComponent<DayButton>();
                dayButtonScript.ReverseTime();
            }
        } else if (dialPosition == 1)
        {
            foreach (DayButton dial in buttonArray)
            {
                dayButtonScript = dial.GetComponent<DayButton>();
                dayButtonScript.AdvanceTime();
            }
        }
    }

    public void AdvanceTime()
    {
        TimeOfDay++;
        if (TimeOfDay > 2) { TimeOfDay = 0; }
        SetColor();
    }

    public void ReverseTime()
    {
        TimeOfDay--;
        if (TimeOfDay < 0) { TimeOfDay = 2; }
        SetColor();
    }
}
