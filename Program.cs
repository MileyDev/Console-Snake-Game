using System;
using System.Net.NetworkInformation;

Random random = new Random();
Console.CursorVisible = false;
int height = Console.WindowHeight - 1;
int width = Console.WindowWidth - 5;
bool shouldExit = false;
int score = 0;
int lastScore = -1;
int moveDelay = 50;
int timeLeft = 60;
int endTime = -1;
DateTime timeCheck = DateTime.Now;
DateTime boostEndTime = DateTime.MinValue;

// Console position of the player
int playerX = 0;
int playerY = 1;

// Console position of the food
int foodX = 0;
int foodY = 0;

// Available player and food strings
string[] states = { "(o_o)", "(^_^)", "(x_x)" };
string[] foods = { "🍕", "💎", "💣" };

// Current player string displayed in the Console
string player = states[0];

// Index of the current food
int food = 0;

Console.BackgroundColor = ConsoleColor.Black;
Console.Clear();
InitializeGame();

while (!shouldExit)
{
    UpdateTime();
    ScoreBoard();

    if (TerminalResized())
    {
        Console.SetCursorPosition(1, height);
        Console.Write("Console was resized. Exiting...");
        shouldExit = true;
    }

    if (PlayerSick())
    {
        FreezePlayer();
    }
    else
    {
        bool fast = FastSpeed();
        Move(speed: fast ? 2 : 1);
    }

    if (FoodConsumed())
    {
        score++;
        ChangePlayer();
        ShowFood();
    }
    if (DateTime.Now > boostEndTime && moveDelay < 30)
    {
        moveDelay = 50;
    }

    System.Threading.Thread.Sleep(moveDelay);
}

// Returns true if the Terminal was resized 
bool TerminalResized()
{
    return height != Console.WindowHeight - 1 || width != Console.WindowWidth - 5;
}

void UpdateTime()
{
    if ((DateTime.Now - timeCheck).TotalSeconds >= 1)
    {
        timeLeft--;
        timeCheck = DateTime.Now;

        if (timeLeft <= 0)
        {
            Console.Clear();
            Console.WriteLine("⏱️  Time's up!");
            Console.WriteLine($"🏆 Final Score: {score}");
            shouldExit = true;
        }
    }
}

void ScoreBoard()
{
    if (timeLeft != endTime || score != lastScore)
    {
        Console.SetCursorPosition(0, 0);
        if (timeLeft <= 10)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
        }

        Console.Write($"Score: {score}   Time left: {timeLeft}s    ");
        Console.ResetColor();

        endTime = timeLeft;
        lastScore = score;
    }
}
// Displays random food at a random location
void ShowFood()
{
    // Update food to a random index
    food = random.Next(0, foods.Length);

    // Update food position to a random location
    foodX = random.Next(0, width - player.Length);
    foodY = random.Next(0, height - 1);

    // Display the food at the location
    Console.SetCursorPosition(foodX, foodY);
    Console.Write(foods[food]);
}

// Changes the player to match the food consumed
void ChangePlayer()
{
    player = states[food];
    switch (player)
    {
        case "(o_o)":
            Console.ForegroundColor = ConsoleColor.Yellow;
            timeLeft += 3;
            moveDelay = 25;
            break;
        case "(^_^)":
            Console.ForegroundColor = ConsoleColor.Blue;
            moveDelay = 5;
            timeLeft += 10;
            boostEndTime = DateTime.Now.AddSeconds(4);
            break;
        case "(x_x)":
            Console.ForegroundColor = ConsoleColor.Red;
            moveDelay = 50;
            timeLeft -= 10;
            break;
    }

    //To avoid negative or extreme time and score
    if (score < 0) score = 0;
    if (timeLeft < 0) timeLeft = 0;
    if (timeLeft > 999) timeLeft = 999;

    Console.SetCursorPosition(playerX, playerY);
    Console.Write(player);
}

// Temporarily stops the player from moving
void FreezePlayer()
{
    System.Threading.Thread.Sleep(1500);
    score -= 2;
    player = states[0];
}

// Clears the console, displays the food and player
void InitializeGame()
{
    Console.Clear();
    ShowFood();
    Console.SetCursorPosition(0, 0);
    Console.Write(player);
}

// checks if player has consumed food
bool FoodConsumed()
{
    int range = player.Length - 1;
    return Math.Abs(playerX - foodX) <= range && playerY == foodY;
}


// checks for current player, then freezes player
bool PlayerSick() => player == states[2];

// checks for player state, then determine the move speed
bool FastSpeed() => player.Equals(states[1]);

// Reads directional input from the Console and moves the player
void Move(bool game = false, int speed = 1)
{
    int lastX = playerX;
    int lastY = playerY;

    if (Console.KeyAvailable)
    {
        ConsoleKeyInfo key = Console.ReadKey(true);
        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                playerY--;
                break;
            case ConsoleKey.DownArrow:
                playerY++;
                break;
            case ConsoleKey.LeftArrow:
                playerX -= speed;
                break;
            case ConsoleKey.RightArrow:
                playerX += speed;
                break;
            case ConsoleKey.Escape:
                shouldExit = true;
                break;
            default:
                shouldExit = game;
                break;
        }
    }


    // Clear the characters at the previous position
    Console.SetCursorPosition(lastX, lastY);
    Console.Write(new string(' ', player.Length));

    // Keep player position within the bounds of the Terminal window
    playerX = (playerX < 0) ? 0 : (playerX >= width ? width : playerX);
    playerY = (playerY < 0) ? 0 : (playerY >= height ? height : playerY);

    // Draw the player at the new location
    Console.SetCursorPosition(playerX, playerY);
    Console.ForegroundColor = player switch
    {
        "(^_^)" => ConsoleColor.Blue,
        "(x_x)" => ConsoleColor.Red,
        "(o_o)" => ConsoleColor.Yellow,
    };
    Console.Write(player);
    Console.ResetColor();

    Thread.Sleep(50);
}
