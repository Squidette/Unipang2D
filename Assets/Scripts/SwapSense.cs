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
        if (Input.GetMouseButtonDown(0)) // 마우스 다운시 유니팡에 대응되는 좌표를 mouseDown 변수에 담는다
        {
            Vector3 mp = myCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseDown.row = -(int)Mathf.Round(mp.y);
            mouseDown.col = (int)Mathf.Round(mp.x);
        }
        if (unipangManager.AreCoordsValidArrayMember(mouseDown))
        {
            if (Input.GetMouseButtonUp(0)) // 마우스를 떼면 유효함을 판단한 후 mouseUp 좌표를 설정해줌
            {
                Vector3 mouseUpPosition = myCamera.ScreenToWorldPoint(Input.mousePosition);
                int rowToSwap = -(int)Mathf.Round(mouseUpPosition.y);
                int colToSwap = (int)Mathf.Round(mouseUpPosition.x);

                Coords mouseUp = new Coords(mouseDown);

                if (mouseDown.col == colToSwap)
                {
                    if (mouseDown.row > rowToSwap) // 상
                    {
                        mouseUp.row--;
                        unipangManager.ReceiveSwapInput(mouseDown, mouseUp);
                    }
                    else if (mouseDown.row < rowToSwap) // 하
                    {
                        mouseUp.row++;
                        unipangManager.ReceiveSwapInput(mouseDown, mouseUp);
                    }
                }
                else if (mouseDown.row == rowToSwap)
                {
                    if (mouseDown.col > colToSwap) // 좌
                    {
                        mouseUp.col--;
                        unipangManager.ReceiveSwapInput(mouseDown, mouseUp);
                    }
                    else if (mouseDown.col < colToSwap) // 우
                    {
                        mouseUp.col++;
                        unipangManager.ReceiveSwapInput(mouseDown, mouseUp);
                    }
                }
            }
        }
    }
}
