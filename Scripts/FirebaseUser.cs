using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseUser : MonoBehaviour
{

   public class Home {
      string RestaurantName;
      string Password;
      int RestaurantUserID;
      int AnonymousUserID;
      enum UserType {
          owner, kitchen_Staff
      }
      

   }
    
   public class RestaurantOwner : Home {
       string PhysAddress;
       string PhoneNumber;
       string[] CompletedOrders;
       string[] QueuedOrders;
 
       string[] Menu;
   }

   public class KitchenStaff : RestaurantOwner {
        string[] CompletedOrders;
        string[] QueuedOrders;
   }

   public class AnonymousUser {
       int AnonymousUserID;
       int CreditCardNum;
       int CVV;
       string BillingAddress;
       string State;
   }

}
