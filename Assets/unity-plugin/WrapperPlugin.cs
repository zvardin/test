using UnityEngine;
using System.Collections;
using Animate;
using System.Collections.Generic;

public class WrapperPlugin : MonoBehaviour
{
    List<MainAnimation> animations = new List<MainAnimation>();
    //float currentTime = 0;
    //int currentFrame = 0;
    //float currentTimeforEnable = 0;
    //int currentFrameforEnable = 0;
    void Start() {
        //Camera mainCam = Camera.main;
        //mainCam.transform.position = new Vector3(-200, 200, -1);
        //mainCam.orthographicSize = 550;
        AddAnimation("n");
        AddAnimation("a");
        AddAnimation("d");
        //object1.LayerStateObjDICT[object1.mainGameObject.LayerStateName].anim.Stop("");
        //object1.CreateGameObject(object1.mainGameObjectName, null, "");
        /* If there are frame levels in animation, mention it as object1.PlayAnimation("walk"); */
        //object1.PlayAnimation("walk");
    }

    private void AddAnimation(string folder) {
        var object1 = new MainAnimation(folder);
        object1.Start1();
        object1.PlayAnimation("");
        animations.Add(object1);
    }

    void FixedUpdate()
    {
        foreach(var o in animations) {
            var currentTime = o.currentTime;
            var currentFrame = o.currentFrame;
            //var currentTimeforEnable = o.currentTimeforEnable;
            //var currentFrameforEnable = o.currentFrameforEnable;
            
            foreach (var ls in o.LayerStateObjDICT)
            {
                if (ls.Value.spriteName != null)
                {
                    if (ls.Value.sortingorder.ContainsKey(currentFrame))
                    {
                        ls.Value.gobj.GetComponent<SpriteRenderer>().sortingOrder = -ls.Value.sortingorder[currentFrame];

                    }
                }
            }

            
            
            foreach (var ls in o.LayerStateObjDICT)
            {
                if (ls.Value.spriteName != null)
                    ls.Value.gobj.GetComponent<SpriteRenderer>().enabled = ls.Value.SpriteEnabled[currentFrame];
                if (ls.Value.color.ContainsKey(currentFrame))
                {
                    ls.Value.gobj.GetComponent<SpriteRenderer>().color = ls.Value.color[currentFrame];
                }
            }
            //o.currentTimeforEnable += 1;
            //o.currentFrameforEnable = (currentFrameforEnable + 1) % o.MainTimeline.TimelineDuration;

            o.currentTime += Time.fixedDeltaTime;
            o.currentFrame = Mathf.FloorToInt(o.currentTime*o.framerate) % o.MainTimeline.TimelineDuration;

        }
    }
    
}