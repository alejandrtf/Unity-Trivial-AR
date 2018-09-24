
using System;

[Serializable]
public class UserScore{

    public string UserIdAuth;
    public string UserEmail;
    public int Score;
    public int CurrentLevel;
    public int TotalWonGames;
    public int TotalQuestions;
    public int TotalOkQuestions;
    public int TotalOkConsecutiveQuestions;

    public UserScore(string userIdAuth, string userEmail, int score, int currentLevel, int totalWonGames, int totalQuestions, int totalOkQuestions, int totalOkConsecutiveQuestions)
    {
        UserIdAuth = userIdAuth;
        UserEmail = userEmail;
        Score = score;
        CurrentLevel = currentLevel;
        TotalWonGames = totalWonGames;
        TotalQuestions = totalQuestions;
        TotalOkQuestions = totalOkQuestions;
        TotalOkConsecutiveQuestions = totalOkConsecutiveQuestions;
    }
}
