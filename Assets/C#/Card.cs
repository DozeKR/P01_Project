using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public GameManager manager;
    public ParticleSystem effect;

    public int level;
    public bool isDrag;
    public bool isMerge;

    Rigidbody2D rigid;
    CircleCollider2D col;
    Animator anim;

    void Awake(){
        rigid = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        anim = GetComponent<Animator>();
    }

    void OnEnable() {
        anim.SetInteger("Level", level);
    }

    void Update(){
        if(isDrag){
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            float leftBorder = -3.7f + transform.localScale.x / 2f;
            float rightBorder = 3.7f - transform.localScale.x / 2f;

            if(mousePos.x < leftBorder){
                mousePos.x = leftBorder;
            }
            else if(mousePos.x > rightBorder){
                mousePos.x = rightBorder;
            }

            mousePos.y = 8;
            mousePos.z = 0;
            transform.position = Vector3.Lerp(transform.position, mousePos, 0.2f);
        }
      
    }

    public void Drag(){
        isDrag = true;
    }

    public void Drop(){
        isDrag = false;
        rigid.simulated = true;
    }

    void OnCollisionStay2D(Collision2D collision) {
        if(collision.gameObject.tag == "Card"){
            Card other = collision.gameObject.GetComponent<Card>();

            // 카드합치기조건
            if(level == other.level && !isMerge && !other.isMerge && level < 3){
                float meX = transform.position.x;
                float meY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;

                // 1. 내가 아래
                // 2. 동일한 높이, 내가 오른쪽에 있을 떄
                if(meY < otherY || (meY == otherY && meX > otherX)){
                    // 상대방 숨기기
                    other.Hide(transform.position);
                    // 레벨업
                    LevelUp();

                }
            }
        }
    }

    public void Hide(Vector3 targetPos){
        isMerge = true;

        rigid.simulated = false;
        col.enabled = false;

        StartCoroutine(HideRoutine(targetPos));
    }

    IEnumerator HideRoutine(Vector3 targetPos){
        int frameCount =0;
        while(frameCount < 20){
            frameCount++;
            transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
             yield return null;
        }

        isMerge = false;
        gameObject.SetActive(false);
    }

    void LevelUp(){
        isMerge = true;

        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;

        StartCoroutine(LevelUpRoutine());
    }

    IEnumerator LevelUpRoutine(){
        yield return new WaitForSeconds(0.2f);

        anim.SetInteger("Level", level + 1);
        EffectPlay();

        yield return new WaitForSeconds(0.3f);
        
        level++;

        manager.maxLevel = Mathf.Max(level, manager.maxLevel);
        
        isMerge = false;
    }

    void EffectPlay(){
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale * 2;
        effect.Play();
    }
}
