using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IndividualElement : MonoBehaviour
{
    public Vector3 gameCoord; // 게임상 위치해야할 좌표
    private float speed = 5.0F;

    void Start()
    {
        
    }

    void Update()
    {
        if (gameCoord != null)
        {
            float distance = Vector3.Distance(transform.position, gameCoord);

            if (distance > 0.01F)
            {
                transform.position = Vector3.MoveTowards(transform.position, gameCoord, speed * Time.deltaTime);
            }
            else
            {
                transform.position = gameCoord;
            }
        }
    }
}
