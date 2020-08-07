using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAndInputDemo : MonoBehaviour
{
    public CharacterController character;
    public Camera controlleringCamera;
    public float moveSpeed = 5f;
    // Start is called before the first frame update
    void Start()
    {
        InputManager.useMobileInputOnNonMobile = true;
    }

    // Update is called once per frame
    void Update()
    {
        var inputV = InputManager.GetAxis("Vertical", false);
        var inputH = InputManager.GetAxis("Horizontal", false);
        var moveDirection = Vector3.zero;
        moveDirection += controlleringCamera.transform.forward * inputV;
        moveDirection += controlleringCamera.transform.right * inputH;
        character.SimpleMove(moveDirection * moveSpeed * 1000f * Time.deltaTime);
    }
}
