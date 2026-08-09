using UnityEngine;

using System.Collections.Generic;   // Contains dict
using TMPro;

public class GameManager : MonoBehaviour
{
    private int MaxGoals = 3;

    // UI panels
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject gameOverPanel;
    // UI score
    [SerializeField] private TMP_Text leftScore;
    [SerializeField] private TMP_Text rightScore;
    [SerializeField] private TMP_Text winnerText;
    // Menu UI selections
    [SerializeField] private MenuUI menuUI;

    private AbstractAgent leftAgent;
    private AbstractAgent rightAgent;
    private Dictionary<TeamSide, int> currentScores;

    [SerializeField] private BoardManager boardManager;
    private GameState gameState;

    void Start()
    {
        gameState = GameState.Menu;

        SetPanel();
    }

    // Run the current game state.
    void Update()
    {
        switch(gameState)
        {
            case GameState.Menu:
                HandleMenuInput();
                break;

            case GameState.Playing:
                HandleGameInput();
                break;

            case GameState.GameOver:
                HandleGameOverInput();
                break;
        }        
    }

    // Game state methods
    private void HandleMenuInput(){
        // Exit game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        // Start game
        if (Input.GetKeyDown(KeyCode.Space)){
            AgentOption leftAgentOption = menuUI.GetLeftAgent();
            AgentOption rightAgentOption = menuUI.GetRightAgent();
            leftAgent = Instantiate(leftAgentOption.prefab);
            rightAgent = Instantiate(rightAgentOption.prefab);

            gameState = GameState.Playing;
            SetPanel();
            StartMatch();
        }        
    }

    private void HandleGameInput(){
        // Restart
        if (Input.GetKeyDown(KeyCode.Space)){
            currentScores = new Dictionary<TeamSide, int>{
                {TeamSide.Left, 0},
                {TeamSide.Right, 0}
            }; 
            rightScore.text = $"{currentScores[TeamSide.Right]}";
            leftScore.text = $"{currentScores[TeamSide.Left]}";
            boardManager.ResetBoard();

            gameState = GameState.Playing;
            SetPanel();            
        }             

        // Quit
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameState = GameState.Menu;
            SetPanel();  
            DestroyGame();
        }
    }

    private void HandleGameOverInput(){
        // Restart
        if (Input.GetKeyDown(KeyCode.Space)){
            currentScores = new Dictionary<TeamSide, int>{
                {TeamSide.Left, 0},
                {TeamSide.Right, 0}
            }; 
            rightScore.text = $"{currentScores[TeamSide.Right]}";
            leftScore.text = $"{currentScores[TeamSide.Left]}";
            boardManager.ResetBoard();

            gameState = GameState.Playing;
            SetPanel();            
        }

        // Quit
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameState = GameState.Menu;
            SetPanel();  
            DestroyGame();
        }
    }


    // Update the score and end the match if needed.
    public void TeamScored(TeamSide teamSide){
        currentScores[teamSide] ++;
        if (teamSide == TeamSide.Left){
            rightScore.text = $"{currentScores[teamSide]}";
        }
        else{
            leftScore.text = $"{currentScores[teamSide]}";
        }
        if (currentScores[teamSide] == MaxGoals && gameState != GameState.GameOver){            
            winnerText.text = (teamSide == TeamSide.Right ? "Left " : "Right" )  + "Wins!";

            gameState = GameState.GameOver;
            SetPanel();           
        }
    }

    // Create a new match.
    private void StartMatch(){
        currentScores = new Dictionary<TeamSide, int>{
            {TeamSide.Left, 0},
            {TeamSide.Right, 0}
        }; 
        boardManager.Startmatch(leftAgent, rightAgent);
    }

    // Clean up the current match.
    private void DestroyGame(){
        currentScores = new Dictionary<TeamSide, int>{
            {TeamSide.Left, 0},
            {TeamSide.Right, 0}
        };         
        rightScore.text = $"{currentScores[TeamSide.Right]}";
        leftScore.text = $"{currentScores[TeamSide.Left]}";
        boardManager.EndMatch();
        leftAgent.enabled = false;
        rightAgent.enabled = false;
        Destroy(leftAgent.gameObject);
        Destroy(rightAgent.gameObject);
    }

    // Show the UI for the current game state.
    private void SetPanel(){
        optionsPanel.SetActive(gameState == GameState.Menu);
        hudPanel.SetActive(gameState == GameState.Playing);
        gameOverPanel.SetActive(gameState == GameState.GameOver);     
    }
}