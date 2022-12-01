using UnityEngine;

public class UIAnchorScaler : MonoBehaviour
{
#if UNITY_EDITOR
	[ExecuteInEditMode]
	public void ProcessUIAnchorScaler()
	{
		RectTransform rectTransform = GetComponent<RectTransform>();
		RectTransform parentRectTransform = transform.parent.GetComponent<RectTransform>();

		if (rectTransform && parentRectTransform)
		{
			Rect rect = rectTransform.rect;
			Vector3 position = transform.position;
			Rect parentRect = parentRectTransform.rect;

			rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
			rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.width);
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.height);
			rectTransform.position = position;

			float xMin = rectTransform.anchoredPosition.x + rect.xMin + (parentRect.width / 2);
			float yMin = rectTransform.anchoredPosition.y + rect.yMin + (parentRect.height / 2);
			float xMax = rectTransform.anchoredPosition.x + rect.xMax + (parentRect.width / 2);
			float yMax = rectTransform.anchoredPosition.y + rect.yMax + (parentRect.height / 2);

			float xMinAnchor = xMin / parentRect.width;
			float yMinAnchor = yMin / parentRect.height;
			float xMaxAnchor = xMax / parentRect.width;
			float yMaxAnchor = yMax / parentRect.height;

			rectTransform.anchorMin = new Vector2(xMinAnchor, yMinAnchor);
			rectTransform.anchorMax = new Vector2(xMaxAnchor, yMaxAnchor);

			rectTransform.offsetMin = new Vector2(0, 0);
			rectTransform.offsetMax = new Vector2(0, 0);

		}

		DestroyImmediate(this);
	}
#endif
}
