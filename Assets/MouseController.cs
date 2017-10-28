using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour {

    public float speed;

    private GameObject target;
    private GameObject targetDrop;
    private bool isDragging = false;

	void Update () {
        CheckClick();
        UpdateMovement();
    }

    private void UpdateMovement() {
        var x = Input.GetAxis("Horizontal") + Input.GetAxis("Mouse X");
        var y = Input.GetAxis("Vertical") + Input.GetAxis("Mouse Y");

        transform.position += new Vector3(x * speed, y * speed, 0f);

        Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);
        pos.x = Mathf.Clamp01(pos.x);
        pos.y = Mathf.Clamp01(pos.y);
        transform.position = Camera.main.ViewportToWorldPoint(pos);

        if (isDragging)
        {
            target.transform.position = transform.position;
        }
    }

    private void CheckClick() {
        if (target == null)
        {
            return;
        }
        
        if (Input.GetButtonDown("Fire1"))
        {
            if (isDragging)
            {
                Drop();
            }
            else if(target != null)
            {
                Drag();
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Drop();
        }
    }

    private void Drag() {
        isDragging = true;
        target.tag = "target";
    }

    private void Drop() {
        if (targetDrop != null)
        {
            target.transform.position = targetDrop.transform.position;
            target.tag = "Untagged";
            
        }
        else
        {
            target.tag = "draggable";
        }
        target = null;
        isDragging = false;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        Debug.Log("TRIGGER on " + collision.name);
        if (isDragging && collision.tag == "dropspot" && collision.name == target.name)
        {
            targetDrop = collision.gameObject;
        } else if (!isDragging && collision.tag == "draggable")
        {
            target = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        var o = collision.gameObject;
        if (targetDrop != null && targetDrop == o)
        {
            targetDrop = null;
        }
        else if (!isDragging && target != null && target == o)
        {
            target = null;
        }
    }
}
