using Firebase.Auth;
using UnityEngine;

namespace CanvasManagers
{
    public class PhoneNumberCanvasController
    {
        private PhoneNumberTextCanavs _view;
        private PhoneAuthProvider provider;
        private string _verficationId;
        private uint _autoVerifyTimeOut = 60 * 1000;
        private string phoneNumberwithoutareacode = "";
        private string phoneNumber = "";
        public PhoneNumberCanvasController(PhoneNumberTextCanavs canvas)
        {
            _view = canvas;
        }

        public void Init()
        {
            _view._numberValidationView.gameObject.SetActive(true);
            _view.verifyNumberButton.onClick.AddListener(OnVerifyNumberClicked);
            _view._numberInputField.onValueChanged.AddListener(EditPhoneNumber);
            _view._numberValidationView._submitButton.onClick.AddListener(Varify_OTP);
            _view._numberValidationView.gameObject.SetActive(false);
            PlayerPrefs.SetInt("IsInvited", 0);
        }

        public void Uninitilise()
        {
            _view.verifyNumberButton.onClick.RemoveAllListeners();
            _view._numberInputField.onValueChanged.RemoveAllListeners();
            _view._numberValidationView._submitButton.onClick.RemoveAllListeners();
            _view._numberValidationView.gameObject.SetActive(true);

        }

        private void OnVerifyNumberClicked()
        {
            phoneNumber = _view._countryCodeDropdown.captionText.text.Trim()+_view._numberInputField.text.Trim();
            phoneNumberwithoutareacode = _view._numberInputField.text.Trim();

            provider = PhoneAuthProvider.GetInstance(FirebaseAuth.DefaultInstance);
            
            provider.VerifyPhoneNumber(phoneNumber, _autoVerifyTimeOut, null,
                verificationCompleted: (credential) => {
                    Debug.LogError("Credential :: "+ credential.Provider);
                    // Auto-sms-retrieval or instant validation has succeeded (Android only).
                    // There is no need to input the verification code.
                    // `credential` can be used instead of calling GetCredential().
                },
                verificationFailed: (error) => {
                    // The verification code was not sent.
                    Debug.LogError("Error :: "+error);

                    // `error` contains a human readable explanation of the problem.
                },
                codeSent: (id, token) => {
                    _verficationId = id;
                    Debug.LogError("ID ::"+id);
                    Debug.LogError("Token :: "+token);
                    Debug.LogError("OTP ID :"+_verficationId);
                    // Verification code was successfully sent via SMS.
                    // `id` contains the verification id that will need to passed in with
                    // the code from the user when calling GetCredential().
                    // `token` can be used if the user requests the code be sent again, to
                    // tie the two requests together.
                },
                codeAutoRetrievalTimeOut: (id) => {
                    Debug.LogError("Time Out ID :: "+id);

                    // Called when the auto-sms-retrieval has timed out, based on the given
                    // timeout parameter.
                    // `id` contains the verification id of the request that timed out.
                });

            ActiveValidationWindow(phoneNumber);
        }

        public void Varify_OTP()
        {
            _view.loadingSign.SetActive(true);
#if UNITY_EDITOR
            Debug.Log("is in UnityEditor Force Next Screen");
            //set phonen number without Verification
            _view.StartCoroutine(FbManager.instance.SetUserPhoneNumber(phoneNumber, (isSuccess) =>
            {
               
                Debug.Log("Set phone number callback");
                if (isSuccess)
                {
                    _view.StartCoroutine(FbManager.instance.IsUserListedInInvited(phoneNumberwithoutareacode, (callbackIsSuccess) =>
                    {
                        _view.loadingSign.SetActive(false);
                        if (callbackIsSuccess)
                        {
                            Debug.Log("User Invited: true");
                            FbManager.instance.AddInvitesToFriends(phoneNumberwithoutareacode);
                            FbManager.instance.AddInviteTracesToTracesReceived(phoneNumberwithoutareacode);
                        }
                        else
                            Debug.LogError("Failed to get user invite");
                        
                        //change screen only after check
                        ScreenManager.instance.ChangeScreenForwards("Username");
                    }));
                }
                else
                {
                    _view.loadingSign.SetActive(false);
                    Debug.LogError("Failed to update phone");
                }
            }));
            return;
#endif
            Debug.LogError("Verify_OTP Called");

            Credential credential =
                provider.GetCredential(_verficationId, _view._numberValidationView._verificationCode.text);
            var isValid = credential.IsValid();
            if (isValid)
            {
                _view.StartCoroutine(FbManager.instance.SetUserPhoneNumber(phoneNumber, (isSuccess) =>
                {
                    if (isSuccess)
                    {
                        _view.StartCoroutine(FbManager.instance.IsUserListedInInvited(phoneNumberwithoutareacode, (callbackIsSuccess) =>
                        {
                            if (callbackIsSuccess)
                            {
                                Debug.Log("User Invited: true");
                                FbManager.instance.AddInvitesToFriends(phoneNumberwithoutareacode);
                                FbManager.instance.AddInviteTracesToTracesReceived(phoneNumberwithoutareacode);
                            }
                            else
                                Debug.LogError("Failed to update phone");
                            _view.loadingSign.SetActive(false);
                            ScreenManager.instance.ChangeScreenForwards("Username");
                        }));
                    }
                    else
                    {
                        Debug.LogError("Failed to update phone");
                        _view.loadingSign.SetActive(false);
                    }
                }));
                Debug.LogError("Valid Credentials");
            }
            else
            {
                _view.loadingSign.SetActive(false);
                Debug.LogError("Invalid Credentials");
            }
        }

        private void ActiveValidationWindow(string number)
        {
            _view.validationScreen.SetActive(true);
            _view._numberValidationView._phoneNumberPreviewText.text = "It was sent to " + number;
        }
        
        
        public void EditPhoneNumber(string inputText)
        {
            var phoneNumber = inputText;
            _view._phoneNumberDisplay.text = phoneNumber;


            //first dash in phone number
            if (phoneNumber.Length > 3)
            {
                var phoneNumberVisual = _view._phoneNumberDisplay.text.Insert(3, "-");
                _view._phoneNumberDisplay.text = phoneNumberVisual;
            }

            if (phoneNumber.Length > 7)
            {
                var phoneNumberVisual = _view._phoneNumberDisplay.text.Insert(7, "-");
                _view._phoneNumberDisplay.text = phoneNumberVisual;
            }

            
            //second dash in phone number
            // if (phoneNumber.Length == 7 && lastLength != 8)
            // {
            //     var phoneNumberVisual = _inputField.text + "-";
            //     _inputField.text = phoneNumberVisual;
            //     _inputField.MoveToEndOfLine(false, false);
            // }else if (phoneNumber.Length == 7 && lastLength == 8) //if delete key pressed
            // {
            //     _phoneNumberDisplay.text = _inputField.text.Substring(0, _inputField.text.Length - 1);
            // }


            //once at length deselect input field
            if (phoneNumber.Length == 10)
            {
                _view._numberInputField.DeactivateInputField();
            }

            //keeps phone number length correct
            if (phoneNumber.Length > 10)
            {
                _view._numberInputField.text = _view._numberInputField.text.Substring(0, _view._numberInputField.text.Length - 1);
            }
        }

    }
}