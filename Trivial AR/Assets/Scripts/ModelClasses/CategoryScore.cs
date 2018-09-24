
using System;

[Serializable]
public class CategoryScore  {

    public string UserIdAuth;  //el id de FirebaseAuth
    public string UserEmail; //su email, ya que no guardo el user name
    public string UserId_IdCat; // representa user_cat. Por ejemplo:  pepe_01
    public string CategoryName;
    public string CategoryId;
    public int TotalCategoryQuestions;  //indica el total de preguntas respondidas de esa categoría (acertadas y/o falladas)
    public int TotalOkCategoryQuestions; //indica el total de preguntas acertadas de esa categoría

    public CategoryScore(string userIdAuth, string userEmail, string userId_IdCat, string categoryName, string categoryId, int totalCategoryQuestions, int totalOkCategoryQuestions)
    {
        UserIdAuth = userIdAuth;
        UserEmail = userEmail;
        UserId_IdCat = userId_IdCat;
        CategoryName = categoryName;
        CategoryId = categoryId;
        TotalCategoryQuestions = totalCategoryQuestions;
        TotalOkCategoryQuestions = totalOkCategoryQuestions;
    }
}
