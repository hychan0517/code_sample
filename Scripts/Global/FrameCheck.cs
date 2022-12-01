using UnityEngine;
using UnityEngine.UI;

public class FrameCheck : MonoBehaviour
{
#if DEBUG_MODE
    private Text _textFPS;
    private float _fFrameSumDeltaTime = 0.0f;
    private int _iFrameDataCount = 0;
    private float _fFrameLow = 1000.0f;
    private float _fFrameBest = 0;

    private void Awake()
    {
        _textFPS = GetComponentInChildren<Text>();
    }

    void Update()
    {
        DisplayDebuggingFrameRate();

        if (Input.GetKeyDown(KeyCode.F) || Input.touchCount > 3)
        {
            _fFrameLow = 1000;
            _fFrameBest = 0;
        }
    }
    private void DisplayDebuggingFrameRate()
    {
        if (Time.deltaTime > 0.0f)
        {
            float fFrameRate = 1.0f / Time.deltaTime;

            if (fFrameRate < _fFrameLow)
            {
                _fFrameLow = fFrameRate;
            }
            if (fFrameRate > _fFrameBest)
                _fFrameBest = fFrameRate;


            _fFrameSumDeltaTime += Time.deltaTime;
            ++_iFrameDataCount;
            float fFrameSumRate = 1.0f / (_fFrameSumDeltaTime / _iFrameDataCount);

            string log = string.Format("FPS : {0,3}\nLow : {1,3}\nBest : {2,3}\nAvg : {3,3}", (int)fFrameRate, (int)_fFrameLow, (int)_fFrameBest, (int)fFrameSumRate);
            if (_textFPS)
                _textFPS.text = log;
        }
        else
        {
            if (_textFPS)
                _textFPS.text = "FPS:0.0,  Low:0.0,  Avg:0.0";
        }
    }
#else
    private void Awake()
	{
        Destroy(gameObject);
	}
#endif
}
