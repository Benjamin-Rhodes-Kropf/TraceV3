using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraceUserInfoStructure {
    //they must be public for json utility conversion
    public string username;
    public string name;
    public string email;
    public string phone;
    public string photo;
    public bool isOnline;
    public bool isLogedIn;
    
    public TraceUserInfoStructure()
    {
        isOnline = false;
        isLogedIn = true;
    }

    public TraceUserInfoStructure(string username, string name, string photo, string email, string phone) {
        this.username = username;
        this.name = name;
        this.photo = photo;
        this.email = email;
        this.phone = phone;
        isLogedIn = true;
    }
}
