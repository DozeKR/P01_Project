using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Card lastCard;
    public GameObject cardPrefab;
    public Transform deck;

    public int maxLevel;

    void Awake() {
        Application.targetFrameRate = 60;
    }

    void Start() {
        NextCard();
    }

    Card GetCard(){
        GameObject instant = Instantiate(cardPrefab, deck);
        Card instantCard = instant.GetComponent<Card>();
        return instantCard;
    }

    void NextCard(){
        Card newCard = GetCard();
        lastCard = newCard;
        lastCard.manager = this;
        lastCard.level = Random.Range(0,maxLevel);
        lastCard.gameObject.SetActive(true);
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
}
