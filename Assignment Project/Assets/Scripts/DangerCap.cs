using UnityEngine;
using UnityEngine.UIElements;


public class DangerCap : MonoBehaviour

{
   [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float moveDistance = 5f;
    
    private Vector3 startPos;
    private Vector3 leftBound;
    private Vector3 rightBound;
    
    void Start()
    {
        startPos = transform.position;
        leftBound = startPos - Vector3.right * moveDistance;
        rightBound = startPos + Vector3.right * moveDistance;
    }
    
    void Update()
    {
        // Move left and right
        transform.position += Vector3.right * moveSpeed * Time.deltaTime;
        
        // Check if reached right bound
        if (transform.position.x >= rightBound.x)
        {
            moveSpeed = -Mathf.Abs(moveSpeed); // Move left
        }
        // Check if reached left bound
        else if (transform.position.x <= leftBound.x)
        {
            moveSpeed = Mathf.Abs(moveSpeed); // Move right
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Check if collided with player
        if (collision.gameObject.CompareTag("Player"))
        {
            QuitGame();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Alternative if using trigger collider
        if (other.CompareTag("Player"))
        {
            QuitGame();
        }
    }
    
    void QuitGame()
    {
        Debug.Log("Player touched quit capsule! Quitting game...");
        
        #if UNITY_EDITOR
            // Stop playing in Unity Editor
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // Quit the application when built
            Application.Quit();
        #endif
    }
}