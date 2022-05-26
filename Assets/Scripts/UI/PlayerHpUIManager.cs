using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHpUIManager : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private RawImage hpBarBackground;
    [SerializeField] private RawImage hpStatusBar;

    private float totalHP;
    private float hpBarRatio;

    private float hpBarWidth;
    private float hpBarHeight;

    private void Start()
    {
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        RectTransform rectTransform = hpBarBackground.GetComponent<RectTransform>();

        hpBarWidth = rectTransform.sizeDelta.x;
        hpBarHeight = rectTransform.sizeDelta.y;

        IGamePiece gamePiece = GetComponent<IGamePiece>();
        hpBarRatio = 1.0f / (float)gamePiece.GetMaxHp();
    }


    /// <summary>
    /// Change the width of the UI element.
    /// </summary>
    /// <param name="currentHp"></param>
    public void ChangeStatusBarWidth(int currentHp)
    {
        float width = currentHp * hpBarRatio * hpBarWidth;

        hpStatusBar.rectTransform.sizeDelta = new Vector2(width, hpBarHeight);
    }


    /// <summary>
    /// Reset the width of the HP-bar
    /// </summary>
    public void ResetStatusBarWidth()
    {
        float width = totalHP * hpBarRatio * hpBarWidth;

        hpStatusBar.rectTransform.sizeDelta = new Vector2(width, hpBarHeight);
    }

}
