using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    public Game game { get; private set; }
    public Vector3Int pos { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int[] tetrominoPositions { get; private set; }
    public int rotationIndex { get; private set; }

    [SerializeField] float stepDelay = 1.0f;
    [SerializeField] float moveDelay = 0.1f;
    [SerializeField] float lockDelay = 0.5f;
    [SerializeField] int totalValidStepsBeforeLock = 2;

    private float stepTime;
    private float moveTime;
    private float lockTime;
    private int validLockingCount;

    private int comboCount = -1;
    private int backToBack = -1;
    private bool tspin;

    [SerializeField] bool playerMode = false;
    public bool PlayerMode { get => playerMode; set => playerMode = value; }


    public void Init(Game game, Vector3Int pos, TetrominoData data) {
        this.game = game;
        this.pos = pos;
        this.data = data;
        this.rotationIndex = 0;

        this.stepTime = Time.time + stepDelay;
        this.lockTime = 0.0f;
        this.moveTime = Time.time + moveDelay;
        this.validLockingCount = 0;

        if(this.tetrominoPositions == null){
            this.tetrominoPositions = new Vector3Int[data.location.Length];
        }

        for(int x = 0; x < data.location.Length; x++){
            this.tetrominoPositions[x] = (Vector3Int) data.location[x];
        }
    }

    bool spin = false;

    // since heuristics run in FixedUpdate, which is after an update step
    // we implement psuedo down input registers to register a single press

    [HideInInspector] public bool pressedQ = false;
    [HideInInspector] public bool pressedE = false;
    [HideInInspector] public bool pressedA = false;
    [HideInInspector] public bool pressedD = false;

    void Update() {

        this.game.ClearCurrentTetromino();

        this.lockTime += Time.deltaTime;

        if(playerMode){ // handle this in Heuristic (Agent Script)
            spin = false;
            if(Input.GetKeyDown(KeyCode.Q)){ // ROTATE LEFT
                pressedQ = true;
            } else if(Input.GetKeyDown(KeyCode.E)) { // ROTATE RIGHT
                pressedE = true;
            }

            //if(Input.GetKeyDown(KeyCode.Space)) { // HARD DROP
            //    HardDrop();
            //}

            if(Input.GetKeyDown(KeyCode.A)){ // MOVE LEFT
                pressedA = true;
            } else if(Input.GetKeyDown(KeyCode.D)){ // MOVE RIGHT
                pressedD = true;
            }
            //else if(Input.GetKey(KeyCode.S)){ // SOFT DROP
            //    SoftDrop();
            //}
        }

        if(Time.time >= this.stepTime){
            Step(spin);
        }

        this.game.SetCurrentTetromino();
    }

    #region Game Inputs

    public void RotateLeft(){
        this.game.ClearCurrentTetromino();
        RotateTetromino(-1);
        spin = true;
        this.game.SetCurrentTetromino();
    }

    public void RotateRight(){
        this.game.ClearCurrentTetromino();
        RotateTetromino(1);
        spin = true;
        this.game.SetCurrentTetromino();
    }

    public void HardDrop() {
        this.game.ClearCurrentTetromino();
        while(MoveTetromino(Vector2Int.down)){
            continue;
        }

        Lock();
        this.game.SetCurrentTetromino();
    }

    public void MoveLeft() {
        this.game.ClearCurrentTetromino();
        if (Time.time > moveTime)
            MoveTetromino(Vector2Int.left);
        this.game.SetCurrentTetromino();
    }

    public void MoveRight() {
        this.game.ClearCurrentTetromino();
        if (Time.time > moveTime)
            MoveTetromino(Vector2Int.right);
        this.game.SetCurrentTetromino();
    }

    public void SoftDrop() {
        this.game.ClearCurrentTetromino();
        if(Time.time > moveTime && MoveTetromino(Vector2Int.down))
            stepTime = Time.time + stepDelay;
        this.game.SetCurrentTetromino();
    }

    #endregion

    void HandleMoveInputs() {
        if(Input.GetKeyDown(KeyCode.A)){ // MOVE LEFT
            MoveLeft();
        } else if(Input.GetKeyDown(KeyCode.D)){ // MOVE RIGHT
            MoveRight();
        } else if(Input.GetKey(KeyCode.S)){ // SOFT DROP
            SoftDrop();
        }
    }

    #region Game Rules

    void Step(bool spun) {
        this.stepTime = Time.time + this.stepDelay;
        
        MoveTetromino(Vector2Int.down);

        // calculate scoring
        tspin = data.tetromino == Tetromino.T && spun && this.game.CheckTSpin(pos);


        if(this.lockTime >= this.lockDelay){
            Lock();
        }
    }

    void Lock() {
        this.validLockingCount = 0;
        this.game.SetCurrentTetromino();

        // calculate scoring
        bool perfectClear = false;
        int clearedLines = this.game.CheckLines(out perfectClear);

        if(clearedLines > 0){ // add to combo
            this.comboCount += 1;
        } else { // reset combo
            this.comboCount = -1;
        }

        if(clearedLines == 4 || tspin || perfectClear){ // a difficult move
            this.backToBack += 1;
        } else { // reset combo
            this.backToBack = -1;
        }

        if(this.game.PAgent != null){
            this.game.CalculateAgentScoring(clearedLines, tspin, comboCount, (backToBack > 0), perfectClear);
        } else {
            this.game.CalculateScoring(clearedLines, tspin, comboCount, (backToBack > 0), perfectClear);
        }

        // spawn new piece
        this.game.SpawnTetrominoRandomizer();
    }

    #endregion

    #region Tetris Movement

    bool MoveTetromino(Vector2Int movePos) {
        Vector3Int newPos = this.pos;
        newPos.x += movePos.x;
        newPos.y += movePos.y;
        if(this.game.ValidPos(newPos) && this.validLockingCount < totalValidStepsBeforeLock){
            this.pos = newPos;
            this.lockTime = 0.0f;
            this.moveTime = Time.time + moveDelay;
            return true;
        }

        return false;
    }

    void RotateTetromino(int dir){
        int currentRotationIndex = this.rotationIndex;
        int newRotationIndex = Wrap(this.rotationIndex + dir, 0, 4);

        ApplyRotationMatrix(dir);

        if(!TestWallKick(newRotationIndex, dir)){
            this.rotationIndex = currentRotationIndex;
            ApplyRotationMatrix(-dir);
        }
    }

    void ApplyRotationMatrix(int dir){
        for(int x = 0; x < this.tetrominoPositions.Length; x++){
            Vector3 temp = this.tetrominoPositions[x];
            switch(this.data.tetromino){
                case Tetromino.I:
                case Tetromino.O:
                    // change the pivot point locations (for 4x4 pivot locations)
                    temp.x -= 0.5f;
                    temp.y -= 0.5f;
                    this.tetrominoPositions[x] = new Vector3Int(
                        Mathf.CeilToInt((temp.x * BaseData.RotationMatrix[0] * dir) + (temp.y * BaseData.RotationMatrix[1] * dir)),
                        Mathf.CeilToInt((temp.x * BaseData.RotationMatrix[2] * dir) + (temp.y * BaseData.RotationMatrix[3] * dir)),
                        0
                    );
                    break;
                default:
                    this.tetrominoPositions[x] = new Vector3Int(
                        Mathf.RoundToInt((temp.x * BaseData.RotationMatrix[0] * dir) + (temp.y * BaseData.RotationMatrix[1] * dir)),
                        Mathf.RoundToInt((temp.x * BaseData.RotationMatrix[2] * dir) + (temp.y * BaseData.RotationMatrix[3] * dir)),
                        0
                    );
                    break;
            }
        }
    }

    // WALL KICKS

    bool TestWallKick(int rotIndex, int rotDir){
        int wallKickIndex = GetWallKickIndex(rotIndex, rotDir);
        for(int x = 0; x < this.data.wallKicks.GetLength(1); x++){
            if(MoveTetromino(this.data.wallKicks[wallKickIndex, x]))
                return true;
        }
        return false;
    }

    int GetWallKickIndex(int rotIndex, int rotDir){
        int result = rotIndex * 2;
        if(rotDir < 0) {
            result--;
        }

        return Wrap(result, 0, this.data.wallKicks.GetLength(0));
    }

    // assume wrap lowest is 0
    private int Wrap(int input, int min, int max){
        if (input < min) {
            return max - (min - input) % (max - min);
        } else {
            return min + (input - min) % (max - min);
        }
    }

    #endregion
}
