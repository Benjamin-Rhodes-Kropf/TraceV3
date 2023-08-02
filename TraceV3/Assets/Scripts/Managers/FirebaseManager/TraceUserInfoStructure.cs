using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraceUserInfoStructure {
    //they must be public for json utility conversion
    public string batch;
    public string username;
    public string name;
    public string email;
    public string phone;
    public string photo;
    public string createdDate;
    public bool isInQue;
        

    public TraceUserInfoStructure(string batch, string username, string name, string photo, string email, string phone, string createdDate)
    {
        this.batch = batch;
        this.username = username;
        this.name = name;
        this.photo = photo;
        this.email = email;
        this.phone = phone;
        this.createdDate = createdDate;
        isInQue = true;
    }
}
