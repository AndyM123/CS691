using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PasswordField : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<InputField>().inputType = InputField.InputType.Password;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
