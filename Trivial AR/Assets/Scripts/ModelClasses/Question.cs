// Represent Question model to parse FirebaseDatabase Question reference


using System;

public class Question {

    public string IdQuestion {get;set;}
    public string IdCategory { get; set; }
    public string QuestionText { get; set; }
    public string ChoiceA { get; set; }
    public string ChoiceB { get; set; }
    public string ChoiceC { get; set; }
    public string ChoiceD { get; set; }
    public string CorrectAnswer { get; set; }
    public bool IsAudioQuestion { get; set; }
    public bool IsImageQuestion { get; set; }
    public string QuestionImageUrl { get; set; }
    public string QuestionAudioUrl { get; set; }


    public Question(string id, string idCat, string text, string a, string b, string c, string d, string correct, bool isAudio, bool isImage)
    {
        this.IdQuestion = id;
        this.IdCategory = idCat;
        this.QuestionText = text;
        this.ChoiceA = a;
        this.ChoiceB = b;
        this.ChoiceC = c;
        this.ChoiceD = d;
        this.CorrectAnswer = correct;
        this.IsAudioQuestion = isAudio;
        this.IsImageQuestion = isImage;
        
    }


    public override string ToString()
    {
        return String.Format("IdQuestion:{0}, IdCategory:{1}, QuestionText:{2}, OpcionA:{3}, OpciónB:{4}, OpciónC:{5}, OpcionD:{6}, Correcta: {7}, IsAudio: {8}, IsImage: {9}", IdQuestion, IdCategory, QuestionText, ChoiceA, ChoiceB, ChoiceC,ChoiceD,CorrectAnswer,IsAudioQuestion,IsImageQuestion);
    }



    public string GetCorrectAnswer(string idAnswer)
    {
        switch (idAnswer)
        {
            case "ChoiceA":return this.ChoiceA;
            case "ChoiceB":return this.ChoiceB;
            case "ChoiceC": return this.ChoiceC;
            case "ChoiceD":return this.ChoiceD;
            default:return "";
        }
    }

}
