using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class FriendModel : IEquatable<FriendModel>
{
    public enum RelationShipType
    {
        Friend,
        BestFriend,
        User,
        SuperUser,
        Following
    }
    
    public RelationShipType relationship = RelationShipType.Friend;
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
