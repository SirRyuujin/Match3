using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class GemSwapAnimator : MonoBehaviour
{
    [Header("References")]
    public GameEventSO OnGemsSwappedEvent;
    public GameEventSO OnFailedSwapEvent;
    [SerializeField] private FloatVariable SwapAnimationSpeed;
    [SerializeField] private FloatVariable SelectedGemScale;
    [SerializeField] private GameObject _tile1;
    [SerializeField] private GameObject _tile2;
    [SerializeField] private Image _tile1Img;
    [SerializeField] private Image _tile2Img;

    public void AnimateFailedSwap()
    {
        if (OnFailedSwapEvent.RecentCaller == null)
            return;

        BoardController board = OnGemsSwappedEvent.RecentCaller.GetComponent<BoardController>();
        Image img1 = board.SelectedTiles[1].GetComponent<Image>();
        Image img2 = board.SelectedTiles[0].GetComponent<Image>();

        _tile1Img.sprite = img1.sprite;
        _tile2Img.sprite = img2.sprite;

        Vector3 g1LPos = board.SelectedTiles[1].GetLocalPosition();
        Vector3 g2LPos = board.SelectedTiles[0].GetLocalPosition();
        _tile1.transform.localPosition = g1LPos;
        _tile2.transform.localPosition = g2LPos;

        _tile1.transform.localScale = new Vector3(SelectedGemScale.Value, SelectedGemScale.Value, 1);
        _tile2.transform.localScale = new Vector3(SelectedGemScale.Value, SelectedGemScale.Value, 1);

        _tile1Img.enabled = true;
        _tile2Img.enabled = true;

        TileShakeSequence(_tile1);
        TileShakeSequence(_tile2);

        BlinkRedSequence(_tile1Img);
        BlinkRedSequence(_tile2Img);
    }

    private void TileShakeSequence(GameObject tile)
    {
        Sequence tileShake = DOTween.Sequence();
        tileShake.Append(tile.transform.DORotate(new Vector3(0, 0, -45), 0.2f));
        tileShake.Append(tile.transform.DORotate(new Vector3(0, 0, 45), 0.2f));
        tileShake.Append(tile.transform.DORotate(new Vector3(0, 0, 0), 0.1f));

        tileShake.Play();
    }

    private void BlinkRedSequence(Image tileImg)
    {
        Sequence tileBlink = DOTween.Sequence();
        tileBlink.Append(tileImg.DOColor(new Color(1, 0, 0), 0));
        tileBlink.AppendInterval(.4f);
        tileBlink.Append(tileImg.DOColor(new Color(1, 1, 1), .2f));
        tileBlink.OnComplete(() => ToogleImage(tileImg));

        tileBlink.Play();
    }

    public void AnimateSwap()
    {
        if (OnGemsSwappedEvent.RecentCaller == null)
            return;

        BoardController board = OnGemsSwappedEvent.RecentCaller.GetComponent<BoardController>();
        Image img1 = board.SelectedTiles[1].GetComponent<Image>();
        Image img2 = board.SelectedTiles[0].GetComponent<Image>();

        _tile1Img.sprite = img1.sprite;
        _tile2Img.sprite = img2.sprite;

        Vector3 g1LPos = board.SelectedTiles[0].GetLocalPosition();
        Vector3 g2LPos = board.SelectedTiles[1].GetLocalPosition();

        _tile1.transform.localPosition = g1LPos;
        _tile2.transform.localPosition = g2LPos;

        _tile1.transform.localScale = new Vector3(SelectedGemScale.Value, SelectedGemScale.Value, 1);
        _tile2.transform.localScale = new Vector3(SelectedGemScale.Value, SelectedGemScale.Value, 1);

        _tile1Img.enabled = true;
        _tile2Img.enabled = true;


        img1.enabled = false;
        img2.enabled = false;


        _tile1.transform.DOLocalMove(g2LPos, SwapAnimationSpeed.Value).OnComplete(() => ToogleImage(_tile1Img));
        _tile2.transform.DOLocalMove(g1LPos, SwapAnimationSpeed.Value).OnComplete(() => EndSwapSequence(_tile2Img, img1, img2));
    }

    private void EndSwapSequence(Image fakeImage, Image img1, Image img2)
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