using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Card lastCard;
    public GameObject cardPrefab;
    public Transform deck;

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
