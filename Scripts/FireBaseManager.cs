using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity.Editor;
using UnityEngine;
using UnityEngine.UI;
public class FireBaseManager : MonoBehaviour {
    [Header ("GUI Elements")]
    public InputField Create_User;
    public InputField Create_Pass;
    public Text OutputText;
    public GameObject LoginPanel;
    public GameObject AdminPanel;
    public Button Login;
    private string UserKey;
    private FirebaseAuth auth;
    private InputField UserInput;
    private InputField PassInput;

    // Start is called before the first frame update
    void Start () {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl ("https://cs-691-project.firebaseio.com/");
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        UserInput = GameObject.Find ("UserField").GetComponent<InputField> ();
        PassInput = GameObject.Find ("PassField").GetComponent<InputField> ();
        InitializeFirebase ();

    }

    private void Update () {
        if (Input.GetKeyDown (KeyCode.U)) {
            UploadUserData ();
        }
    }
    void UploadUserData () {
        Dictionary<string, object> childUpdates = new Dictionary<string, object> ();
        childUpdates["/restaurants/" + newUser.UserId] = "";
        reference.UpdateChildrenAsync (childUpdates);
    }

    DatabaseReference reference;
    private void InitializeFirebase () {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync ().ContinueWith (task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available) {
                InitializeFirebase ();
            } else {
                UnityEngine.Debug.LogError (System.String.Format (
                    "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            }
        });

    }
    private bool _SignedInFlag;
    private Firebase.Auth.FirebaseUser newUser;

    private void ProgressInterface (GameObject prevMenu, GameObject nextMenu) {
        prevMenu.SetActive (false);
        nextMenu.SetActive (true);
    }
    private bool accCreatedFlag;
    async public void CreateAcc () {
        await auth.CreateUserWithEmailAndPasswordAsync (Create_User.text, Create_Pass.text).ContinueWith (task => {
            if (task.IsCanceled) {
                OutputText.text = "Failed!";
                OutputText.color = Color.red;
                Debug.LogError ("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                OutputText.text = "Failed!";
                OutputText.color = Color.red;
                Debug.LogError ("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // Firebase user has been created.
            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat ("Firebase user created successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
        });
        OutputText.text = "User Created!";
        OutputText.color = Color.green;
        StartCoroutine(ResetText());
    }

    IEnumerator ResetText(){
        yield return new WaitForSeconds(5);
        OutputText.text = "";
    }
    async public void SignIn () {
        await auth.SignInWithEmailAndPasswordAsync (UserInput.text, PassInput.text).ContinueWith (task => {

            if (task.IsCanceled) {
                Debug.LogError ("SignInWithEmailAndPasswordAsync was canceled.");
                _SignedInFlag = false;
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError ("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                _SignedInFlag = false;
                return;
            }

            newUser = task.Result;
            Debug.LogFormat ("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
            _SignedInFlag = true;
        });
        if (_SignedInFlag) {
            ProgressInterface (LoginPanel, AdminPanel);
            OutputText.text = "Success!";
            OutputText.color = Color.green;
        } else {
            OutputText.text = "Failed Login!";
            OutputText.color = Color.red;
        }
        StartCoroutine(ResetText());
    }
}