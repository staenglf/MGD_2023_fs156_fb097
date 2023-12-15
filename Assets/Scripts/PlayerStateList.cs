using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Generates the states a Player can have
public class PlayerStateList : MonoBehaviour
{
    public bool jumping = false;
    public bool dashing = false;
    public bool recoilingX, recoilingY;
    public bool lookingRight;
    public bool invincible;
}