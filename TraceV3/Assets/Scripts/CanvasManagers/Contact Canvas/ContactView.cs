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
   public Image _contactImage;
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
      Destroy( _contactImage.sprite); //destroy
      Destroy( _contactImage); //destroy
      _addButton = null;
      _removeButton = null;
   }
   
   public void UpdateContactInfo(IAddressBookContact contact)
   {
      _givenName.text = contact.FirstName + " "+ contact.LastName;
      _phoneNumber.text = contact.PhoneNumbers[0];
      
      contact.LoadImage((result, error) =>
      {
         var texture = result.GetTexture();
         if (texture)
         {
            var sprite = Sprite.Create(
               texture,
               new Rect(0, 0, texture.width, texture.height),
               new Vector2(0.5f, 0.5f));
            _contactImage.sprite = sprite;
            //_contactImage.sprite = CropTexture(texture);
         }
      });
      _addButton.onClick.RemoveAllListeners();
      _addButton.onClick.AddListener(OnContactButtonAddClicked);
      _removeButton.onClick.RemoveAllListeners();
      _removeButton.onClick.AddListener(OnRemoveClick);
   }

   private void OnContactButtonAddClicked()
   {
      HelperMethods.SendSMS(_phoneNumber.text, "What up! I've been using this app for the past week and its lowk rly fun you should join the beta! its exclusive 🎉 https://testflight.apple.com/join/B4j5DDbh");
   }

   private void OnRemoveClick()
   {
      print("On Remove Clicked");
      Destroy(gameObject);
   }
}
