using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
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

    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
    public enum Sfx { Next, LevelUp, Button, Over, Warning, Attach}
    int sfxCursor;

    public int score;
    public int maxLevel;
    public bool isOver;

    void Awake() {
        Application.targetFrameRate = 60;

        cardPool = new List<Card>();
        effectPool = new List<ParticleSystem>();

        for(int i=0; i < poolSize; i++){
            MakeCard();
        }
    }

    void Start() {
        bgmPlayer.Play();
        NextCard();
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
        Card[] cards = GameObject.FindObjectsOfType<Card>();

        for(int i=0; i < cards.Length; i++){
            cards[i].rigid.simulated = false;
        }

        for(int i=0; i < cards.Length; i++){
            cards[i].Hide(Vector3.up * 100);
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(1f);

        SfxPlay(Sfx.Over);
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
}
