
using System;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity.Editor;
using UnityEngine;
using UnityEngine.UI;
public class FireBaseManager : MonoBehaviour
{
    [Header("GUI Elements")]
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
    public InputField AlternativeEmailAddress;
    public InputField SupportInquiry;
    public GameObject RegistrationScreen;
    public string currentUser;
    public GameObject FoodItemListBase;
    public Text EditItemName;
    public InputField EditItemDesc;
    public InputField EditItemPrice;
    public GameObject LoadingScreenItemList;

    [Header("Food Item Inputs")]
    public InputField[] FoodItem;

    [Header("Restaurant Detail Inputs")]
    public InputField[] RestaurantDetails;

    // Start is called before the first frame update
    void Start()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://cs-691-project.firebaseio.com/");
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        UserInput = GameObject.Find("UserField").GetComponent<InputField>();
        PassInput = GameObject.Find("PassField").GetComponent<InputField>();
        InitializeFirebase();
    }

    public void ListFoodItems()
    {
        FirebaseDatabase.DefaultInstance.GetReference("/restaurants/" + newUser.UserId + "/Menu").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                // Handle the error...
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (DataSnapshot subInfo in snapshot.Children)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(InstantiateItemList(subInfo));
                }
                UnityMainThreadDispatcher.Instance().Enqueue(LoadingFinishedForItemList());
            }
        });
    }

    public void FillSpecificFoodItemFields(string key)
    {
        GetFoodItem(key);
    }

    public void DeleteFoodItem(string key)
    {
        DeleteFoodItemFromServer(key);
    }

    private IEnumerator LoadingFinishedForItemList()
    {
        LoadingScreenItemList.SetActive(false);
        yield return null;
    }

    private IEnumerator EditSpecificFoodItemFields(DataSnapshot subInfo)
    {
        EditItemName.text = subInfo.Key.ToString();
        EditItemPrice.text = subInfo.Child("Price").Value.ToString();
        EditItemDesc.text = subInfo.Child("Description").Value.ToString();
        yield return null;
    }

    private IEnumerator InstantiateItemList(DataSnapshot subInfo)
    {
        GameObject a = Instantiate(FoodItemListBase.transform.GetChild(0).gameObject, FoodItemListBase.transform);
        a.SetActive(true);
        a.GetComponent<Text>().text = subInfo.Key.ToString();
        yield return null;
    }

    public void GetFoodItem(string KEY)
    {
        FirebaseDatabase.DefaultInstance.GetReference("/restaurants/" + newUser.UserId + "/Menu/" + KEY).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                // Handle the error...
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                UnityMainThreadDispatcher.Instance().Enqueue(EditSpecificFoodItemFields(snapshot));
            }
        });
    }

    public void DeleteFoodItemFromServer(string KEY)
    {
        FirebaseDatabase.DefaultInstance.GetReference("/restaurants/" + newUser.UserId + "/Menu/" + KEY).RemoveValueAsync();
        FindObjectOfType<ListFoodItems>().OnEnable();
    }


    public void EditFoodItem()
    {
        string KEY = EditItemName.text;
        Dictionary<string, object> childUpdates = new Dictionary<string, object>();
        childUpdates["/restaurants/" + newUser.UserId + "/Menu/" + KEY + "/Description"] = EditItemDesc.text;
        childUpdates["/restaurants/" + newUser.UserId + "/Menu/" + KEY + "/Price"] = EditItemPrice.text;
        reference.UpdateChildrenAsync(childUpdates);
    }

    private void Update()
    {
        QualitySettings.vSyncCount = 1;
        if (AdminPanel.activeInHierarchy)
        {
            OutputText.gameObject.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            populateOrders();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            populateReservations();
        }
    }
    void UploadUserData()
    {
        Dictionary<string, object> childUpdates = new Dictionary<string, object>();
        childUpdates["/users/" + newUser.UserId] = "";
        reference.UpdateChildrenAsync(childUpdates);
    }

    public void SubmitSupportTicket()
    {
        Dictionary<string, object> childUpdates = new Dictionary<string, object>();
        
        if(AlternativeEmailAddress.text.Length > 0)
        {
            childUpdates["inquiries/" + newUser.UserId] = AlternativeEmailAddress.text + "      " + SupportInquiry.text;
        }
        else
        {
            childUpdates["inquiries/" + newUser.UserId] = currentUser + "      " + SupportInquiry.text;
        }

        reference.UpdateChildrenAsync(childUpdates);
        AlternativeEmailAddress.text = "";
        SupportInquiry.text = "";
    }

    public void AddOrder(string FoodTitle, string CustomerName)
    {
        Dictionary<string, object> childUpdates = new Dictionary<string, object>();
        childUpdates["/restaurants/" + newUser.UserId + "/ActiveOrders/" + "/" + FoodTitle] = CustomerName;
        reference.UpdateChildrenAsync(childUpdates);
    }

    public void UpdateSubscription()
    {
        Dictionary<string, object> childUpdates = new Dictionary<string, object>();
        var date = DateTime.Now;
        var nextSunday = date.AddDays(7 - (int)date.DayOfWeek);
        childUpdates["/restaurants/" + newUser.UserId + "/SubscriptionInfo/" + "ExpirationDate"] = nextSunday.ToString();
        reference.UpdateChildrenAsync(childUpdates);
    }

    public void populateReservations()
    {
        Dictionary<string, object> childUpdates = new Dictionary<string, object>();
        childUpdates["/restaurants/" + newUser.UserId + "/reservations/Table"] = "Table " + UnityEngine.Random.Range(0,99999).ToString();
        reference.UpdateChildrenAsync(childUpdates);
    }
    public void populateOrders()
    {
        Dictionary<string, object> childUpdates = new Dictionary<string, object>();
        childUpdates["/restaurants/" + newUser.UserId + "/orders/Order"] = "John Doe : " + UnityEngine.Random.Range(0, 99999).ToString() + "Pancakes";
        reference.UpdateChildrenAsync(childUpdates);
    }
    public void UpdateSubscriptionRatios(int admin, int kitchen, int customer)
    {
        Dictionary<string, object> childUpdates = new Dictionary<string, object>();
        childUpdates["/restaurants/" + newUser.UserId + "/SubscriptionInfo/Seats/" + "AdminSeats"] = admin;
        childUpdates["/restaurants/" + newUser.UserId + "/SubscriptionInfo/Seats/" + "KitchenSeats"] = kitchen;
        childUpdates["/restaurants/" + newUser.UserId + "/SubscriptionInfo/Seats/" + "CustomerSeats"] = customer;
        reference.UpdateChildrenAsync(childUpdates);
    }

    public void RetrieveSubscriptionRatios()
    {
        SubscriptionManager sub = FindObjectOfType<SubscriptionManager>();
        FirebaseDatabase.DefaultInstance.GetReference("/restaurants/" + newUser.UserId + "/SubscriptionInfo/Seats").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                // Handle the error...
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (DataSnapshot subInfo in snapshot.Children)
                {
                    if (subInfo.Key.ToString() == "AdminSeats")
                        sub.numAdmins = int.Parse(subInfo.Value.ToString());

                    if (subInfo.Key.ToString() == "KitchenSeats")
                        sub.numKitchens = int.Parse(subInfo.Value.ToString());

                    if (subInfo.Key.ToString() == "CustomerSeats")
                        sub.numCustomers = int.Parse(subInfo.Value.ToString());
                }
            }
        });
    }

    public class Order
    {
        public string foodname;
        public string price;

        public Order(string food, string theprice)
        {
            this.foodname = food;
            this.price = theprice;
        }
    }
    public List<IDictionary> currOrders = new List<IDictionary>();
    public void RetrieveActiveOrders()
    {

        FirebaseDatabase.DefaultInstance.GetReference("/restaurants/" + newUser.UserId + "/ActiveOrders").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                // Handle the error...
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                foreach (DataSnapshot order in snapshot.Children)
                {
                    print(order.Key.ToString());
                    print(order.Value.ToString());
                }
            }
        });

    }

    public DateTime Next(DateTime from, DayOfWeek dayOfWeek)
    {
        int start = (int)from.DayOfWeek;
        int target = (int)dayOfWeek;
        if (target <= start)
            target += 7;
        return from.AddDays(target - start);
    }

    public void AddToMenu()
    {
        Dictionary<string, object> childUpdates = new Dictionary<string, object>();
        childUpdates["/restaurants/" + newUser.UserId + "/Menu/" + FoodItem[0].text + "/Price"] = FoodItem[2].text;
        childUpdates["/restaurants/" + newUser.UserId + "/Menu/" + FoodItem[0].text + "/Description"] = FoodItem[1].text;
        reference.UpdateChildrenAsync(childUpdates);
        FoodItem[0].text = "";
        FoodItem[1].text = "";
        FoodItem[2].text = "";
    }

    DatabaseReference reference;
    private void InitializeFirebase()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus != Firebase.DependencyStatus.Available)
            {
                UnityEngine.Debug.LogError(System.String.Format(
                    "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            }
        });
    }
    private bool _SignedInFlag;
    private Firebase.Auth.FirebaseUser newUser;


    private IEnumerator ProgressInterface(GameObject prevMenu, GameObject nextMenu)
    {
        nextMenu.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        prevMenu.SetActive(false);
        UpdateSubscription();
        //AddOrder("Cake", "Jon Doe");
        yield return 0;
    }

    private bool accCreatedFlag = false;
    async public void CreateAcc()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(SetText(Color.gray, "Loading..."));
        await auth.CreateUserWithEmailAndPasswordAsync(Create_User.text, Create_Pass.text).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(SetText(Color.red, "Failed to Create Account!"));
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(printError(task.Exception.ToString()));
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // Firebase user has been created.
            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);
            accCreatedFlag = true;

            Dictionary<string, object> childUpdates = new Dictionary<string, object>();
            childUpdates["/restaurants/" + newUser.UserId + "/SubscriptionInfo/Seats/" + "AdminSeats"] = 1;
            childUpdates["/restaurants/" + newUser.UserId + "/SubscriptionInfo/Seats/" + "KitchenSeats"] = 1;
            childUpdates["/restaurants/" + newUser.UserId + "/SubscriptionInfo/Seats/" + "CustomerSeats"] = 1;
            reference.UpdateChildrenAsync(childUpdates);
        });
        if (accCreatedFlag)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(SetText(Color.green, "User Created!"));
            RegistrationScreen.GetComponent<Animator>().SetTrigger("Off");
            LoginPanel.SetActive(true);
        }
        accCreatedFlag = false;
    }

    public void SignOut()
    {
        _SignedInFlag = false;
        auth.SignOut();
    }
    IEnumerator SetText(Color color, string Value)
    {
        OutputText.gameObject.SetActive(false);
        OutputText.gameObject.SetActive(true);
        OutputText.text = Value;
        OutputText.color = color;
        yield return 0;
    }



    private string RecentError;
    IEnumerator printError(string Exception)
    {
        if (Exception.Contains("badly formatted"))
            RecentError = "Please enter a proper email";
        if (Exception.Contains("email address must be provided"))
            RecentError = "Email field cannot be blank";
        if (Exception.Contains("given password is invalid"))
            RecentError = "Password must be 6 characters or longer";
        if (Exception.Contains("address is already in use by another account"))
            RecentError = "That email address is already in use";
        if (Exception.Contains("password must be provided"))
            RecentError = "Password field cannot be blank";
        UnityMainThreadDispatcher.Instance().Enqueue(SetText(Color.red, RecentError));
        yield return 0;
    }
    async public void SignIn()
    {

        UnityMainThreadDispatcher.Instance().Enqueue(SetText(Color.gray, "Loading..."));
        await auth.SignInWithEmailAndPasswordAsync(UserInput.text, PassInput.text).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                _SignedInFlag = false;
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                _SignedInFlag = false;
                return;
            }

            newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
            _SignedInFlag = true;
        });
        if (_SignedInFlag)
        {
            currentUser = UserInput.text;
            OutputText.gameObject.SetActive(false);
            UnityMainThreadDispatcher.Instance().Enqueue(ProgressInterface(LoginPanel, AdminPanel));

        }
        else
        {
            UnityMainThreadDispatcher.Instance().Enqueue(SetText(Color.red, "Please enter valid credentials"));
        }
    }
}
