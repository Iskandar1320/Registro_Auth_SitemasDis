using System;
using System.Collections.Generic;

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
public class UpdateUserRequest
{
    public string username;
    public UserUpdateData data;
}

[Serializable]
public class UserUpdateData
{
    public int score;
}

[Serializable]
public class User
{
    public string _id;
    public string uid;
    public string username;
    public string state;
    public int score;
}

[Serializable]
public class AuthResponse
{
    public User usuario;
    public string token;
}

[Serializable]
public class UserResponse
{
    public User usuario;
}

[Serializable]
public class UsersResponse
{
    public List<User> usuarios;
}