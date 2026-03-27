using System;
using System.Collections.Generic;

[Serializable]
public class AuthData
{
    public string username;
    public string password;
}

[Serializable]
public class RegisterRequest
{
    public string username;
    public string password;
}

[Serializable]
public class LoginRequest
{
    public string username;
    public string password;
}

[Serializable]
public class UsersResponse
{
    public List<User> usuarios;
}

[Serializable]
public class UserResponse
{
    public User usuario;
}

[Serializable]
public class ScoreUpdate
{
    public string username;
    public UserData data;
}

[Serializable]
public class UpdateUserRequest
{
    public string username;
    public UserData data;
}

[Serializable]
public class AuthResponse
{
    public User usuario;
    public string token;
}

[Serializable]
public class User
{
    public string _id;
    public string username;
    public UserData data;
}

[Serializable]
public class UserData
{
    public int score;
}