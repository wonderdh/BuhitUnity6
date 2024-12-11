using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindAnim : MonoBehaviour
{

    [SerializeField]
    Sprite[] sprites;

    SpriteRenderer spriteRenderer;

    public void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = null;
        Debug.Log("onFind");
        OnFind();
    }

    public void OnFind()
    {
        StartCoroutine(findAnim());
    }

    IEnumerator findAnim()
    {
        for(int i = 0; i < sprites.Length; i++)
        {
            spriteRenderer.sprite = sprites[i];
            yield return new WaitForSeconds(0.15f);
        }

        spriteRenderer.sprite = null;
        Destroy(transform.gameObject);
    }
}
