using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public enum Relationship{
    Other,
    Friend,
    BestFriend,
    Following,
    SuperUser,
}

[Serializable]
public class FriendModel : IEquatable<FriendModel>
{
    public Relationship relationship;
    public string friendID;
    public bool Equals(FriendModel other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return friendID == other.friendID;
    }

    public override int GetHashCode()
    {
        return (friendID != null ? friendID.GetHashCode() : 0);
    }
}
