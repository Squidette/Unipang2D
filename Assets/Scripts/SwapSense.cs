using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SwapSense : MonoBehaviour
{
    public Camera myCamera;
    public UnipangManager unipangManager;
    
    Coords mouseDown;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // ���콺 �ٿ�� �����ο� �����Ǵ� ��ǥ�� mouseDown ������ ��´�
        {
            Vector3 mp = myCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseDown.row = -(int)Mathf.Round(mp.y);
            mouseDown.col = (int)Mathf.Round(mp.x);
        }
        if (unipangManager.AreCoordsValidArrayMember(mouseDown))
        {
            if (Input.GetMouseButtonUp(0)) // ���콺�� ���� ��ȿ���� �Ǵ��� �� mouseUp ��ǥ�� ��������
            {
                Vector3 mouseUpPosition = myCamera.ScreenToWorldPoint(Input.mousePosition);
                int rowToSwap = -(int)Mathf.Round(mouseUpPosition.y);
                int colToSwap = (int)Mathf.Round(mouseUpPosition.x);

                Coords mouseUp = new Coords(mouseDown);

                if (mouseDown.col == colToSwap)
                {
                    if (mouseDown.row > rowToSwap) // ��
                    {
                        mouseUp.row--;
                        unipangManager.ReceiveSwapInput(mouseDown, mouseUp);
                    }
                    else if (mouseDown.row < rowToSwap) // ��
                    {
                        mouseUp.row++;
                        unipangManager.ReceiveSwapInput(mouseDown, mouseUp);
                    }
                }
                else if (mouseDown.row == rowToSwap)
                {
                    if (mouseDown.col > colToSwap) // ��
                    {
                        mouseUp.col--;
                        unipangManager.ReceiveSwapInput(mouseDown, mouseUp);
                    }
                    else if (mouseDown.col < colToSwap) // ��
                    {
                        mouseUp.col++;
                        unipangManager.ReceiveSwapInput(mouseDown, mouseUp);
                    }
                }
            }
        }
    }
}
