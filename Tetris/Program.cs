using System;
using System.Threading;

class Program
{
    static void Main()
    {
        Console.WriteLine("Geben Sie eine Spielfeld-Breite ein (Enter width):     (Standard ist 10*20)");
        int width = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine("Geben Sie eine Spielfeld-Höhe ein (Enter heigth:     (Standard ist 10*20)");
        int height = Convert.ToInt32(Console.ReadLine());

        Console.Clear();

        Game game = new Game(width, height);
        game.Start();
    }
}

public class Game
{
    private Board board;
    private Logic logic;

    public Game(int width, int height)
    {
        board = new Board(width, height + 1);
        logic = new Logic(board);
    }

    public void Start()
    {
        Thread controlThread = new Thread(new ThreadStart(logic.Controls));
        controlThread.Start();

        while (true)
        {
            Draw();
        }
    }

    public void Draw()
    {
        Console.Clear();
        board.Draw(logic);
        logic.Run();
    }
}

public class Board
{
    public int[,] grid;
    public ConsoleColor[,] colors;
    public Board(int columns, int rows)
    {
        grid = new int[rows, columns];
        colors = new ConsoleColor[rows, columns];
    }

    public void Draw(Logic logic)
    {
        Console.WriteLine();
        for (int i = 1; i < grid.GetLength(0); i++)
        {
            Console.Write($"{grid.GetLength(0) - i:d2}| ");
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                if (grid[i, j] == 1)
                {
                    Console.ForegroundColor = colors[i, j];                    
                    Console.Write("██");
                }
                else
                {
                    Console.Write("  ");
                }
            }
            Console.WriteLine();
            Console.ResetColor();
        }
        for (int i = 0; i <= grid.GetLength(1) + 1; i++)
        {
            Console.Write("--");
        }
        Console.WriteLine();
        Console.WriteLine($"{logic.score}");
    }
}

public class Block
{
    public int[,] shape;
    public int select;
    public ConsoleColor color;

    public Block(int[,] shape)
    {
        this.shape = shape;
        this.color = GetRandomColor();
    }

    public static ConsoleColor GetRandomColor()
    {
        Random rand = new Random();
        ConsoleColor[] colors = {
            ConsoleColor.Red, ConsoleColor.Green, ConsoleColor.Blue,
            ConsoleColor.Yellow, ConsoleColor.Cyan, ConsoleColor.Magenta
        };
        return colors[rand.Next(colors.Length)];
    }

    public static Block CreateRandomBlock()
    {
        Random rand = new Random();
        int select = rand.Next(0, 7);

        switch (select)
        {
            case 0:
                return new Block(new int[,] { { 1, 1, 1, 1 } });
            case 1:
                return new Block(new int[,] { { 1, 0, 0 }, { 1, 1, 1 } });
            case 2:
                return new Block(new int[,] { { 0, 0, 1 }, { 1, 1, 1 } });
            case 3:
                return new Block(new int[,] { { 1, 1 }, { 1, 1 } });
            case 4:
                return new Block(new int[,] { { 0, 1, 1 }, { 1, 1, 0 } });
            case 5:
                return new Block(new int[,] { { 1, 1, 0 }, { 0, 1, 1 } });
            case 6:
                return new Block(new int[,] { { 0, 1, 0 }, { 1, 1, 1 } });
            default:
                return new Block(new int[,] { { 0 } });
        }
    }

    public void RotateClockwise()
    {
        int rows = shape.GetLength(0);
        int cols = shape.GetLength(1);
        int[,] newShape = new int[cols, rows];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                newShape[j, rows - 1 - i] = shape[i, j];
            }
        }
        shape = newShape;
    }
}

public class Logic
{
    public bool blockExists = false;
    Board board;
    Block block;
    private int mid;
    private int currentRow = 0;
    private char lastInput = ' ';
    public int score = 0;

    public Logic(Board board)
    {
        this.board = board;
    }

    public void Run()
    {
        if (!blockExists)
        {
            if (IsGameOver())
            {
                GameOver();
            }
            else
            {
                block = Block.CreateRandomBlock();
                mid = (board.grid.GetLength(1) - block.shape.GetLength(1)) / 2;
                currentRow = 0;
                blockExists = true;
                DrawBlockOnBoard();
            }
        }
        else
        {
            Fall();
            HandleControls(ref mid);
            CheckIfTetris();
        }
    }

    public void Fall()
    {
        int milliseconds = 500;
        Thread.Sleep(milliseconds);

        if (CanMoveDown())
        {
            ClearBlockFromBoard();
            currentRow++;
            DrawBlockOnBoard();
        }
        else
        {
            MergeBlockToBoard();
            blockExists = false;
        }
    }

    bool CanMoveDown()
    {
        for (int j = 0; j < block.shape.GetLength(1); j++)
        {
            for (int i = block.shape.GetLength(0) - 1; i >= 0; i--)
            {
                if (block.shape[i, j] == 1)
                {
                    int newRow = currentRow + i + 1;
                    if (newRow >= board.grid.GetLength(0) || board.grid[newRow, mid + j] == 1)
                    {
                        return false;
                    }
                    break;
                }
            }
        }
        return true;
    }

    void ClearBlockFromBoard()
    {
        for (int i = 0; i < block.shape.GetLength(0); i++)
        {
            for (int j = 0; j < block.shape.GetLength(1); j++)
            {
                if (block.shape[i, j] == 1)
                {
                    board.grid[currentRow + i, mid + j] = 0;
                }
            }
        }
    }

    void DrawBlockOnBoard()
    {
        for (int i = 0; i < block.shape.GetLength(0); i++)
        {
            for (int j = 0; j < block.shape.GetLength(1); j++)
            {
                if (block.shape[i, j] == 1)
                {
                    board.grid[currentRow + i, mid + j] = 1;
                    board.colors[currentRow + i, mid + j] = block.color;
                }
            }
        }
    }

    void MergeBlockToBoard()
    {
        for (int i = 0; i < block.shape.GetLength(0); i++)
        {
            for (int j = 0; j < block.shape.GetLength(1); j++)
            {
                if (block.shape[i, j] == 1)
                {
                    board.grid[currentRow + i, mid + j] = 1;
                }
            }
        }
    }

    public void Controls()
    {
        while (true)
        {
            lastInput = Convert.ToChar(Console.ReadKey(true).KeyChar);
        }
    }

    void HandleControls(ref int mid)
    {
        switch (lastInput)
        {
            case 'w':
                Rotate();
                break;
            case 'a':
                if (CanMoveLeft())
                {
                    ClearBlockFromBoard();
                    mid -= 1;
                    DrawBlockOnBoard();
                }
                break;
            case 's':
                TpDown();
                break;
            case 'd':
                if (CanMoveRight())
                {
                    ClearBlockFromBoard();
                    mid += 1;
                    DrawBlockOnBoard();
                }
                break;
            default:
                break;
        }
        lastInput = ' ';
    }

    bool CanMoveLeft()
    {
        for (int i = 0; i < block.shape.GetLength(0); i++)
        {
            for (int j = 0; j < block.shape.GetLength(1); j++)
            {
                if (block.shape[i, j] == 1)
                {
                    if (mid + j - 1 < 0 || board.grid[currentRow + i, mid + j - 1] == 1)
                    {
                        return false;
                    }
                    break;
                }
            }
        }
        return true;
    }

    bool CanRotate()
    {
        Block testBlock = new Block((int[,])block.shape.Clone());
        testBlock.RotateClockwise();

        for (int i = 0; i < testBlock.shape.GetLength(0); i++)
        {
            for (int j = 0; j < testBlock.shape.GetLength(1); j++)
            {
                if (testBlock.shape[i, j] == 1)
                {
                    int newRow = currentRow + i;
                    int newCol = mid + j;
                    if (!(newRow >= board.grid.GetLength(0) || newCol < 0 || newCol >= board.grid.GetLength(1) || board.grid[newRow, newCol] == 1))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }


    void Rotate()
    {
        if (CanRotate())
        {
            ClearBlockFromBoard();
            block.RotateClockwise();
            DrawBlockOnBoard();
        }
    }

    bool CanMoveRight()
    {
        for (int i = 0; i < block.shape.GetLength(0); i++)
        {
            for (int j = block.shape.GetLength(1) - 1; j >= 0; j--)
            {
                if (block.shape[i, j] == 1)
                {
                    if (mid + j + 1 >= board.grid.GetLength(1) || board.grid[currentRow + i, mid + j + 1] == 1)
                    {
                        return false;
                    }
                    break;
                }
            }
        }
        return true;
    }

    public void TpDown()
    {
        ClearBlockFromBoard();

        while (CanMoveDown())
        {
            currentRow++;
        }

        DrawBlockOnBoard();

        MergeBlockToBoard();

        blockExists = false;
    }

    public void CheckIfTetris()
    {
        for (int i = board.grid.GetLength(0) - 1; i >= 0; i--)
        {
            if (IsFullRow(i))
            {
                RemoveRow(i);
                ShiftRowsDown(i);
                i++;
                score++;
            }
        }
    }

    private bool IsFullRow(int row)
    {
        for (int j = 0; j < board.grid.GetLength(1); j++)
        {
            if (board.grid[row, j] == 0)
            {
                return false;
            }
        }
        return true;
    }

    private void RemoveRow(int row)
    {
        for (int j = 0; j < board.grid.GetLength(1); j++)
        {
            board.grid[row, j] = 0;
        }
    }

    private void ShiftRowsDown(int fromRow)
    {
        for (int i = fromRow; i > 0; i--)
        {
            for (int j = 0; j < board.grid.GetLength(1); j++)
            {
                board.grid[i, j] = board.grid[i - 1, j];
            }
        }
        for (int j = 0; j < board.grid.GetLength(1); j++)
        {
            board.grid[0, j] = 0;
        }
    }

    bool IsGameOver()
    {
        for (int j = 0; j < board.grid.GetLength(1); j++)
        {
            if (board.grid[0, j] == 1)
            {
                return true;
            }
        }
        return false;
    }

    void GameOver()
    {
        Console.WriteLine("Game Over");
        Console.WriteLine("Zum Beenden 'e' eingeben (Enter 'e' to end game");
        char exit = Convert.ToChar(Console.ReadLine());
        if (exit == 'e')
        {
            Environment.Exit(0);
        }
    }
}