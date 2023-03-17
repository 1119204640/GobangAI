using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    enum state : int //棋局的形势得分,黑棋为正，白棋为负
    {
        unkown = 0,

        black_sleepOne = 1,
        black_liveOne = 10,
        black_sleepTwo = 10,
        black_sleepThree = 100,
        black_liveTwo = 100,
        black_liveThree = 1000,
        black_sleepFour = 10000,
        black_liveFour = 100000,
        black_five = 1000000,

        white_sleepOne = -1,
        white_liveOne = -10,
        white_sleepTwo = -10,
        white_sleepThree = -100,
        white_liveTwo = -100,
        white_liveThree = -1000,
        white_sleepFour = -10000,
        white_liveFour = -100000,
        white_five = -1000000,

    }

    long score = 0; //整个棋局的总分

    public Transform top_left, top_right, bottom_left, bottom_right; //棋盘四个角
    private Vector2 pos_TL, pos_TR, pos_BL, pos_BR; //四个角的坐标

    private float gridWidth, gridHeight; //每个格子的长和宽
    private Vector2[,] chessPos; //格子的交点，即落子位

    public enum ChessInfo : int //棋子自身状态：空位、黑色、白色、未知
    {
        blank = 0, black = 1, white = 2, unknown = 3
    };
    

    struct eachPointStatus //记录每个交点的状态
    {
        public ChessInfo info; //棋子颜色，是否为空
        public bool isCheck; // 是否已被遍历
    }

    private eachPointStatus[,] snapshotMap; //保存棋盘上交点的状态

    public enum Turn { Black, White };
    Turn turn = Turn.Black;

    public GameObject blackChess;
    public GameObject whiteChess;

    private Vector2 mousePos;

    // Start is called before the first frame update
    void Start()
    {
        InitChessBoard();
    }

    private const float THRESHOLD = 1f; //用来判断曼哈顿距离的阈值

    // Update is called once per frame
    void Update()
    {
        PutChess();
    }

    //初始化棋盘，为棋盘落子位设置交点
    void InitChessBoard()
    {
        chessPos = new Vector2[15, 15];
        snapshotMap = new eachPointStatus[15, 15];

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

    //放置棋子
    void PutChess()
    {
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
                        CreateChess(row, col, chessPos[row,col]);
                        //Evaluate();
                        //Debug.Log("score" + score);
                        
                    }
                }
            }
        }
    }

    //计算鼠标点击时的坐标和哪个棋盘交点最接近
    bool Isnearby(Vector2 mousPos, Vector2 chessPos)
    {
        float distance = Mathf.Abs(mousePos.x - chessPos.x) + Mathf.Abs(mousePos.y - chessPos.y); //曼哈顿距离计算公式
        if (distance < THRESHOLD)
        {
            return true;
        }
        return false;
    }

    //生成棋子
    void CreateChess(int row, int col, Vector2 putPostion)
    {
        if (snapshotMap[row,col].info == ChessInfo.blank) //如果当前无棋子
        {
            snapshotMap[row, col].info = (turn == Turn.Black ? ChessInfo.black : ChessInfo.white);
            switch (turn)
            {
                case Turn.Black:
                    Instantiate(blackChess, new Vector3(putPostion.x, putPostion.y, 0), Quaternion.identity);
                    turn = Turn.White;
                    break;
                case Turn.White:
                    Instantiate(whiteChess, new Vector3(putPostion.x, putPostion.y, 0), Quaternion.identity);
                    turn = Turn.Black;
                    break;

            }
        }
    }

    int Check(ChessInfo[] group) //辅助判断几连子
    {
        //int connect_num = 0; //统计几连子
        //for (int i = 0; i < 5; i++)
        //{
        //    switch 
        //}

        return 0;
    }

    int Analysis(int row, int col) //以当前棋子为中心，分析某一行（列、斜角）属于活一、活二、眠二……中哪种情况
    {
        //ChessInfo[] group = new ChessInfo[5]
        //{ChessInfo.unknown, ChessInfo.unknown, ChessInfo.unknown, ChessInfo.unknown, ChessInfo.unknown};
        
        ////横向
        //for (int i = 0; i < 5; i++) //左->右
        //{
        //    if ((row + i) > 14 || (row + i) < 0)
        //    {
        //        //超出棋盘范围
        //        break;
        //    }
        //    else
        //    {
        //        //放入数组中
        //        group[i] = snapshotMap[row + i, col];
        //    }
        //}
        //switch (Check(group))
        //{

        //}
        

        ////总向

        ////斜向
        
        return 0;
    }

    void Evaluate() //计算整个当前棋局的总分
    {
        //for (int row = 0; row < 15; row++)
        //{
        //    for (int col = 0; col < 15; col++)
        //    {
        //        if (snapshotMap[row, col] != ChessInfo.blank)
        //        {
        //            score += Analysis(row, col);
        //        }
        //    }
        //}
    }

    //搜索到某行某列是black或者white后，开始以这个坐标开始进行深度搜索
    void DFS(int row, int col, ChessInfo info)
    {
        //横向
        for (int i = 0; i < 5; i++)
        {

        }
        //纵向

        //斜向
    }
}
