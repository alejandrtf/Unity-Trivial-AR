// Represent User model to parse FirebaseDatabase User reference

public class User {

    public string Email { get; set; }
    public string UserIdAuth { get; set; }
    public string UserIdDatabase { get; set; }
    public string Password { get; set; }

    public User(string email, string password, string userIdAuth)
    {
        this.Email = email;
        this.UserIdAuth = userIdAuth;
        this.Password = Seguridad.Encriptar(password);
    }

    public User(string email, string password, string userIdAuth,string userIdDatabase)
    {
        this.Email = email;
        this.UserIdAuth = userIdAuth;
        this.Password = Seguridad.Encriptar(password);
        this.UserIdDatabase = userIdDatabase;
    }


    /*Constructor que crea un usuario a partir de un UserScore*/
    public User(UserScore userScore)
    {
        this.Email = userScore.UserEmail;
        this.UserIdAuth = userScore.UserIdAuth;
        this.Password = "";
        this.UserIdDatabase = this.UserIdAuth;
    }
}
