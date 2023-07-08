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
            _contactImage.sprite = CropTexture(texture);
         }
      });
      _addButton.onClick.RemoveAllListeners();
      _addButton.onClick.AddListener(OnContactButtonAddClicked);
      _removeButton.onClick.RemoveAllListeners();
      _removeButton.onClick.AddListener(OnRemoveClick);
   }

   private Sprite CropTexture(Texture2D texture)
   {
      Sprite croppedSprite = null;
      Texture2D originalTexture = texture;
      int squareSize = Mathf.Min(originalTexture.width, originalTexture.height);
      Rect croppingRect = new Rect((originalTexture.width - squareSize) / 2, (originalTexture.height - squareSize) / 2, squareSize, squareSize);
      Texture2D croppedTexture = new Texture2D((int)croppingRect.width, (int)croppingRect.height);
      croppedTexture.SetPixels(originalTexture.GetPixels((int)croppingRect.x, (int)croppingRect.y, (int)croppingRect.width, (int)croppingRect.height));
      croppedTexture.Apply();
      croppedSprite = Sprite.Create(croppedTexture, new Rect(0, 0, croppedTexture.width, croppedTexture.height), new Vector2(0.5f, 0.5f));
      return croppedSprite;
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
