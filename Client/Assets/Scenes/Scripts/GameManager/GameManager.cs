using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(this.gameObject); // ±£Ö¤¿ç³¡¾°
    }
}
