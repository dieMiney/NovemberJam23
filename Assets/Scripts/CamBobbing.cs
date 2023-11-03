using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamBobbing : MonoBehaviour
{
    public float bobbingSpeed = 0.05f;
    public float bobbingAmount = 0.35f;
    public float midpoint = 2.0f;

    private float timer = 0.0f;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponentInParent<CharacterController>();
    }

    void Update()
    {
        if(controller.isGrounded)
        {
            float waveslice = 0.0f;
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 cSharpPosition = transform.localPosition;

            if (Mathf.Abs(horizontal) == 0 && Mathf.Abs(vertical) == 0)
            {
                timer = 0.0f;
            }
            else
            {
                waveslice = Mathf.Sin(timer);
                timer = timer + bobbingSpeed;
                if (timer > Mathf.PI * 2)
                {
                    timer = timer - (Mathf.PI * 2);
                }
            }

            if (waveslice != 0)
            {
                float translateChange = waveslice * bobbingAmount;
                float totalAxes = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
                totalAxes = Mathf.Clamp(totalAxes, 0.0f, 1.0f);
                translateChange = totalAxes * translateChange;
                cSharpPosition.y = midpoint + translateChange;
            }
            else
            {
                cSharpPosition.y = midpoint;
            }

            transform.localPosition = cSharpPosition;
        }
    }
}
