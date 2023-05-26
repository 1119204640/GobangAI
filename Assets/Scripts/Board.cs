using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    public Transform top_left, top_right, bottom_left, bottom_right; //棋盘四个角
    public enum ChessInfo : int //棋子自身状态：空位、黑色、白色
    {
        blank = 0, black = 1, white = -1
    };

    private Dictionary<string, int> scoreStatus = new Dictionary<string, int>() {
        {"one", 1}, {"two", 5}, {"three", 100}, {"four", 10000}, {"five", 10000000}
    };
    public enum Turn { Black, White };
    public GameObject blackChess;  //黑棋
    public GameObject whiteChess;  //白棋
    // public Image winImage;
    // public Sprite blackWinSprite;
    // public Sprite whiteWinSprite;
    
    private Vector2 pos_TL, pos_TR, pos_BL, pos_BR; //四个角的坐标
    private float gridWidth, gridHeight; //每个格子的长和宽
    private Vector2[,] chessPos; //格子的交点，即落子位
    private ChessInfo[,] board; //保存棋盘上交点的状态
    private Vector2 mousePos;
    private Turn turn = Turn.Black; //黑色先手
    private bool isGameOver = false;
    private int LastMoveRow = -1;  
    private int LastMoveCol = -1;  //记录最后一步棋的行列
    private const float THRESHOLD = 1f; //用来判断曼哈顿距离的阈值
    private const int DEPTH = 2; //搜索深度
    public Image winImage;
    public Sprite blackWinSprite;
    public Sprite whiteWinSprite;
    
    private void Start() {
        InitChessBoard();
    }
    
    private void Update() {
        HumanPlay();

        AIPlay();
    }

    //初始化棋盘，为棋盘落子位设置交点
    private void InitChessBoard()
    {
        chessPos = new Vector2[15, 15];
        board = new ChessInfo[15, 15];
        for (int row = 0; row < 15; row++) {
            for (int col = 0; col < 15; col++) {
                board[row,col] = ChessInfo.blank;
            }
        } 

        pos_TL = top_left.position;
        pos_TR = top_right.position;
        pos_BL = bottom_left.position;
        pos_BR = bottom_right.position;

        gridWidth = (pos_BR.x - pos_BL.x) / 14f;
        gridHeight = (pos_TR.y - pos_BR.y) / 14f;

        for (int row = 0; row < 15; row++)
        {
            for (int col = 0; col < 15; col++)
            {
                float pos_x = gridWidth * (col - 7);
                float pos_y = gridHeight * (row - 7);
                chessPos[row, col] = new Vector2(pos_x, pos_y); //计算每个交点的坐标

                GameObject obj = new GameObject(row + "," + col); //为每个交点定锚点对象
                obj.transform.position = new Vector2(chessPos[row, col].x, -chessPos[row, col].y);
                obj.transform.SetParent(this.transform);
            }
        }
        
    }

    public void HumanPlay() {
        if (Input.GetMouseButtonDown(0))
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Debug.DrawRay(mousePos, Camera.main.transform.forward, Color.red);
            //Debug.Log("ScreenPostion: " + Input.mousePosition + " WorldPostion: " + mousePos);

            for (int row = 0; row < 15; row++) //每点击一次鼠标，就逐个判断当前坐标离哪个棋盘交点最近（复杂度太高，日后考虑用raycast，或者改成用button）
            {
                for (int col = 0; col < 15; col++)
                {
                    if (Isnearby(mousePos, chessPos[row,col]))
                    {
                        //Debug.Log("mousePostion = " + mousePos + " chessPostion(row, col) = " + "(" + row + "," + col + ")");
                        if (turn == Turn.Black && !isGameOver) {
                            HumanPutChess(row, col, chessPos[row,col]);
                            turn = Turn.White;
                            Debug.Log("after black: " + Evaluate());
                        }
                        
                    }
                }
            }
        }
    }

    public void AIPlay() {
        if (turn == Turn.White && !isGameOver) {
                AIputChess();
                turn = Turn.Black;
                Debug.Log("after white: " + Evaluate());
        }
    }
    
    private void AIputChess() { // AI放置棋子
        // Debug.Log("-----------AI现在开始-----------");
        int row = 0;
        int col = 0;
        // Debug.Log("可行空位数： " + GetMoves().Count);
        if (GetMoves().Count > 0) {

            // row = GetMoves()[0][0];
            // col = GetMoves()[0][1];
            row = FindBestMove(DEPTH)[0];
            col = FindBestMove(DEPTH)[1];
        }
        // Debug.Log("next move: " + row + ", " + col);
        Instantiate(whiteChess, new Vector3(chessPos[row,col].x, chessPos[row,col].y, 0), Quaternion.identity);
        board[row, col] = ChessInfo.white;
        LastMoveRow  = row;
        LastMoveCol = col;
        // Debug.Log("-----------AI已经结束-----------");
    }

    private List<int[]> GetMoves() { //获取当前可行的落子位置
        List<int[]> moveList = new List<int[]>();
        for (int row = 14; row >= 0; row--) {
            for (int col = 0; col < 15; col++) {
                if (board[row,col] == ChessInfo.blank) {
                    moveList.Add(new int[] {row, col});
                }
            }
        }
        return moveList;
    }

    //计算鼠标点击时的坐标和哪个棋盘交点最接近
    private bool Isnearby(Vector2 mousPos, Vector2 chessPos)
    {
        float distance = Mathf.Abs(mousePos.x - chessPos.x) + Mathf.Abs(mousePos.y - chessPos.y); //曼哈顿距离计算公式
        if (distance < THRESHOLD)
        {
            return true;
        }
        return false;
    }

    //生成人类玩家方的棋子,可判断胜负和当前局面评分
    private void HumanPutChess(int row, int col, Vector2 putPostion)
    {
        if (board[row,col] == ChessInfo.blank && board[row, col] != ChessInfo.white) //如果当前无棋子或者不是白棋子
        {
            Instantiate(blackChess, new Vector3(putPostion.x, putPostion.y, 0), Quaternion.identity);
            board[row,col] = ChessInfo.black;
            LastMoveRow  = row;
            LastMoveCol = col;
        }
    }

    private ChessInfo[] GetRow(int row, int col) {
        ChessInfo[] result = new ChessInfo[5];
        for (int i = 0; i < 5; i++) {
            result[i] = board[row, col + i];
        }
        return result;
    }

    private ChessInfo[] GetCol(int row, int col) { 
        ChessInfo[] result = new ChessInfo[5];
        for (int i = 0; i < 5; i++) {
            result[i] = board[row + i, col];
        }
        return result;
    }

    public ChessInfo[] GetDiagonal(int row, int col) {
        ChessInfo[] result = new ChessInfo[5];
        for (int i = 0; i < 5; i++) {
            result[i] = board[row + i, col - i];
        }
        return result;
    }

    public ChessInfo[] GetAntiDiagonal(int row, int col) {
        ChessInfo[] result = new ChessInfo[5];
        for (int i = 0; i < 5; i++) {
            result[i] = board[row + i, col + i];
        }
        return result;
    }

    public int EvaluateBlack()
    {
        int score = 0;
        
        // Evaluate rows
        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 11; j++) {
                if (board[i, j] == ChessInfo.black) {
                    ChessInfo[] row = GetRow(i, j);
                    score += EvaluateLine(row);
                }
            }
        }

        // Evaluate columns
        for (int i = 0; i < 11; i++) {
            for (int j = 0; j < 15; j++) {
                if (board[i, j] == ChessInfo.black) {
                    ChessInfo[] col = GetCol(i, j);
                    score += EvaluateLine(col);
                }
            }
        }

        // Evaluate diagonals
        for (int i = 0; i < 11; i++) {
            for (int j = 4; j < 15; j++) {
                if (board[i, j] == ChessInfo.black) {
                    ChessInfo[] diagonal = GetDiagonal(i, j);
                    score += EvaluateLine(diagonal);
                }
            }
        }

        // Evaluate anti-diagonals
        for (int i = 0; i < 11; i++) {
            for (int j = 0; j < 11; j++) {
                if (board[i, j] == ChessInfo.black) {
                    ChessInfo[] anti_diagonal = GetAntiDiagonal(i, j);
                    score += EvaluateLine(anti_diagonal);
                }
            }
        }
        return score;
    }

    public int EvaluateWhite()
    {
        int score = 0;
        
        // Evaluate rows
        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 11; j++) {
                if (board[i, j] == ChessInfo.white) {
                    ChessInfo[] row = GetRow(i, j);
                    score -= EvaluateLine(row);
                }
            }
        }

        // Evaluate columns
        for (int i = 0; i < 11; i++) {
            for (int j = 0; j < 15; j++) {
                if (board[i, j] == ChessInfo.white) {
                    ChessInfo[] col = GetCol(i, j);
                    score -= EvaluateLine(col);
                }
            }
        }

        // Evaluate diagonals
        for (int i = 0; i < 11; i++) {
            for (int j = 4; j < 15; j++) {
                if (board[i, j] == ChessInfo.white) {
                    ChessInfo[] diagonal = GetDiagonal(i, j);
                    score -= EvaluateLine(diagonal);
                }
            }
        }

        // Evaluate anti-diagonals
        for (int i = 0; i < 11; i++) {
            for (int j = 0; j < 11; j++) {
                if (board[i, j] == ChessInfo.white) {
                    ChessInfo[] anti_diagonal = GetAntiDiagonal(i, j);
                    score -= EvaluateLine(anti_diagonal);
                }
            }
        }
        return score;
    }

    public int Evaluate() {
        // Debug.Log("黑色得分：" + EvaluateBlack());
        // Debug.Log("白色得分：" + EvaluateWhite());
        return EvaluateBlack() + EvaluateWhite();
    }

    //判断五元组里属于哪种情况
    private int EvaluateLine(ChessInfo[] line)
    {
        ChessInfo player = line[0];
        ChessInfo opponent = (player == ChessInfo.black ? ChessInfo.white : ChessInfo.black);
        int score = 0;
        int len = line.Length;
        bool blocked = false;
        int count = 0;

        // 五元组中包含了对方颜色的任意枚数棋子，直接评0分
        for (int i = 0; i < len; i++)
        {
            if (line[i] == opponent) {
                count = 0;
                blocked = true;
                break;
            }
            else if (line[i] == player) {
                count ++;
            }
        }
        if (!blocked) {
            switch (count) {
                case 0:
                score += 0;
                break;
                case 1: 
                score += scoreStatus["one"];
                break;
                case 2: 
                score += scoreStatus["two"];
                break;
                case 3: 
                score += scoreStatus["three"];
                break;
                case 4: 
                score += scoreStatus["four"];
                break;
                case 5: 
                score += scoreStatus["five"];
                isGameOver = true;
                if (player == ChessInfo.black) {
                    winImage.gameObject.SetActive(true);
                    winImage.sprite = blackWinSprite;
                }else {
                    winImage.gameObject.SetActive(true);
                    winImage.sprite = whiteWinSprite;
                }
                break;
            }
        }
        // Debug.Log("五元组中棋子个数：" + count);
        return score;
    }


    public int[] FindBestMove(int depth) //AI寻找最小值
    {
        int[] bestMove = null;
        int bestScore = int.MaxValue;

        foreach (int[] move in GetMoves()) {
            board[move[0], move[1]] = ChessInfo.white;
            // int score = Minimax(depth, false);
            int alpha = int.MinValue;
            int beta = int.MaxValue;
            int score = AlphaBeta(depth, alpha, beta, false);
            if (score < bestScore) {
                bestScore = score;
                bestMove = move;
            }
            board[move[0], move[1]] = ChessInfo.blank;
        }
        
        return bestMove;
    }

    public int Minimax(int depth, bool isMax) {
        if (isGameOver || depth == 0) {
            return Evaluate();
        }
        if (isMax) {  //人类是Max
            int bestVal = int.MinValue;
            foreach (int[] move in GetMoves()) {
                int value = Minimax(depth-1, false);
                bestVal = Mathf.Max(value, bestVal);
            }
            return bestVal;
        }
        else {  //AI是Min
            int bestVal = int.MaxValue;
            foreach (int[] move in GetMoves()) {
                int value = Minimax(depth-1, true);
                bestVal = Mathf.Min(value, bestVal);
            }
            return bestVal;
        }
    }

    public int AlphaBeta(int depth, int alpha, int beta, bool isMax) {
        if (depth == 0 || isGameOver) {
            return Evaluate();
        }
        if (isMax) {
            int bestVal = int.MinValue;
            foreach (int[] move in GetMoves()) {
                int value = AlphaBeta(depth-1, alpha, beta, false);
                bestVal = Mathf.Max(value, bestVal);
                alpha = Mathf.Max(alpha, bestVal);
                if (beta <= alpha) {
                    break;
                }
            }
            return bestVal;
        } else {
            int bestVal = int.MaxValue;
            foreach (int[] move in GetMoves()) {
                int value = AlphaBeta(depth-1, alpha, beta, true);
                bestVal = Mathf.Min(value, bestVal);
                beta = Mathf.Min(beta, bestVal);
                if (beta <= alpha) {
                    break;
                }
            }
            return bestVal;
        }
    }
}

