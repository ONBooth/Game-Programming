using UnityEngine;
using UnityEngine.SceneManagement;

// ATTACH THIS TO YOUR PLANE OBJECT
public class LevelSwitcher : MonoBehaviour
{
    [Header("Level Settings")]
    public bool useSceneIndex = false; 
    public string nextLevelName = "cylinder"; 
    public int nextLevelIndex = 1; 
    
    [Header("Visual Feedback")]
    public Color triggerColor = Color.green;
    public bool showPrompt = true;
    
    private bool playerOnTrigger = false;
    private Renderer planeRenderer;
    private Color originalColor;
    
    void Start()
    {
        // Get the plane's renderer to change color//
        planeRenderer = GetComponent<Renderer>();
        if (planeRenderer != null)
        {
            originalColor = planeRenderer.material.color;
        }
        
        // Make sure this has a collider set as trigger//
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnTrigger = true;
            
           
            if (planeRenderer != null)
            {
                planeRenderer.material.color = triggerColor;
            }
            
            Debug.Log("Player on level switch trigger!");
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && playerOnTrigger)
        {
            // Automatically switch after standing for 1 second
            Invoke("SwitchLevel", 1f);
            playerOnTrigger = false; // Prevent multiple calls
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnTrigger = false;
            
            // Reset color
            if (planeRenderer != null)
            {
                planeRenderer.material.color = originalColor;
            }
            
            // Cancel level switch if player leaves
            CancelInvoke("SwitchLevel");
        }
    }
    
    void SwitchLevel()
    {
        Debug.Log("Switching to next level!");
        
        if (useSceneIndex)
        {
            // Load by index number
            SceneManager.LoadScene(nextLevelIndex);
        }
        else
        {
            // Load by scene name
            SceneManager.LoadScene(nextLevelName);
        }
    }
    
    void OnGUI()
    {
        // Show prompt when player is on trigger
        if (showPrompt && playerOnTrigger)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
            
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 100, 200, 30), 
                     "Loading next level...", 
                     style);
        }
    }
}

//this was stress full to do :) i am happy now 
//i hope it works fine
//i did my best to not make any mistakes
//i hope you like it
//*** Happy new year ***\\