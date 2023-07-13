using System.Collections;
using System.Collections.Generic;
using Algolia.Search.Clients;
using Algolia.Search.Models.Search;
using UnityEngine;

public class AlgoliaManager : MonoBehaviour
{
    private SearchClient client;
    private SearchIndex index;
    // Start is called before the first frame update
    void Start()
    {
        InitAlgolia();
        SearchByUsername("tracetestinga1");
    }

    private void InitAlgolia(){
        client = new SearchClient("Q324B39BS7", "f5c161e91e27b4bd94cdf05a5bd53ef4");
        index = client.InitIndex("users");
    }


    private void SearchByUsername(string userName)
    {
       var result  =  index.Search<TraceData>(new Query(userName));
       Debug.LogError(result.Hits[0].email);
    }
    
}

public class TraceData
{
   public string objectID { get; set; }
   public string name{ get; set; }
   public int score{ get; set; }
   public int friendCount{ get; set; }
    public string phone{ get; set; }
    public bool isLogedIn{ get; set; }
    public bool isOnline{ get; set; }
    public string email{ get; set; }
    public string username{ get; set; }
    public string path{ get; set; }
}
