using DG.Tweening;
using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Text))]
public class GameText : MonoBehaviour
{
    Text _text;
    [SerializeField] string[] _texts;
    [SerializeField] TextType _type = TextType.None;
    [SerializeField] Text _newRecord;
    [SerializeField] Transform _newRecordAnchor;
    ReactiveProperty<int> _time = new IntReactiveProperty();
    int _indexNumber = 0;
    void OnEnable()
    {
        GameManager.Instance.NewRecord += NewRecord;
    }
    void OnDisable()
    {
        GameManager.Instance.NewRecord -= NewRecord;
    }
    void Start()
    {
        _text = GetComponent<Text>();
        if(_type == TextType.MoveText) 
            StartCoroutine(MoveText());
        if( _type == TextType.Time)
        {
            _time.Value = (int)GameManager.Instance._stageTime;
            _time.Where(time => time > 7).Subscribe(_ => _text.color = Color.white);
            _time.Where(time => time > 5 && time <= 7).Subscribe(_ => _text.DOColor(new Color(1, 1, 0), 0.9f));
            _time.Where(time => time <= 5)
            .Subscribe(_ =>
            {
                _text.rectTransform.localScale = Vector3.zero;
                _text.rectTransform.DOScale(1, 0.9f).SetEase(Ease.OutElastic);
                _text.DOColor(new Color(1, 0, 0), 0.9f);
            });
        }
    }

    void Update()
    {
        if(_type == TextType.Time)
        {
            _time.Value = (int)GameManager.Instance._stageTime;
            _text.text = TimeDisplay(GameManager.Instance._stageTime);
        }
        if (_type == TextType.Steps) 
            _text.text = GameManager.Instance._steps.ToString("0000");
        if(_type == TextType.ClearSteps)    //  ClearPanelクラスに移植済み
            _text.text = "クリア歩数：" + GameManager.Instance._steps.ToString();
        if (_type == TextType.ClearTime)    //  ClearPanelクラスに移植済み
        {
            var gm = GameManager.Instance;
            _text.text = "クリア時間：" + (gm.MapEditor._stageData.timeLimit - gm._stageTime).ToString("0.00");
        }
        if (_type == TextType.timeAchievement)
        {
            _text.text = $"[1]{GameManager.Instance.MapEditor._stageData.timeAchievement}秒以内にクリア";
            if (GameManager.Instance._isAchieved[0]())
                _text.color = Color.white;
            else _text.color = Color.red;
        }
        if (_type == TextType.stepAchievement1)
        {
            _text.text = $"[2]{GameManager.Instance.MapEditor._stageData.stepAchievement1}歩以内にクリア";
            if (GameManager.Instance._isAchieved[0]() && GameManager.Instance._isAchieved[1]())
                _text.color = Color.white;
            else _text.color = Color.red;
        }
        if (_type == TextType.stepAchievement2)
        {
            _text.text = $"[3]{GameManager.Instance.MapEditor._stageData.stepAchievement2}歩以内にクリア";
            if (GameManager.Instance._isAchieved[0]() && GameManager.Instance._isAchieved[1]() && GameManager.Instance._isAchieved[2]())
                _text.color = Color.white;
            else _text.color = Color.red;
        }
    }
    IEnumerator MoveText()
    {
        while(true)
        {
            _text.text = _texts[_indexNumber];
            ++_indexNumber;
            _indexNumber %= _texts.Length;
            yield return new WaitForSeconds(5f);
        }
    }
    string TimeDisplay(float seconds)
    {
        int minutes = 0;
        minutes += (int)seconds / 60;
        seconds %= 60;
        minutes %= 60;
        return $"{minutes.ToString("00")}:{seconds.ToString("00.00")}";
    }
    void NewRecord(TextType type)
    {
        if (_type != type) return;
        if (_newRecord == null) return;
        if (_newRecordAnchor == null) return;
        Text newRecord = Instantiate(_newRecord, _newRecordAnchor);
        newRecord.rectTransform.DOLocalMoveY(20f, 0.4f)
                               .SetRelative(true).SetEase(Ease.OutQuad)
                               .SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
        newRecord.DOColor(Color.white, 1f).SetEase(Ease.Flash, 8).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
    }
}