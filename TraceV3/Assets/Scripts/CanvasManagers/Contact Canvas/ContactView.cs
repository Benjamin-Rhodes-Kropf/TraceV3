using System;
using System.Collections;
using System.Collections.Generic;
using CanvasManagers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxelBusters.EssentialKit;


public class ContactView : MonoBehaviour
{
   public TMP_Text _givenName;
   public TMP_Text _phoneNumber;
   public RawImage _contactImage;
   public Button _addButton;
   public Button _removeButton;
   
   public void OnDestroy()
   {
      // Unsubscribe from event handlers
      _addButton.onClick.RemoveAllListeners();
      _removeButton.onClick.RemoveAllListeners();
      
      // Release object references
      _givenName = null;
      _phoneNumber = null;
      _contactImage = null;
      Destroy( _contactImage.texture); //destroy
      Destroy( _contactImage); //destroy
      _addButton = null;
      _removeButton = null;
   }
   
   public void UpdateContactInfo(IAddressBookContact contact)
   {
      _givenName.text = contact.FirstName + " "+ contact.LastName;
      _phoneNumber.text = contact.PhoneNumbers[0];
      _contactImage.texture = null;

      var persistentData = PersistentStorageHandler.s_Instance.GetTextureFromPersistentStorage("contacts", contact.PhoneNumbers[0]);
      if (persistentData.updateImage)
      {
         contact.LoadImage((result, error) =>
         {
            var texture = result.GetTexture();
            if (texture)
            {
               var newTexture = PersistentStorageHandler.s_Instance.CropImage(texture);
               SaveToPersistentStorage(newTexture);
               _contactImage.texture = newTexture;
            }
         });
      }
      else
      {
         _contactImage.texture = persistentData.texture;
      }

      _addButton.onClick.RemoveAllListeners();
      _addButton.onClick.AddListener(OnContactButtonAddClicked);
      _removeButton.onClick.RemoveAllListeners();
      _removeButton.onClick.AddListener(OnRemoveClick);
   }

   IEnumerator SaveToPersistentStorage(Texture2D texture2D)
   {
      texture2D.Apply();
      yield return new WaitForEndOfFrame();
      PersistentStorageHandler.s_Instance.SaveTextureToPersistentStorage(texture2D,"contacts",_phoneNumber.text);
   }
   
   
//Todo: Just for Testing  Purpose
   public void ContactInfoUpdate(string Name,string  ContactNumber, Texture2D _texture)
   {
      _givenName.text = Name;
      _phoneNumber.text = ContactNumber;
      _contactImage.texture = _texture;
   }
   
   private void OnContactButtonAddClicked()
   {
      StartCoroutine(FbManager.instance.SendInvite(_phoneNumber.text));
      FbManager.instance.AnalyticsOnAddContact(_phoneNumber.text);
      HelperMethods.SendSMS(_phoneNumber.text, "What up! add me on trace! it's super fun. https://apps.apple.com/us/app/trace-leave-a-trace/id6450400543");
   }

   private void OnRemoveClick()
   {
      print("On Remove Clicked");
   }
}
