using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : MonoBehaviour
{
    public GameObject customExplosionEffect;
    public PotionType potionType;

    public int xIndex;
    public int yIndex;

    public bool isMatched;
    private Vector2 currentPos;
    private Vector2 targetPos;

    public bool isMoving;

        // Tamaño original de la poción
    private Vector3 originalScale;

    private void Start()
    {
        // Guardamos la escala original al iniciar
        originalScale = transform.localScale;
    }

    public void Select()
    {
        // Aumentar la escala de la poción
        transform.localScale = originalScale * 1.1f; // Aumenta el tamaño al 110%
    }

    public void Deselect()
    {
        // Restablecer la escala a su tamaño original
        transform.localScale = originalScale;
    }

    public Potion(int _x, int _y)
    {
        xIndex = _x;
        yIndex = _y;
    }

    public void SetIndicies(int _x, int _y)
    {
        xIndex = _x;
        yIndex = _y;
    }

    //MoveToTarget
    public void MoveToTarget(Vector2 _targetPos)
    {
        StartCoroutine(MoveCoroutine(_targetPos));
    }
    //MoveCoroutine
    private IEnumerator MoveCoroutine(Vector2 _targetPos)
    {
        isMoving = true;
        float duration = 0.2f;

        Vector2 startPosition = transform.position;
        float elaspedTime = 0f;

        while (elaspedTime < duration)
        {
            float t = elaspedTime / duration;

            transform.position = Vector2.Lerp(startPosition, _targetPos, t);

            elaspedTime += Time.deltaTime;

            yield return null;
        }

        transform.position = _targetPos;
        isMoving = false;
    }
}

public enum PotionType
{
    Red,
    Blue,
    Purple,
    Green,
    White,
    Bomb,
    Lightning 
}
