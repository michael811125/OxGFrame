using UnityEngine;
using System.Collections;

public class TextMover: MonoBehaviour {
    
	[SerializeField]
	float speed = 30f;

	// Update is called once per frame
	void Update()
	{
		float h, v;

		#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IPHONE)
		GetMobile(out h, out v);
		#else
		GetDesktop(out h, out v);
		#endif

		transform.position = (transform.position + new Vector3(h, v) * speed * Time.deltaTime);
	}

	void GetMobile(out float h, out float v)
	{
		if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
		{
			Vector2 delta = Input.GetTouch(0).deltaPosition;

			h = delta.x;
			v = delta.y;
		}
		else
		{
			h = v = 0f;
		}
	}

	void GetDesktop(out float h, out float v)
	{
		h = Input.GetAxis("Horizontal");
		v = Input.GetAxis("Vertical");
	}
}
