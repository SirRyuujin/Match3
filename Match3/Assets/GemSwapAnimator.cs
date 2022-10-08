using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class GemSwapAnimator : MonoBehaviour
{
    [Header("References")]
    public GameEventSO OnGemsSwappedEvent;
    [SerializeField] private FloatVariable SwapAnimationSpeed;
    [SerializeField] private FloatVariable SelectedGemScale;
    [SerializeField] private GameObject _gem1;
    [SerializeField] private GameObject _gem2;
    [SerializeField] private Image _gem1Image;
    [SerializeField] private Image _gem2Image;

    public void AnimateSwap()
    {
        if (OnGemsSwappedEvent.RecentCaller == null)
            return;

        BoardController board = OnGemsSwappedEvent.RecentCaller.GetComponent<BoardController>();
    //    gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(board.Width, board.Height);
        Image img1 = board.SelectedTiles[0].GetComponent<Image>();
        Image img2 = board.SelectedTiles[1].GetComponent<Image>();

        _gem1Image.sprite = board.SelectedTiles[1].Gem.Sprite;
        _gem2Image.sprite = board.SelectedTiles[0].Gem.Sprite;

        Vector3 g1LPos = board.SelectedTiles[0].GetLocalPosition();
        Vector3 g2LPos = board.SelectedTiles[1].GetLocalPosition();

        _gem1.transform.localPosition = g2LPos;
        _gem2.transform.localPosition = g1LPos;

        _gem1.transform.localScale = new Vector3(SelectedGemScale.Value, SelectedGemScale.Value, 1);
        _gem2.transform.localScale = new Vector3(SelectedGemScale.Value, SelectedGemScale.Value, 1);

        _gem1Image.enabled = true;
        _gem2Image.enabled = true;


        img1.enabled = false;
        img2.enabled = false;


        _gem1.transform.DOLocalMove(g1LPos, SwapAnimationSpeed.Value).OnComplete(() => ToogleImage(_gem1Image));
        _gem2.transform.DOLocalMove(g2LPos, SwapAnimationSpeed.Value).OnComplete(() => EndSequence(_gem2Image, img1, img2, board));
    }

    private void EndSequence(Image fakeImage, Image img1, Image img2, BoardController board)
    {
        ToogleImage(fakeImage);
        img1.enabled = true;
        img2.enabled = true;
    }

    private void ToogleImage(Image image)
    {
        image.enabled = !image.enabled;
    }
}