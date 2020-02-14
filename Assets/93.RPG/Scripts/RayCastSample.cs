using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCastSample : MonoBehaviour
{
    // raycast sample 
    public GUISkin myskin = null;
    private Vector3 PickPosition = Vector3.zero;
    private void FixedUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit raycastHit = new RaycastHit();
        // 충돌이 되었다면.
        //if (physics.raycast(ray, out raycasthit, mathf.infinity) == true)
        //{
        //    pickposition = raycasthit.point;
        //}

        // background layer만 검출
        // picklayer
        int BackgroundLayer = LayerMask.NameToLayer("BackGround");
        int PickLayer = 1 << BackgroundLayer;
        if (Physics.Raycast(ray, out raycastHit, Mathf.Infinity, PickLayer) == true)
        {
            PickPosition = raycastHit.point;
        }
    }

    private void OnGUI()
    {
        GUI.color = Color.black;
        GUI.skin = myskin;
        GUILayout.Label("point : " + PickPosition.ToString());
    }
}
