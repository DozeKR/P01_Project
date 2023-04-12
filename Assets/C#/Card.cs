using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public GameManager manager;
    public ParticleSystem effect;

    public int level;
    public int type;
    public bool isDrag;
    public bool isMerge;
    public bool isWarning;
    public bool isAttach;

    public Rigidbody2D rigid;
    CircleCollider2D col;
    Animator anim;
    SpriteRenderer spriteRenderer;

    float deadTime;
    
    void Awake(){
        rigid = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable() {
        type = Random.Range(0,4);
        anim.SetInteger("Level", level);
        anim.SetInteger("Type", type);
    }

    void OnDisable() {
        //카드 속성 초기화
        level = 0;
        type = 0;
        isDrag = false;
        isMerge = false;
        isWarning = false;
        isAttach = false;
        //카드 트랜스폼 초기화
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;
        //카드 물리 초기화
        rigid.simulated = false;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;
        col.enabled = true;
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

    void OnCollisionEnter2D(Collision2D collision) {
        StartCoroutine(AttachRoutine());
    }

    IEnumerator AttachRoutine(){
        if(isAttach){
            yield break;
        }

        isAttach = true;
        manager.SfxPlay(GameManager.Sfx.Attach);

        yield return new WaitForSeconds(0.2f);

        isAttach = false;
    }

    void OnCollisionStay2D(Collision2D collision) {
        if(collision.gameObject.tag == "Card"){
            Card other = collision.gameObject.GetComponent<Card>();

            // 카드합치기조건
            if(level == other.level && !isMerge && !other.isMerge && level < 3 && type == other.type){
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

        if(targetPos == Vector3.up * 100){
            EffectPlay();
        }

        StartCoroutine(HideRoutine(targetPos));
    }

    IEnumerator HideRoutine(Vector3 targetPos){
        int frameCount =0;
        while(frameCount < 20){
            frameCount++;
            if(targetPos != Vector3.up * 100){
                transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
            }
            else if(targetPos == Vector3.up * 100){
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f);
            }

            yield return null;
        }

        manager.score += (int)Mathf.Pow(2, level);

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
        manager.SfxPlay(GameManager.Sfx.LevelUp);

        yield return new WaitForSeconds(0.3f);
        
        level++;

        manager.maxLevel = Mathf.Max(level, manager.maxLevel);
        
        isMerge = false;
    }

    void OnTriggerStay2D(Collider2D collision) {
        if(collision.tag == "Finish"){
            deadTime += Time.deltaTime;

            if(deadTime > 2){
                spriteRenderer.color = new Color(0.9f, 0.1f, 0.1f);
            }
            if(deadTime > 3 && !isWarning){
                manager.SfxPlay(GameManager.Sfx.Warning);
                isWarning = true;
            }
            if(deadTime > 5){
                manager.GameOver();
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        if(collision.tag == "Finish"){
            deadTime = 0;
            spriteRenderer.color = Color.white;
            isWarning = false;
        }
    }

    void EffectPlay(){
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale * 2;
        effect.Play();
    }
}
