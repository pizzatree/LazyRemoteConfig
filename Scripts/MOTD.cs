using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MOTD : MonoBehaviour
{
    private void Start() => GetComponent<Text>().text = Settings.NiceMessage;
}
