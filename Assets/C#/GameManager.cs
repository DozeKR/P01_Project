using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("[Core]")]
    public int score;
    public int maxLevel;
    public bool isOver;

    [Header("[Object Pooling]")]
    public GameObject cardPrefab;
    public Transform deck;
    public List<Card> cardPool;

    public GameObject effectPrefab;
    public Transform  effectGroup;
    public List<ParticleSystem> effectPool;

    [Range(1, 30)]
    public int poolSize;
    public int poolCursor;
    public Card lastCard;

    [Header("[Audio]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
    public enum Sfx { Next, LevelUp, Button, Over, Warning, Attach}
    int sfxCursor;

    [Header("[UI]")]
    public GameObject startGroup;
    public GameObject endGroup;
    public Text scoreText;
    public Text maxScoreText;
    public Text subScoreText;

    [Header("[ETC]")]
    public GameObject cutline;
    public GameObject bottom;


    void Awake() {
        Application.targetFrameRate = 60;

        cardPool = new List<Card>();
        effectPool = new List<ParticleSystem>();

        for(int i=0; i < poolSize; i++){
            MakeCard();
        }

        if(!PlayerPrefs.HasKey("MaxScore")){
            PlayerPrefs.SetInt("MaxScore", 0);
        }
        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();
    }

    public void GameStart() {
        //오브젝트 활성화
        cutline.SetActive(true);
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);

        //오디오
        bgmPlayer.Play();
        SfxPlay(Sfx.Button);
        
        //게임 시작(카드생성)
        Invoke("NextCard", 1.5f);
    }

    Card MakeCard(){
        //이팩트 생성
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect " + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        //카드 생성
        GameObject instantCardObj = Instantiate(cardPrefab, deck);
        instantCardObj.name = "Card " + cardPool.Count;
        Card instantCard = instantCardObj.GetComponent<Card>();
        instantCard.manager = this;
        instantCard.effect = instantEffect;
        cardPool.Add(instantCard);

        return instantCard;
    }

    Card GetCard(){
        for(int i=0; i < cardPool.Count; i++){
            poolCursor = (poolCursor + 1) % cardPool.Count;
            if(!cardPool[poolCursor].gameObject.activeSelf){
                return cardPool[poolCursor];
            }
        }
        
        return MakeCard();
    }

    void NextCard(){
        if(isOver){
            return;
        }

        Card newCard = GetCard();
        lastCard = newCard;
        lastCard.level = Random.Range(0,maxLevel);
        lastCard.gameObject.SetActive(true);

        SfxPlay(Sfx.Next);
        StartCoroutine(WaitNext());
    }

    IEnumerator WaitNext(){
        while(lastCard != null){
            yield return null;
        }

        yield return new WaitForSeconds(2.0f);

        NextCard();
    }

    public void TouchDown(){
        if(lastCard == null)
            return;
        lastCard.Drag();
    }

    public void TouchUp(){
        if(lastCard == null)
            return;
        lastCard.Drop();
        lastCard = null;
    }

    public void GameOver(){
        if(isOver){
            return;
        }

        isOver = true;
        
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine(){
        //모든 카드 가져오기
        Card[] cards = GameObject.FindObjectsOfType<Card>();

        //모든 카드 물리효과 비활성화
        for(int i=0; i < cards.Length; i++){
            cards[i].rigid.simulated = false;
        }

        //모든 카드를 하나씩 접근해서 지우기
        for(int i=0; i < cards.Length; i++){
            cards[i].Hide(Vector3.up * 100);
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(1f);

        //최고점수 갱신
        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);
        
        //게임오버 UI
        subScoreText.text = "SCORE : " + scoreText.text;
        endGroup.SetActive(true);

        bgmPlayer.Stop();
        SfxPlay(Sfx.Over);
    }

    public void Reset(){
        SfxPlay(Sfx.Button);
        StartCoroutine(ResetRoutine());
    }

    IEnumerator ResetRoutine(){
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Main");
    }

    public void SfxPlay(Sfx type){
        switch(type){
            case Sfx.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[0];
                break;
            case Sfx.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[1];
                break;
            case Sfx.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[2];
                break;
            case Sfx.Over:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;
            case Sfx.Warning:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(4,6)];
                break;
            case Sfx.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
        }

        sfxPlayer[sfxCursor].Play();
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
    }

    void Update(){
        if(Input.GetButtonDown("Cancel")){
            Application.Quit();
        }
    }

    void LateUpdate() {
        scoreText.text = score.ToString();
    }
}
