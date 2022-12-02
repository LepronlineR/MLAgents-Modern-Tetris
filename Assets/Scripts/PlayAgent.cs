using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PlayAgent : Agent {
    
    [SerializeField] Game game;
    [SerializeField] GameController controller;

    [SerializeField] Text textScore;
    [SerializeField] Text textMaxLines;
    [SerializeField] Text textCurLines;
    [SerializeField] Text textTspin;
    [SerializeField] Text textPC;
    [SerializeField] Text textB2B;
    [SerializeField] Text textMaxCombo;

    private float totalScore;

    // all score handlers
    [HideInInspector] public int maxLines = 0;
    [HideInInspector] public int curLines = 0;
    [HideInInspector] public int sTspin = 0;
    [HideInInspector] public int dTspin = 0;
    [HideInInspector] public int tTspin = 0;
    [HideInInspector] public int b2b = 0;
    [HideInInspector] public int sPc = 0;
    [HideInInspector] public int dPc = 0;
    [HideInInspector] public int tPc = 0;
    [HideInInspector] public int qPc = 0;
    [HideInInspector] public int maxCombo = 0;

    void Start() {
        game.InitGame();
        game.InitRandomizer();
    }

    void SoftResetScores() {
        curLines = 0;
        totalScore = 0;
    }

    public override void OnEpisodeBegin() {
        SoftResetScores();
        game.AgentInit();
    }
    
    // Observations

    // all the tiles (occupied or not occupied)
    
    // current piece
    
    // (NOT ADDED YET) next pieces

    // (NOT ADDED YET) held piece    
    public override void CollectObservations(VectorSensor sensor){
        sensor.AddObservation(game.observations);
    }

    public override void OnActionReceived(ActionBuffers actions){
        /* 2 discrete branches
        -- Rotation -> No rotation, Rotate Right, Rotate Left
        -- Movement -> No movement, Move left, Move right, Soft Drop, Hard Drop
        */

        // Rotation
        switch(actions.DiscreteActions[0]){
            case 0:
                break;
            case 1:
                controller.RotateRight();
                break;
            case 2:
                controller.RotateLeft();
                break;
            default:
                break;
        }

        // Movement
        switch(actions.DiscreteActions[1]){
            case 0:
                break;
            case 1:
                controller.MoveLeft();
                break;
            case 2:
                controller.MoveRight();
                break;
            case 3:
                controller.SoftDrop();
                break;
            //case 4:
            //    controller.HardDrop();
            //    break;
            default:
                break;
        }
    }

    public void CheckDroppedReward(int lowestRow)
    {
        // favour getting lines at the bottom of the grid
        // multiplier will be in range 20 - lowest row / 5 (4.4)
        float multiplier = (game.BoardSize.y - lowestRow) / 5f;
        float reward = Mathf.Pow(curLines, 2) * 10 * multiplier;
        AddReward(reward);
        // update score
        totalScore += reward;
        textScore.text = "Score: " + totalScore;
    }

    public void TetrisScoringReward(int reward){
        AddReward(reward);
        // update score
        totalScore += reward;
        textScore.text = "Score: " + totalScore;

        textMaxLines.text = "Max Lines: " + maxLines;
        textCurLines.text = "Lines Cleared: " + curLines;
        textTspin.text = "T-Spins: " + sTspin + " " + dTspin + " " + tTspin;
        textPC.text = "PC: " + sPc + " " + dPc + " " + tPc;
        textB2B.text = "B2B: " + b2b;
        textMaxCombo.text = "Max Combo: " + maxCombo;
    }

    public void GameOverReward() {
        AddReward(-100.0f);
    }
}
