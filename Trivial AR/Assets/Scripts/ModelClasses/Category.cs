// Represent Category model to parse FirebaseDatabase Category and CategoryQuestions reference

using System.Collections.Generic;

public class Category
{

    public string IdCategory { get; set; }
    public string Name { get; set; }
    public string urlImage { get; set; }
    public int QuestionsTotalCat { get; set; }
    private List<string> questionsList = new List<string>(); //contiene el id de las preguntas sólo
    public List<string> QuestionsList
    {
        get { return questionsList; }
        set { questionsList = value; }
    }


    public Category(string IdCategory, int QuestionsTotalCat)
    {
        this.IdCategory = IdCategory;
        this.QuestionsTotalCat = QuestionsTotalCat;
        this.Name = getNameCategory(IdCategory);
    }

    public Category(string IdCategory)
    {
        this.IdCategory = IdCategory;
        this.Name = getNameCategory(IdCategory);

    }


    public void AddQuestionId(string idQuestion)
    {
        this.questionsList.Add(idQuestion);
    }




    private string getNameCategory(string idCategory)
    {

        switch (idCategory)
        {
            case "art": return "Arte";
            case "b&g": return "Biología y Geología";
            case "cel": return "Celebrities";
            case "s&t": return "Ciencia y Tecnología";
            case "spo":return "Otros deportes";
            case "phi":return "Filosofía";
            case "foot": return "Fútbol";
            case "p&c": return "Física y Química";
            case "geo":return "Geografía";
            case "his":return "Historia";
            case "eng": return "Inglés";
            case "l&l":return "Literatura y Lengua";
            case "mat":return "Matemáticas";
            case "mus":return "Música";
            case "tv": return "Cine y TV";
            case "vid":return "Videojuegos";
            default:return "";
        }

    }

}
