﻿using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.utils;
using UnityEngine;

public class PredictionManager : MonoBehaviour
{
    public LineRenderer LineRenderer1;
    public LineRenderer LineRenderer2;
    public LineRenderer LineRenderer3;
    private Vector3 _from;
    private Vector3[] _toos;
    private bool _isShowing;
    private bool _startShowing;
    private IEnumerator _displayPrediction;

    [Header("Colors")]
    public Color RedPrediction;
    public Color BluePrediction;
    public Color YellowPrediction;
    public Color GreenPrediction;
    public Color BrownPrediction;

    // Start is called before the first frame update
    void Start()
    {
        Show(false);
    }

    public void ChangeColor(BlobColor blobColor)
    {
        Color color;
        switch (blobColor)
        {
            case BlobColor.BLUE:
                color = BluePrediction;
                break;
            case BlobColor.YELLOW:
                color = YellowPrediction;
                break;
            case BlobColor.GREEN:
                color = GreenPrediction;
                break;
            case BlobColor.BROWN:
                color = BrownPrediction;
                break;
            case BlobColor.RED:
            default:
                color = RedPrediction;
                break;
        }
        LineRenderer1.startColor = color;
        LineRenderer1.endColor = color;
        LineRenderer2.startColor = color;
        LineRenderer2.endColor = color;
        LineRenderer3.startColor = color;
        LineRenderer3.endColor = color;
    }

    public void Reset()
    {
        _isShowing = false;
        _startShowing = false;
        Show(false);
    }

    public void Show(bool val)
    {
        LineRenderer1.gameObject.SetActive(val);
        LineRenderer2.gameObject.SetActive(val);
        LineRenderer3.gameObject.SetActive(val);
    }

    public void ShowPrediction(Vector3 from, Vector3[] toos)
    {
        _from = from;
        _toos = toos;
        if (_isShowing)
        {
            DisplayPrediction();
        }
        if (_startShowing)
        {
            return;
        }

        _isShowing = false;
        _startShowing = true;
        if (_displayPrediction != null)
        {
            StopCoroutine(_displayPrediction);
        }
        _displayPrediction = WaitDisplayPrediction();
        StartCoroutine(_displayPrediction);
    }

    IEnumerator WaitDisplayPrediction()
    {
        yield return new WaitForSeconds(0.33f);

        if (Game._.Player.IsDragging)
        {
            _isShowing = true;
            DisplayPrediction();
        }
    }

    private void DisplayPrediction()
    {
        Vector3[] linePredictions = BlobService.GetLinePrediction(_from, _toos);
        LineRenderer1.gameObject.SetActive(true);
        LineRenderer1.positionCount = 2;
        LineRenderer1.SetPositions(new Vector3[2] { linePredictions[0], linePredictions[1] });
        if (linePredictions.Length > 2)
        {
            LineRenderer2.gameObject.SetActive(true);
            LineRenderer2.positionCount = 2;
            LineRenderer2.SetPositions(new Vector3[2] { linePredictions[1], linePredictions[2] });
            if (linePredictions.Length > 3)
            {
                LineRenderer3.gameObject.SetActive(true);
                LineRenderer3.positionCount = 2;
                LineRenderer3.SetPositions(new Vector3[2] { linePredictions[2], linePredictions[3] });
            }
            else
            {
                LineRenderer3.gameObject.SetActive(false);
            }
        }
        else
        {
            LineRenderer2.gameObject.SetActive(false);
        }
    }
}
