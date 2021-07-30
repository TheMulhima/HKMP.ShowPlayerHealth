using UnityEngine;
using UnityEngine.UI;

namespace PlayerHealth
{
    //this code was taken from jngo's Multiplayer mod.
    public class CanvasText
    {
        private GameObject textObj;
        private Vector2 size;

        public CanvasText(GameObject parent, Vector2 pos, Vector2 sz, Font font, string text, int fontSize = 13, FontStyle style = FontStyle.Normal, TextAnchor alignment = TextAnchor.UpperLeft)
        {
            if (sz.x == 0 || sz.y == 0)
            {
                size = new Vector2(Screen.width, Screen.height);
            }
            else
            {
                size = sz;
            }

            textObj = new GameObject("Canvas Text - " + text);
            textObj.AddComponent<CanvasRenderer>();
            RectTransform textTransform = textObj.AddComponent<RectTransform>();
            textTransform.sizeDelta = size;

            CanvasGroup group = textObj.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;

            Text t = textObj.AddComponent<Text>();
            t.text = text;
            t.font = font;
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.alignment = alignment;

            Outline outline = textObj.AddComponent<Outline>();
            outline.effectColor = Color.black;

            textObj.transform.SetParent(parent.transform, false);

            Vector2 position = new Vector2((pos.x + size.x / 2f) / Screen.width, (Screen.height - (pos.y + size.y / 2f)) / Screen.height);
            textTransform.anchorMin = position;
            textTransform.anchorMax = position;

            Object.DontDestroyOnLoad(textObj);
        }
        public void UpdateText(string text)
        {
            if (textObj != null)
            {
                textObj.GetComponent<Text>().text = text;
            }
        }
    }
}