// Game coded by JT Stukes
// A game by JT Stukes, copied from a physical game with the same rules

using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Xml.XPath;

int diceSides = 6; // standard six sided dice or any number really
int diceCount = 5; // must be >0 up to any amount, 5 dice = 3.7 million boards ~1sec, 6 dice = 672 million boards ~minutes

int[] gameBoard = GameRoll(diceSides, diceCount);

// Rolling dice and creating a gameboard, with printer

int DiceRoll(int sides)
{
    Random random = new Random();
    int roll = random.Next(1, sides + 1);
    return roll;
}

int[] GameRoll(int diceSides, int diceCount)
{
    int[] gameBoard = new int[diceCount + 2]; // last two dice are solution dice

    for (int i = 0; i < gameBoard.Length; i++)
    {
        gameBoard[i] = DiceRoll(diceSides);
    }
    gameBoard[diceCount + 1] = gameBoard[diceCount + 1] * 10; // last solution die is stored as 10s
    return gameBoard;
}

void PrintGameBoard(int[] gameBoard)
{
    Console.WriteLine($"You rolled: ");
    for (int i = 0; i < gameBoard.Length - 2; i++)
    {
        Console.Write($" {gameBoard[i]} ");
    }
    Console.WriteLine($"\nYou much reach: {gameBoard[gameBoard.Length - 1] + gameBoard[gameBoard.Length - 2]}");
}




// Creating a solver that iterates ALL possible solutions - no shortcutting or reducing due to duplicates, or transitive property of + or *

int countSolved = 0;
int countChecked = 0;
char[] operators = { '+', '-', '*', '/', '^', 'V' }; // using 'V' to denote square root
int goal = 0;
bool displayResults = true;

SolverWrapper(gameBoard);

// Create a wrapper method, sets counters and initializes the input for the reduce method

void SolverWrapper(int[] gameBoard)
{
    if (gameBoard.Length < 3)
    {
        Console.WriteLine("Invalid gameboard");
        return;
    }
    Console.WriteLine("Starting");
    string[] answerPath = new string[gameBoard.Length - 3];
    int[] workingSet = new int[gameBoard.Length - 2];
    for (int i = 0; i < workingSet.Length; i++)
    {
        workingSet[i] = gameBoard[i];
    }
    goal = gameBoard[gameBoard.Length - 2] + gameBoard[gameBoard.Length - 1];
    countSolved = 0;
    countChecked = 0;

    // SolverCall
    SolveGame(answerPath, workingSet);

    Console.WriteLine($"There are {countSolved} solutions.");
    Console.WriteLine($"There are {countChecked} checked.");
    PrintGameBoard(gameBoard);

}

// Solver designed to call itself, reducing array length
void SolveGame(string[] answerPath, int[] workingSet)
{
    // mathOperation called from above foreach loop
    // Check to print
    if (workingSet.Length == 1)
    {
        countChecked++;
        if (countChecked % 1000000 == 0) // print a status for large die counts to show progress
        {
            Console.WriteLine($"Checked {countChecked}");
        }
        if (workingSet[0] == goal)
        {
            countSolved++;
            if (displayResults)
            {
                PrintSolution(answerPath, workingSet[0]); // prints answerPath for each solution, but does not save anywhere
            }
            return;
        }
    }

    for (int i = 0; i < workingSet.Length; i++)
    {
        for (int j = 0; j < workingSet.Length; j++)
        {
            if (i != j) // cannot use the same die twice
            {
                foreach (char mathOperator in operators)
                {
                    answerPath[diceCount - workingSet.Length] = $"({workingSet[i]} {mathOperator} {workingSet[j]})";
                    int[] staticSet = new int[workingSet.Length];
                    for (int m = 0; m < workingSet.Length; m++) // remember old workingSet values
                    {
                        staticSet[m] = workingSet[m];
                    }
                    SolveGame(answerPath, ArrayMinusOne(i, j, mathOperator, workingSet));
                    for (int m = 0; m < workingSet.Length; m++)
                    {
                        workingSet[m] = staticSet[m]; // revert workingSet values
                    }
                }
            }
        }
    }
}


int[] ArrayMinusOne(int i, int j, char mathOperator, int[] setToBeReduced)
{
    int[] newWorkingSet = new int[setToBeReduced.Length - 1];
    int newValue = -1;

// combine i and j values based on operator
    switch (mathOperator) 
    {
        case '+':
            newValue = GameAdd(setToBeReduced[i], setToBeReduced[j]);
            break;
        case '-':
            newValue = GameSubtract(setToBeReduced[i], setToBeReduced[j]);
            break;
        case '*':
            newValue = GameMultiply(setToBeReduced[i], setToBeReduced[j]);
            break;
        case '/':
            newValue = GameDivide(setToBeReduced[i], setToBeReduced[j]);
            break;
        case '^':
            newValue = GamePower(setToBeReduced[i], setToBeReduced[j]);
            break;
        case 'V':
            newValue = GameRoot(setToBeReduced[i], setToBeReduced[j]);
            break;
    }
// making sure last value is not still valid
    if (i == newWorkingSet.Length)
    {
        setToBeReduced[j] = newValue;
    }
    else if (j == newWorkingSet.Length)
    {
        setToBeReduced[i] = newValue;
    }
    else
    {
        setToBeReduced[i] = newValue;
        setToBeReduced[j] = setToBeReduced[newWorkingSet.Length];
    }

    setToBeReduced[newWorkingSet.Length] = 0;
    for (int x = 0; x < newWorkingSet.Length; x++)
    {
        newWorkingSet[x] = setToBeReduced[x];
    }
    return newWorkingSet;
}

// GameOperators shorcut some operations based on basic rules, -9999 auto skips all future depth

int GameAdd(int first, int second)
{
    if (first == -9999 || second == -9999)
    {
        return -9999;
    }
    return first + second;
}

int GameSubtract(int first, int second)
{
    if (first == -9999 || second == -9999)
    {
        return -9999;
    }
    else if (first - second < 0)
    {
        return -9999;
    }
    return first - second;
}

int GameMultiply(int first, int second)
{
    if (first == -9999 || second == -9999)
    {
        return -9999;
    }
    return first * second;
}

int GameDivide(int first, int second)
{
    if (first == -9999 || second == -9999)
    {
        return -9999;
    }
    else if (second == 0)
    {
        return -9999;
    }
    else if (first % second != 0)
    {
        return -9999;
    }
    return first / second;
}

int GamePower(int first, int second)
{
    if (first == -9999 || second == -9999)
    {
        return -9999;
    }
    else if (first == 1 || second == 1)
    {
        return -9999;
    }

    double dFirst = first;
    double dSecond = second;
    double answer = Math.Pow(dFirst, dSecond);
    return (int)answer;
}

int GameRoot(int first, int second)
{
    if (first == -9999 || second == -9999)
    {
        return -9999;
    }
    else if (second == 0 || second == 1 || first == 1)
    {
        return -9999;
    }

    double dFirst = first;
    double dSecond = second;
    double answer = Math.Pow(dFirst, (1 / dSecond));

    if (answer - (int)answer == 0)
    {
        return Convert.ToInt32(answer);
    }
    else
    {
        return -9999;
    }
}

void PrintSolution(string[] answerPath, int combinedValue)
{
    foreach (string result in answerPath)
    {
        Console.Write($"{result}");
    }
    Console.WriteLine($" Answer count {countSolved}, result: {combinedValue}, goal: {goal}");
}