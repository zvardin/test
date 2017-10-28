using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(BoxCollider2D))]
public class Draggable : MonoBehaviour
{
    private GAF.Core.GAFMovieClip movie;
    private BoxCollider2D collider;
    
    private void Awake() {
        movie = GetComponent<GAF.Core.GAFMovieClip>();
        tag = "draggable";
        collider = GetComponent<BoxCollider2D>();
        //testing out git
    }
    
    private void Update() {
        if(tag == "Untagged")
        {
            movie.stop();
            enabled = false;
            return;
        }

        if(!movie.isPlaying() && tag == "target")
        {
            movie.play();
        } else if(movie.isPlaying() && tag == "draggable")
        {
            movie.stop();
        }
        collider.enabled = !movie.isPlaying();
    }



}
