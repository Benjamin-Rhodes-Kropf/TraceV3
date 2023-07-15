using System.Collections;
using System.Collections.Generic;
using Algolia.Search.Clients;
using Algolia.Search.Models.Search;
using UnityEngine;

public class AlgoliaManager : MonoBehaviour
{
    private SearchClient client;
    private SearchIndex index;
    public static AlgoliaManager instance;
    
    
    // Start is called before the first frame update
    void Start()
    {
        InitAlgolia();
        SearchByUsername("Nadeem");
    }

    private void InitAlgolia()
    {
        instance = this;
        client = new SearchClient("Q324B39BS7", "f5c161e91e27b4bd94cdf05a5bd53ef4");
        index = client.InitIndex("users");
    }


    private void SearchByUsername(string userName)
    {
       var result  =  index.Search<UserModel>(new Query(userName));
       Debug.LogError(result.Hits[0].Email);
    }

    public List<UserModel> SearchUser(string parameter)
    {
        var result  =  index.Search<UserModel>(new Query(parameter));
        return result.Hits;
    }
    
    
}
//
// public class TraceData
// {
//     public string objectID { get; set; }
//     public string name{ get; set; }
//     public int score{ get; set; }
//     public int friendCount{ get; set; }
//     public string phone{ get; set; }
//     public string phoneNumber { get; set; }
//     public bool isLogedIn{ get; set; }
//     public bool isOnline{ get; set; }
//     public string email{ get; set; }
//     public string username{ get; set; }
//     public string path{ get; set; }
//     public string photoUrl { get; set; }
// }
