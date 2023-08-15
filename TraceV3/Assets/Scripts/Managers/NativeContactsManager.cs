using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;

public class NativeContactsManager : MonoBehaviour
{
    public List<SampleContacts> _SampleContacts;

    private Action<IAddressBookContact[]> OnSuccessfullyReadContacts;
    private Action<string> OnFailedReadContacts;
    
    public static NativeContactsManager s_Instance;

    private void Awake()
    {
        s_Instance = this;
    }
    
    public void LoadUserContacts(Action<IAddressBookContact[]> OnSuccess, Action<string> OnFailed)
    {
        // Reset Callbacks
        OnSuccessfullyReadContacts = null;
        OnFailedReadContacts = null;

        //Assign New Callbacks
        OnSuccessfullyReadContacts += OnSuccess;
        OnFailedReadContacts += OnFailed;
        
        AddressBookContactsAccessStatus status = AddressBook.GetContactsAccessStatus();
        AddressBook.RequestContactsAccess(callback: OnRequestContactsAccessFinish);
    }
    private void OnRequestContactsAccessFinish(AddressBookRequestContactsAccessResult result, Error error)
    {
        Debug.Log("Request for contacts access finished.");
        Debug.Log("Address book contacts access status: " + result.AccessStatus);
        if (result.AccessStatus == AddressBookContactsAccessStatus.Denied)
        {
            //Todo:This Does not work
            Debug.Log("Prompt Again");
            OnFailedReadContacts("Prompt Again for access");
            AddressBook.ReadContactsWithUserPermission(callback: null);
        }

        if (result.AccessStatus == AddressBookContactsAccessStatus.Authorized)
        {
            AddressBook.ReadContactsWithUserPermission(OnReadContactsFinish);
        }
        
    }
        
    private void OnReadContactsFinish(AddressBookReadContactsResult result, Error error)
    {
        if (error == null)
        {
            var contacts    = result.Contacts;
            Debug.Log("Request to read contacts finished successfully.");
            var isContactAvailable = contacts.Length > 0;
            
            
            if (isContactAvailable)
                OnSuccessfullyReadContacts(contacts);
            else
                OnFailedReadContacts("No Contacts Available");
        }
        else
        {
            OnFailedReadContacts("Request to read contacts failed with error. Error: " + error);
        }
    }
}
[System.Serializable]
public class SampleContacts
{
    public string _name;
    public string _phone;
    public Texture2D _profilePicture;
}
