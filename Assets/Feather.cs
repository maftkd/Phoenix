using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feather : MonoBehaviour
{
	public Transform _sfx;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void EquipFeather(){
		Debug.Log("Equipping feather");
		Instantiate(_sfx,transform.position,Quaternion.identity);
		GameObject p = GameObject.FindGameObjectWithTag("Player");
		Bird b = p.GetComponent<Bird>();
		Transform parent = transform.parent;
		b.EquipFeather(transform);
		Destroy(parent.gameObject);
	}
}
